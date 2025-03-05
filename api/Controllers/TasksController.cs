using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

            if (!tasks.Any()) return NotFound(new { message = $"No tasks found for user {assigneeId}." });

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An internal server error occurred. Please try again later." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(
        //POST /api/tasks?assigneeId=1&taskName=New+Feature&priorityId=2&projectId=3&statusId=1&taskDescription=Refactor+code&dueDate=2025-03-05T07:31:02.947Z&projectLabelIds=10&projectLabelIds=20
        [FromQuery] int assigneeId,
        [FromQuery] string taskName,
        [FromQuery] int priorityId,
        [FromQuery] int projectId,
        [FromQuery] int statusId,
        [FromQuery] string? taskDescription = null,
        [FromQuery] DateTime? dueDate = null,
        [FromQuery] List<int>? projectLabelIds = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return BadRequest(new { message = "Task name is required." });
            if (priorityId <= 0)
                return BadRequest(new { message = "Priority is required and must be a positive integer." });
            if (statusId <= 0)
                return BadRequest(new { message = "Status is required and must be a valid value." });
            if (assigneeId <= 0)
                return BadRequest(new { message = "AssigneeId is required and must be a valid value." });

            string description = string.IsNullOrWhiteSpace(taskDescription) ? "No description provided" : taskDescription;
            DateTime finalDueDate = dueDate ?? DateTime.UtcNow.AddDays(7);  // Default to 7 days if not provided

            await _context.CreateTaskAsync(
                assigneeId, taskName, description,
                finalDueDate, priorityId, projectId, statusId);

            var createdTask = await _context.Tasks
                .Where(t => t.AssigneeId == assigneeId && t.TaskName == taskName && t.ProjectId == projectId)
                .OrderByDescending(t => t.Id) 
                .FirstOrDefaultAsync();

            if (createdTask == null)
                return StatusCode(500, new { message = "Task was not found after insertion." });

            if (projectLabelIds != null && projectLabelIds.Any())
            {
                var taskLabels = projectLabelIds.Select(labelId => new TaskLabel
                {
                    TaskId = createdTask.Id,
                    ProjectLabelId = labelId
                }).ToList();

                await _context.TaskLabels.AddRangeAsync(taskLabels);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(CreateTask), new { taskId = createdTask.Id }, new { message = "Task created successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while creating the task. Please try again later." });
        }
    }


    [HttpPut("{taskid}")]
    public async Task<IActionResult> UpdateTask(int taskid, [FromBody] TaskDTO taskDto)
    {
        if (taskDto == null) return BadRequest(new { message = "Invalid task data" });

        var existingTask = await _context.Tasks
            .Include(t => t.TaskLabels) 
            .FirstOrDefaultAsync(t => t.Id == taskid);

        if (existingTask == null) return NotFound(new { message = $"Task with ID {taskid} not found." });

        if (taskDto.ProjectId != existingTask.ProjectId) return BadRequest(new { message = "Updating ProjectId is not allowed." });

        if (string.IsNullOrWhiteSpace(taskDto.TaskName) || 
            taskDto.PriorityId <= 0 || 
            taskDto.StatusId <= 0 || 
            taskDto.AssigneeId <= 0)
        {
            return BadRequest(new { message = "Task Name, Priority, Status, and AssigneeId are required and must be valid." });
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
            return NotFound(new { message = $"Task with ID {taskid} not found." });
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
            return StatusCode(500, new { message = $"Database error occurred while deleting task: {dbEx.Message}" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = $"An unexpected error occurred while deleting task: {ex.Message}" });
        }
    }
}
