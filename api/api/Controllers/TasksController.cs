using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Helpers;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasks()
        {
            try
            {
                var tasks = await _context.Tasks.ToListAsync();

                if (tasks == null || tasks.Count == 0)
                {
                    var message = "No tasks found.";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorFetching("tasks", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Task>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                string message = $"Task with ID {id} not found.";
                _logger.LogWarning(message);
                return NotFound(message);
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
