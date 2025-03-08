using api.Data;
using api.DTOs;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Net;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectsController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(TickItDbContext context, ILogger<ProjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectDTO>), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get all projects")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;

                var projects = await _context.Projects
                    .Where(p => p.Owner.GitHubId == userId)
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        Owner = new UserDTO { ID = p.Owner.Id, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                            .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId })
                            .ToList(),
                    })
                    .ToListAsync();

                if (projects == null || !projects.Any()) return NotFound(new { message = "No projects found" });

                return Ok(projects);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError(" fetching projects", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get a project by project ID")]
        public async Task<ActionResult<ProjectDTO>> GetProjectById(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;

                var project = await _context.Projects
                    .Where(p => p.Id == id)
                    .Select(p => new ProjectWithTasksDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        Owner = new UserDTO { ID = p.Owner.Id, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                            .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId })
                            .ToList(),
                        Tasks = p.Tasks
                            .Select(t => new TaskDTO
                            {
                                AssigneeId = t.AssigneeId,
                                TaskName = t.TaskName,
                                PriorityId = t.PriorityId,
                                ProjectId = t.ProjectId,
                                TaskDescription = t.TaskDescription,
                                DueDate = t.DueDate,
                                ProjectLabelIds = t.TaskLabels
                                    .Select(tl => tl.Id)
                                    .ToList()
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (project == null) return NotFound(new { message = $"Project with ID {id} not found" });

                bool isOwner = project.Owner.GitHubID == userId;
                bool isAssignedUser = project.AssignedUsers.Any(u => u.GitHubID == userId);

                if (!isOwner && !isAssignedUser) 
                    return StatusCode(403, new {message="Unauthorised access to this resource."});

                return Ok(project);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("fetching project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Create a new project")]
        public async Task<ActionResult<ProjectDTO>> AddProject([FromBody] CreateProjectDTO request)
        {

            if (request == null) return BadRequest(new { message = "Project data is null" });

            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == userId);

                if (user == null) 
                    return Unauthorized(new { message = "User not found" });

                if (request.OwnerID != user.Id) 
                    return StatusCode(403, new { message = "Unauthorised access to this resource." });

                bool projectExists = await _context.Projects
                .AnyAsync(p => p.ProjectName == request.ProjectName && p.OwnerId == request.OwnerID);
                if (projectExists) return Conflict(new { message = "A project with this name already exists for this owner" });

                var project = new Project
                {
                    ProjectName = request.ProjectName,
                    ProjectDescription = request.ProjectDescription,
                    OwnerId = request.OwnerID
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var responseDto = new ProjectDTO
                {
                    ID = project.Id,
                    ProjectName = project.ProjectName,
                    ProjectDescription = project.ProjectDescription,
                    Owner = new UserDTO { ID = project.OwnerId },
                };

                return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, responseDto);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("Creating project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }  
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Delete a project based on project ID")]
        public async Task<IActionResult> DeleteProject(int id)
        {               
            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == userId);

                var project = await _context.Projects.FindAsync(id);

                if (project.OwnerId != user.Id) 
                    return StatusCode(403, new { message = "Unauthorised access to this resource." });

                if (project == null) return NotFound(new { message = $"Project with ID {id} not found." });

                bool projectHasTasks = await _context.Tasks.AnyAsync(t => t.ProjectId == id);
                bool projectHasUsers = await _context.UserProjects.AnyAsync(up => up.ProjectId == id);

                if (projectHasTasks || projectHasUsers)
                {
                    return BadRequest(new { message = "Cannot delete project because it has tasks or users." });
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("deleting project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpGet("/api/users/{id}/projects")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get all projects a user is in based on UserID")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetUsersProjects(int id)
        {

            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;

                var userExists = await _context.Users.AnyAsync(u => u.Id == id);
                if (!userExists) return NotFound(new { message = "User does not exist" });

                var projects = await _context.Projects
                    .Where(p => p.OwnerId == id || p.UserProjects.Any(up => up.MemberId == id))
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        Owner = new UserDTO { ID = p.OwnerId, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                                    .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId })
                                    .ToList()
                    }).ToListAsync();

                if (projects == null) return NotFound(new { message = $"No projects found for user with ID {id}" });

                var authorisedProjects = projects
                    .Where(p => p.Owner.GitHubID == userId || p.AssignedUsers.Any(u => u.GitHubID == userId));

                if (authorisedProjects.Count() == 0) 
                    return StatusCode(403, new { message = "Unauthorised access to this resource." });

                return Ok(authorisedProjects);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("fetching user's projects", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpPost("{projectId}/labels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Add a label to project - create if it doesn't exist")]
        public async Task<ActionResult<ProjectLabelDTO>> AddProjectLabel(int projectId, string labelName)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == userEmail);
                if (user == null) return NotFound(new { message = "User not found." });

                var userId = user.Id;

                if (string.IsNullOrWhiteSpace(labelName)) return BadRequest(new { message = "Label name is required." });
                if (projectId <= 0) return BadRequest(new { message = "Project ID is required." });

                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null) return NotFound(new { message = "Project not found." });

                bool isProjectOwner = project.OwnerId == userId;
                bool isAdmin = await _context.UserProjects
                                    .AnyAsync(up => up.MemberId == userId && up.RoleId == 1);

                if (!isProjectOwner && !isAdmin) 
                    return StatusCode(403, new { message = "Unauthorised access to this resource." });

                var label = await _context.Labels
                    .Where(l => l.LabelName == labelName)
                    .Select(l => new LabelDTO { ID = l.Id, LabelName = l.LabelName })
                    .FirstOrDefaultAsync();

                if (label != null)
                {
                    var projectLabelExists = await _context.ProjectLabels
                        .AnyAsync(pl => pl.ProjectId == projectId && label.ID == pl.LabelId);

                    if (projectLabelExists) return BadRequest(new { message = "Project label already exists." });
                }

                await _context.AddLabelToProject(projectId, labelName);
                return Created();
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("adding label to a project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpDelete("{projectId}/labels")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Add a label to project - create if it doesn't exist")]
        public async Task<IActionResult> DeleteProjectLabel(int projectId, string labelName)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == userEmail);
                if (user == null) return NotFound(new { message = "User not found." });

                var userId = user.Id;

                if (string.IsNullOrWhiteSpace(labelName)) return BadRequest(new { message = "Label name is required." });
                if (projectId <= 0) return BadRequest(new { message = "Project ID is required." });

                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null) return NotFound(new { message = "Project not found." });

                bool isProjectOwner = project.OwnerId == userId;
                bool isAdmin = await _context.UserProjects
                                    .AnyAsync(up => up.MemberId == userId && up.RoleId == 1);

                if (!isProjectOwner && !isAdmin)
                    return StatusCode(403, new { message = "Unauthorised access to this resource." });

                var label = await _context.Labels
                            .Where(l => l.LabelName == labelName)
                            .Select(l => new LabelDTO { ID = l.Id, LabelName = l.LabelName })
                            .FirstOrDefaultAsync();

                if (label == null) return NotFound(new { message = "Label not found." });

                var projectLabel = await _context.ProjectLabels
                    .Where(pl => pl.ProjectId == projectId && pl.LabelId == label.ID)
                    .FirstOrDefaultAsync();

                if (projectLabel == null) return NotFound(new { message = "Project label not found." });

                _context.ProjectLabels.Remove(projectLabel);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("deleting project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }
    }
}