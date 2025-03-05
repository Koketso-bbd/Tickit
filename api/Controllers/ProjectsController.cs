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
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        Owner = new UserDTO { ID = p.Owner.Id, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                            .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId })
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

            bool projectExists = await _context.Projects.AnyAsync(p => p.ProjectName == request.ProjectName && p.OwnerId == request.OwnerID);

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

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Delete a project based on project ID")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);

                if (project == null) return NotFound(new { message = $"Project with ID {id} not found." });

                bool projectHasTasks = _context.Tasks.Any(t => t.ProjectId == id);
                bool projectHasUsers = _context.UserProjects.Any(up => up.ProjectId == id);

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

            var userExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!userExists) return NotFound(new { message = "User does not exist" });

            try
            {
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

                return Ok(projects);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("fetching user's projects", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpGet("{id}/labels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get all labels for a project")]
        public async Task<ActionResult<ProjectLabelDTO>> GetProjectLabels(int id)
        {
            try
            {
                var projectLabel = await _context.ProjectLabels
                    .Where(pl => pl.ProjectId == id)
                    .Select(pl => new ProjectLabelDTO
                    {
                        ID = pl.Id,
                        LabelID = pl.LabelId,
                        ProjectID = pl.ProjectId,
                        LabelName = new LabelDTO { ID = pl.LabelId, LabelName = pl.Label.LabelName }
                    })
                    .ToListAsync();

                if (projectLabel == null) return NotFound(new { message = $"Project with ID {id} not found" });

                return Ok(projectLabel);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("fetching project label", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpPost("{projectId}/labels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Add a label to project - create if it doesn't exist")]
        public async Task<ActionResult<ProjectLabelDTO>> AddProjectLabel(int id, string labelName)
        {
            if (labelName.IsNullOrEmpty()) return BadRequest(new { message = "labelName is required." });
            if (id <= 0) return BadRequest(new { message = "ProjectID is required." });

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == id);
            if (!projectExists) return NotFound(new { message = "Project not found" });

            var label = await _context.Labels
                .Where(l => labelName == l.LabelName)
                .Select(l => new LabelDTO { ID = l.Id, LabelName = l.LabelName }).FirstOrDefaultAsync();

            if (label != null)
            {
                var projectLabelExist = await _context.ProjectLabels.AnyAsync(pl => pl.ProjectId == id && label.ID == pl.LabelId);
                if (projectLabelExist) return BadRequest(new { message = "Project label already exists" });
            }

            try
            {
                await _context.AddLabelToProject(id, labelName);
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
        [SwaggerOperation(Summary = "Add a label to project - create if it doesn't exist")]
        public async Task<IActionResult> DeleteProjectLabel(int id, string labelName)
        {
            try
            {
                if (labelName.IsNullOrEmpty()) return BadRequest(new { message = "labelName is required." });
                if (id <= 0) return BadRequest(new { message = "ProjectID is required." });

                var projectExists = await _context.Projects.AnyAsync(p => p.Id == id);
                if (!projectExists) return NotFound(new { message = "Project not found" });

                var label = await _context.Labels
                    .Where(l => labelName == l.LabelName)
                    .Select(l => new LabelDTO { ID = l.Id, LabelName = l.LabelName }).FirstOrDefaultAsync();
                if (label == null) return NotFound(new { message = "Label not found" });

                var projectLabel = await _context.ProjectLabels
                    .Where(pl => pl.ProjectId == id && label.ID == pl.LabelId).FirstOrDefaultAsync();
                if (projectLabel == null) return NotFound(new { message = "Project label not found" });

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