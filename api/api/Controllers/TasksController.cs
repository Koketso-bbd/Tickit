using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;


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
            var tasks = await _context.Tasks.ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                _logger.LogWarning("No tasks found.");
                return NotFound("No tasks found.");
            }

            return Ok(tasks);
        }

        [HttpGet("{assigneeId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserTasks(int assigneeId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.AssigneeId == assigneeId)
                .Select(t => new
                {
                    t.Id,
                    t.TaskName,
                    t.TaskDescription,
                    t.DueDate,
                    t.PriorityId,
                    t.ProjectId,
                    t.StatusId
                })
                .ToListAsync();

            if (!tasks.Any())
            {
                return NotFound($"No tasks found for user {assigneeId}.");
            }

            return Ok(tasks);
        }

        [HttpPost("task")]
        public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
        {
            if (taskDto == null || string.IsNullOrWhiteSpace(taskDto.TaskName))
            {
                return BadRequest("Invalid task data.");
            }

            string description = string.IsNullOrWhiteSpace(taskDto.TaskDescription) ? "No description provided" : taskDto.TaskDescription;
            DateTime dueDate = taskDto.DueDate ?? DateTime.UtcNow.AddDays(7);
            int assigneeId = taskDto.AssigneeId; 

            int result = await _context.CreateTaskAsync(
                assigneeId, taskDto.TaskName, description,
                dueDate, taskDto.PriorityId, taskDto.ProjectId, taskDto.StatusId);

            if (result == 0)
            {
                return StatusCode(422, "Task creation failed due to a logical issue.");
            }

            return CreatedAtAction(nameof(CreateTask), new { taskId = taskDto.Id }, taskDto);
        }


        [HttpPut("{taskid}")]
        public async Task<IActionResult> UpdateTask(int taskid, [FromBody] TaskDTO taskDto)
        {
            if (taskDto == null)
            {
                return BadRequest("Invalid task data.");
            }

            var existingTask = await _context.Tasks.FindAsync(taskid);
            if (existingTask == null)
            {
                return NotFound($"Task with ID {taskid} not found.");
            }

            if (taskDto.ProjectId != existingTask.ProjectId)
            {
                return BadRequest("Updating ProjectId is not allowed.");
            }

            if (string.IsNullOrWhiteSpace(taskDto.TaskName) || 
                taskDto.PriorityId <= 0 || 
                taskDto.StatusId <= 0 || 
                taskDto.AssigneeId <= 0)
            {
                return BadRequest("Task Name, Priority, Status, and AssigneeId are required and must be valid.");
            }

            existingTask.AssigneeId = taskDto.AssigneeId;
            existingTask.TaskName = taskDto.TaskName;
            existingTask.TaskDescription = string.IsNullOrWhiteSpace(taskDto.TaskDescription) 
                                        ? existingTask.TaskDescription 
                                        : taskDto.TaskDescription;
            existingTask.DueDate = (DateTime)taskDto.DueDate; 
            existingTask.PriorityId = taskDto.PriorityId;
            existingTask.StatusId = taskDto.StatusId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{taskid}")]
        public async Task<IActionResult> DeleteTask(int taskid)
        {
            var task = await _context.Tasks.FindAsync(taskid);
            if (task == null)
            {
                return NotFound($"Task with ID {taskid} not found.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var statusTracks = await _context.StatusTracks.Where(st => st.TaskId == taskid).ToListAsync();
                if (statusTracks.Any())
                {
                    _context.StatusTracks.RemoveRange(statusTracks);
                }

                var notifications = await _context.Notifications.Where(n => n.TaskId == taskid).ToListAsync();
                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                }

                var tasklabels = await _context.TaskLabels.Where(tl => tl.TaskId == taskid).ToListAsync();
                if (tasklabels.Any())
                {
                    _context.TaskLabels.RemoveRange(tasklabels);
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = $"Task with ID {taskid} successfully deleted." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error deleting task: {ex.Message}");
            }
        }



    }
}
