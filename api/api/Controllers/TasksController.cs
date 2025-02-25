using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;


namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TickItDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("/projects/{projectId}/tasks")]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasksInProject(int projectId)
        {
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


        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Task>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            string warnMessage = $"Task with ID {id} not found.";

            if (task == null)
            {
                _logger.LogWarning(warnMessage);
                return NotFound(warnMessage);
            }

            return Ok(task);
        }
        

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
        {
            if (taskDto == null || string.IsNullOrWhiteSpace(taskDto.TaskName))
            {
                return BadRequest("Invalid task data.");
            }

            try
            {
                int result = await _context.CreateTaskAsync(
                    taskDto.AssigneeId, taskDto.TaskName, taskDto.TaskDescription,
                    taskDto.DueDate, taskDto.PriorityId, taskDto.ProjectId, taskDto.StatusId);

                if (result == 0)
                    return StatusCode(500, "Task creation failed.");

                return CreatedAtAction(nameof(GetTask), new { id = taskDto.Id }, taskDto);

            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
