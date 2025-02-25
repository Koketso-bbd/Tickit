using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Helpers;
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskDTO taskDto)
        {
            if (taskDto==null || id != taskDto.Id)
            {
                return BadRequest("Invalid task data");
            }

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask==null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            existingTask.AssigneeId = taskDto.AssigneeId;
            existingTask.TaskName = taskDto.TaskName;
            existingTask.TaskDescription = taskDto.TaskDescription;
            existingTask.DueDate = taskDto.DueDate;
            existingTask.PriorityId = taskDto.PriorityId;
            existingTask.StatusId = taskDto.StatusId;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorPut("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var notifications = _context.Notifications.Where(n => n.TaskId == id);
            _context.Notifications.RemoveRange(notifications);

            _context.Tasks.Remove(task);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorDelete("task", _logger, ex);
                return StatusCode(statusCode, message);;
            }
        }

        [HttpGet("labels/{labelId}")]
        public async Task<ActionResult<TaskLabelDTO>> GetTaskLabelById(int id)
        {
            try
            {
                var taskLabel = await _context.TaskLabels
                    .Where(tsl => tsl.Id == id)
                    .Select(tsl => new TaskLabelDTO
                    {
                        ID = tsl.Id,
                        TaskId = tsl.TaskId,
                        ProjectLabelId = tsl.ProjectLabelId

                    })
                    .FirstOrDefaultAsync();

                if (taskLabel == null)
                {
                    var message = $"Task Label with ID {id} not found.";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(taskLabel);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("task label", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{taskId}/labels")]
        public async Task<ActionResult<IEnumerable<TaskLabelDTO>>> GetTaskLabels(int taskId)
        {
            try
            {
                var labels = await _context.TaskLabels
                .Where(tl => tl.TaskId == taskId)
                .Select(tl => new TaskLabelDTO
                {
                    ID = tl.Id,
                    TaskId = tl.TaskId,
                    ProjectLabelId = tl.ProjectLabelId
                })
                .ToListAsync();

                if (labels == null || labels.Count == 0)
                {
                return NotFound($"No labels found for task ID {taskId}.");
                }

                return Ok(labels);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }


        [HttpPost("{taskId}/labels")]
        public async Task<IActionResult> PostTaskLabels(int taskId, [FromBody] TaskLabelDTO taskLabelDTO)
        {
            if (taskLabelDTO == null)
            {
                return BadRequest("Task label data is null.");
            }

            bool taskLabelExists = await _context.TaskLabels
                .AnyAsync(tsl => tsl.TaskId == taskId && tsl.ProjectLabelId == taskLabelDTO.ProjectLabelId);

            if (taskLabelExists)
            {
                return Conflict("This label is already assigned to the task.");
            }

            var taskLabel = new TaskLabel
            {
                TaskId = taskId,
                ProjectLabelId = taskLabelDTO.ProjectLabelId,
            };
            
            try
            {
                _context.TaskLabels.Add(taskLabel);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetTaskLabelById), new { id = taskLabel.Id }, taskLabelDTO);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorPost("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpDelete("{taskId}/labels/{labelId}")]
        public async Task<IActionResult> DeleteTaskLabel(int taskId, int labelId)
        {
            var taskLabel = await _context.TaskLabels
                .FirstOrDefaultAsync(tl => tl.TaskId == taskId && tl.ProjectLabelId == labelId);

            if (taskLabel == null)
            {
                return NotFound($"Label with ID {labelId} not found for task {taskId}.");
            }

            _context.TaskLabels.Remove(taskLabel);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorDelete("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

    }
}
