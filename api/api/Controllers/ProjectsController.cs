using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        OwnerID = p.OwnerId,
                        Owner = new UserDTO { ID = p.Owner.Id, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                            .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId }) // Fixed (sometimes), I think?
                            .ToList(),
                    })
                    .ToListAsync();

                if (projects == null || !projects.Any())
                {
                    _logger.LogWarning("No projects have been found");
                    return NotFound("No projects found");
                }

                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when fetching projects.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDTO>> GetProjectById(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Where(p => p.Id == id)
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        OwnerID = p.OwnerId,
                        Owner = new UserDTO { ID = p.Owner.Id, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                            .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId })
                            .ToList()

                    })
                    .FirstOrDefaultAsync();

                if (project == null)
                {
                    _logger.LogWarning("Project with ID {id} not found.", id);
                    return NotFound($"Project with ID {id} not found");
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when fetching the project.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDTO>> AddProject(ProjectDTO projectDto)
        {
            if (projectDto == null)
            {
                return BadRequest("Project data is null.");
            }

            bool projectExists = await _context.Projects
                .AnyAsync(p => p.ProjectName == projectDto.ProjectName && p.OwnerId == projectDto.OwnerID);

            if (projectExists)
            {
                return Conflict("A project with this name already exists for this owner");
            }

            var project = new Project
            {
                ProjectName = projectDto.ProjectName,
                ProjectDescription = projectDto.ProjectDescription,
                OwnerId = projectDto.OwnerID
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            projectDto.ID = project.Id;

            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, projectDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);

                if (project == null)
                {
                    _logger.LogWarning($"Project with ID {id} not found.");
                    return NotFound($"Project with {id} not found");
                }

                bool projectHasTasks = _context.Tasks.Any(t => t.ProjectId == id);
                bool projectHasUsers = _context.UserProjects.Any(up => up.ProjectId == id);

                if (projectHasTasks || projectHasUsers)
                {
                    return BadRequest("Cannot delete project because it has tasks or users.");
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting this project.");
                return StatusCode(500, "Internal Service Error");
            }
        }
    }
}