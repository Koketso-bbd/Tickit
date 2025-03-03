using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly TickItDbContext _context;
    public TasksController(TickItDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{assigneeId}")]
    public async Task<ActionResult<IEnumerable<TaskDTO>>> GetUserTasks(int assigneeId)
    {
        try
        {
            var tasks = await _context.Tasks
                .Where(t => t.AssigneeId == assigneeId)
                .Select(t => new TaskDTO
                {
                    Id = t.Id,
                    AssigneeId = t.AssigneeId,
                    TaskName = t.TaskName,
                    TaskDescription = t.TaskDescription,
                    DueDate = t.DueDate,
                    PriorityId = t.PriorityId,
                    ProjectId = t.ProjectId,
                    StatusId = t.StatusId,
                    TaskLabels = t.TaskLabels.Select(tl => new TaskLabelDTO
                    {
                        ID = tl.Id,
                        TaskId = tl.TaskId,
                        ProjectLabelId = tl.ProjectLabelId
                    }).ToList()
                })
                .ToListAsync();

            if (!tasks.Any()) return NotFound($"No tasks found for user {assigneeId}.");

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An internal server error occurred. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
    {
        try
        {
            if (taskDto == null)
                return BadRequest("Task data cannot be null.");
            if (string.IsNullOrWhiteSpace(taskDto.TaskName))
                return BadRequest("Task name is required.");
            if (taskDto.PriorityId <= 0)
                return BadRequest("Priority is required and must be a positive integer.");
            if (taskDto.StatusId <= 0)
                return BadRequest("Status is required and must be a valid value.");
            if (taskDto.AssigneeId <= 0)
                return BadRequest("AssigneeId is required and must be a valid value.");

            string description = string.IsNullOrWhiteSpace(taskDto.TaskDescription) ? "No description provided" : taskDto.TaskDescription;
            DateTime dueDate = (DateTime)taskDto.DueDate;
            int assigneeId = taskDto.AssigneeId;

            await _context.CreateTaskAsync(
                assigneeId, taskDto.TaskName, description,
                dueDate, taskDto.PriorityId, taskDto.ProjectId, taskDto.StatusId);

            var createdTask = await _context.Tasks
                .Where(t => t.AssigneeId == assigneeId && t.TaskName == taskDto.TaskName && t.ProjectId == taskDto.ProjectId)
                .OrderByDescending(t => t.Id) 
                .FirstOrDefaultAsync();

            if (createdTask == null)
                return StatusCode(500, "Task was not found after insertion.");

            if (taskDto.TaskLabels != null && taskDto.TaskLabels.Any())
            {
                var taskLabels = taskDto.TaskLabels.Select(label => new TaskLabel
                {
                    TaskId = createdTask.Id,
                    ProjectLabelId = label.ProjectLabelId
                }).ToList();

                await _context.TaskLabels.AddRangeAsync(taskLabels);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(CreateTask), new { taskId = createdTask.Id }, taskDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred while creating the task. Please try again later.");
        }
    }

    [HttpPut("{taskid}")]
    public async Task<IActionResult> UpdateTask(int taskid, [FromBody] TaskDTO taskDto)
    {
        if (taskDto == null) return BadRequest("Invalid task data");

        var existingTask = await _context.Tasks
            .Include(t => t.TaskLabels) 
            .FirstOrDefaultAsync(t => t.Id == taskid);

        if (existingTask == null) return NotFound($"Task with ID {taskid} not found.");

        if (taskDto.ProjectId != existingTask.ProjectId) return BadRequest("Updating ProjectId is not allowed.");

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

        _context.Tasks.Update(existingTask); 

        if (taskDto.TaskLabels != null && taskDto.TaskLabels.Any())
        {
            existingTask.TaskLabels.RemoveAll(tl => !taskDto.TaskLabels.Any(dto => dto.ProjectLabelId == tl.ProjectLabelId));

            foreach (var newLabel in taskDto.TaskLabels)
            {
                if (!existingTask.TaskLabels.Any(tl => tl.ProjectLabelId == newLabel.ProjectLabelId))
                {
                    existingTask.TaskLabels.Add(new TaskLabel
                    {
                        TaskId = taskid,
                        ProjectLabelId = newLabel.ProjectLabelId
                    });
                }
            }
        }

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
            if (statusTracks.Count != 0)
            {
                _context.StatusTracks.RemoveRange(statusTracks);
            }

            var notifications = await _context.Notifications.Where(n => n.TaskId == taskid).ToListAsync();
            if (notifications.Count != 0)
            {
                _context.Notifications.RemoveRange(notifications);
            }

            var taskLabels = await _context.TaskLabels.Where(tl => tl.TaskId == taskid).ToListAsync();
            if (taskLabels.Count != 0)
            {
                _context.TaskLabels.RemoveRange(taskLabels);
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();  
            await transaction.CommitAsync();

            return Ok(new { message = $"Task with ID {taskid} successfully deleted." });
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Database error occurred while deleting task: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"An unexpected error occurred while deleting task: {ex.Message}");
        }
    }
}
