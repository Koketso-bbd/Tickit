using api.Data;
using api.DTOs;
using api.Helpers;
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
                    var message = "No projects found";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(projects);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("projects", _logger, ex);
                return StatusCode(statusCode, message);
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
                    var message = $"Project with ID {id} not found";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("project", _logger, ex);
                return StatusCode(statusCode, message);
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
                    var message = $"Project with ID {id} not found.";
                    _logger.LogWarning(message);
                    return NotFound(message);
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
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorDelete("project", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{projectId}/tasks/{taskid}")]
        public async Task<ActionResult<Models.Task>> GetTask(int projectId, int taskid)
        {
            try
            {
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskid && t.ProjectId == projectId);

                if (task == null)
                {
                    string message = $"Task with ID {taskid} not found in project {projectId}.";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{projectId}/tasks")]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasksInProject(int projectId)
        {
            try{
                var tasks = await _context.Tasks
                                                .Where(t => t.ProjectId==projectId)
                                                .ToListAsync();

                if (tasks == null || tasks.Count == 0)
                {
                    var message = $"No tasks found for Project ID {projectId}."; 
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                    return Ok(tasks);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("tasks", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTask(int projectId, [FromBody] TaskDTO taskDto)
        {
            if (taskDto == null || string.IsNullOrWhiteSpace(taskDto.TaskName))
            {
                return BadRequest("Invalid task data.");
            }

            try
            {
                int result = await _context.CreateTaskAsync(
                    taskDto.AssigneeId, taskDto.TaskName, taskDto.TaskDescription,
                    taskDto.DueDate, taskDto.PriorityId, projectId, taskDto.StatusId);

                if (result == 0)
                    return StatusCode(500, "Task creation failed.");

                return CreatedAtAction(nameof(GetTask), 
                    new { projectId, taskid = taskDto.Id }, taskDto);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorPost("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }
    }
}