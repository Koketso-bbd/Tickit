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
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDTO taskDto)
        {
            if (taskDto == null)
            {
                return BadRequest("Invalid task data.");
            }

            taskDto.Id = id;

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
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

        [HttpGet("{taskid}/labels/{labelId}")]
        public async Task<ActionResult<TaskLabelDTO>> GetTaskLabelById(int taskid, int labelId)
        {
            try
            {
                var taskLabel = await _context.TaskLabels
                    .Where(tsl => tsl.Id == labelId && tsl.TaskId == taskid) // Ensuring label belongs to the correct task
                    .Select(tsl => new TaskLabelDTO
                    {
                        ID = tsl.Id,
                        TaskId = tsl.TaskId,
                        ProjectLabelId = tsl.ProjectLabelId
                    })
                    .FirstOrDefaultAsync();

                if (taskLabel == null)
                {
                    var message = $"Task Label with ID {labelId} not found for Task {taskid}.";
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
        public async Task<IActionResult> PostTaskLabels(int taskId, [FromQuery] int projectLabelId)
        {
            try
            {
                bool taskLabelExists = await _context.TaskLabels
                    .AnyAsync(tsl => tsl.TaskId == taskId && tsl.ProjectLabelId == projectLabelId);

                if (taskLabelExists)
                {
                    return Conflict("This label is already assigned to the task.");
                }

                var taskLabel = new TaskLabel
                {
                    TaskId = taskId,
                    ProjectLabelId = projectLabelId,
                };

                _context.TaskLabels.Add(taskLabel);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTaskLabelById), new { id = taskLabel.Id }, taskLabel);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorPost("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }


        [HttpDelete("{taskId}/labels/{labelId}")]
        public async Task<IActionResult> DeleteTaskLabel(int taskId, int projectlabelId)
        {
            var taskLabel = await _context.TaskLabels
                .FirstOrDefaultAsync(tl => tl.TaskId == taskId && tl.ProjectLabelId == projectlabelId);

            if (taskLabel == null)
            {
                return NotFound($"Label with ID {projectlabelId} not found for task {taskId}.");
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
        public async Task<ActionResult<IEnumerable<object>>> GetTasksInProject(int projectId)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Where(t => t.ProjectId == projectId)
                    .Select(t => new 
                    {
                        t.Id,
                        t.AssigneeId,
                        t.TaskName,
                        t.TaskDescription,
                        t.DueDate,
                        t.PriorityId,
                        t.ProjectId,
                        t.StatusId
                    })
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


        [HttpPost("task")]
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
                    {
                        return StatusCode(422, "Task creation failed due to a logical issue.");
                    }


                return CreatedAtAction(nameof(GetTask), 
                    new { taskDto.ProjectId, taskid = taskDto.Id }, taskDto);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorPost("task", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }
    }
}
