using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

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

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskDTO>> GetTask(int taskId)
    {
        var task = await _context.Tasks
            .Where(t => t.Id == taskId)
            .Select(t => new TaskDTO
            {
                TaskName = t.TaskName,
                ProjectId = t.ProjectId,
                AssigneeId = t.AssigneeId,
                PriorityId = t.PriorityId,
                TaskDescription = t.TaskDescription ?? "No description provided",
                DueDate = t.DueDate,
                ProjectLabelIds = t.TaskLabels.Select(tl => tl.ProjectLabelId).ToList()
            })
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound($"Task with ID {taskId} not found.");
        }

        return Ok(task);
    }
    
    [HttpGet()]
    [SwaggerOperation(Summary = "Get all the users' tasks based on the assignee ID")]
    public async Task<ActionResult<IEnumerable<TaskResponseDTO>>> GetUserTasks()
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == userEmail);
            if (currentUser == null) return Unauthorized(new { message = "User not found" });

            var assigneeExists = await _context.Users.AnyAsync(u => u.Id == currentUser.Id);
            if (!assigneeExists) 
            {
                return NotFound(new { message = $"User with ID {currentUser.Id} does not exist." });
            }

            var tasks = await _context.Tasks
                .Where(t => t.AssigneeId == currentUser.Id)
                .Select(t => new TaskResponseDTO
                {
                    TaskId = t.Id,
                    AssigneeId = t.AssigneeId,
                    TaskName = t.TaskName,
                    TaskDescription = t.TaskDescription,
                    DueDate = t.DueDate,
                    PriorityId = t.PriorityId,
                    ProjectId = t.ProjectId,
                    ProjectLabelIds = t.TaskLabels.Select(tl => tl.ProjectLabelId).ToList()
                })
                .ToListAsync();

            if (!tasks.Any()) 
                return NotFound(new { message = $"No tasks found for user {currentUser.Id}." });

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An internal server error occurred: {ex.Message}" });
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a task in a project")]
    public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
    {
        int defaultStatusId = 1;
        DateTime? finalDueDate;
        int priorityId;
        int assigneeId;
        try
        {
            if (taskDto == null)
                return BadRequest(new { message = "Task data cannot be null." });
            if (string.IsNullOrWhiteSpace(taskDto.TaskName))
                return BadRequest(new { message = "Task name is required." });
            if (taskDto.TaskName.Length > 255)
                return BadRequest(new { message = "Task name cannot exceed 255 charcacters." });
            if (taskDto.TaskDescription.Length > 1000)
                return BadRequest(new { message = "Task Description cannot exceed a 1000 charcacters." });
            if (taskDto.PriorityId.HasValue)
                if (!EnumHelper.IsValidEnumValue<TaskPriority>(taskDto.PriorityId.Value))
                    return BadRequest(new { message = $"Priority must be one of the following: {EnumHelper.GetEnumValidValues<TaskPriority>()}." });
            
            var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.GitHubId == currentEmail);

            if (currentUser == null) return Unauthorized(new { message = "User not found or unauthorised" });
            if (taskDto.AssigneeId.HasValue)
            {
                var assigneeExists = await _context.Users.AnyAsync(u => u.Id == taskDto.AssigneeId);
                if (!assigneeExists) return NotFound(new { message = "Assignee does not exist." });
                else assigneeId = taskDto.AssigneeId.Value;
            }
            else
            {
                var userId = await _context.Users
                .Where(u => u.GitHubId == currentEmail)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

                assigneeId = userId;
            }

            var userHasAccessToProject = await _context.UserProjects
                        .AnyAsync(up => up.MemberId == currentUser.Id && up.ProjectId == taskDto.ProjectId);
            if (!userHasAccessToProject)
                return Unauthorized(new { message = "User does not have permission to create tasks for this project." });

            if (taskDto.DueDate.HasValue)
            {
                if (taskDto.DueDate.Value < DateTime.UtcNow) 
                {
                    return BadRequest(new { message = "Due date cannot be in the past." });
                }
                finalDueDate = taskDto.DueDate.Value;
            }
            else
            {
                finalDueDate = DateTime.UtcNow.AddDays(7);
            }

            await _context.CreateTaskAsync(
                assigneeId, taskDto.TaskName, taskDto.TaskDescription ?? null,
                finalDueDate, taskDto.PriorityId.Value, taskDto.ProjectId, defaultStatusId);

            var createdTask = await _context.Tasks
                .Where(t => t.AssigneeId == assigneeId && t.TaskName == taskDto.TaskName && t.ProjectId == taskDto.ProjectId)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (createdTask == null)
                return StatusCode(500, new { message = "Task was not found after insertion." });

            if (taskDto.ProjectLabelIds != null && taskDto.ProjectLabelIds.Any() && !taskDto.ProjectLabelIds.Contains(0))
            {
                var taskLabels = taskDto.ProjectLabelIds.Select(labelId => new TaskLabel
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
    [SwaggerOperation(Summary = "Update a task in a project based on task ID")]
    public async Task<IActionResult> UpdateTask(int taskid, [FromBody] TaskUpdateDTO taskDto)
    {
        if (taskDto == null) return BadRequest(new { message = "Invalid task data." });

        var existingTask = await _context.Tasks
            .Include(t => t.TaskLabels)
            .FirstOrDefaultAsync(t => t.Id == taskid);

        if (existingTask == null) return NotFound(new { message = $"Task with ID {taskid} not found." });

        if (!string.IsNullOrWhiteSpace(taskDto.TaskName)) 
            if (taskDto.TaskName.Length > 255) return BadRequest(new { message = "Task name cannot exceed 255 charcacters." });
            else existingTask.TaskName = taskDto.TaskName;

        if (taskDto.PriorityId.HasValue)
        {
            if (!EnumHelper.IsValidEnumValue<TaskPriority>(taskDto.PriorityId.Value))
            {
                return BadRequest(new { message = $"Priority must be one of the following: {EnumHelper.GetEnumValidValues<TaskPriority>()}." });
            }

            existingTask.PriorityId = taskDto.PriorityId.Value;
        }
        else
        {
            existingTask.PriorityId = (int)TaskPriority.Low;
        }

        if (taskDto.AssigneeId.HasValue) 
            if (taskDto.AssigneeId <= 0)
                return BadRequest(new { message = "AssigneeId is required and must be a valid value." });
            else  
            {
                var assigneeExists = await _context.Users.AnyAsync(u => u.Id == taskDto.AssigneeId);
                if (!assigneeExists) return NotFound(new { message = "Assignee does not exist." });
                else existingTask.AssigneeId = taskDto.AssigneeId.Value;
            }

        if (!string.IsNullOrWhiteSpace(taskDto.TaskDescription)) 
            if (taskDto.TaskDescription.Length > 1000) return BadRequest(new { message = "Task Description cannot exceed a 1000 charcacters." });
            else existingTask.TaskDescription = taskDto.TaskDescription;

        if (taskDto.DueDate.HasValue) existingTask.DueDate = taskDto.DueDate.Value;

        if (taskDto.ProjectLabelIds != null)
        {
            var existingLabels = existingTask.TaskLabels.ToList(); 

            foreach (var label in existingLabels)
            {
                if (!taskDto.ProjectLabelIds.Contains(label.ProjectLabelId))
                {
                    _context.TaskLabels.Remove(label);
                }
            }

            foreach (var labelId in taskDto.ProjectLabelIds)
            {
                if (!existingTask.TaskLabels.Any(tl => tl.ProjectLabelId == labelId))
                {
                    existingTask.TaskLabels.Add(new TaskLabel
                    {
                        TaskId = taskid,
                        ProjectLabelId = labelId
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Task with ID {taskid} updated successfully." });
    }

    [HttpDelete("{taskid}")]
    [SwaggerOperation(Summary = "Delete a users' task based on the task ID")]
    public async Task<IActionResult> DeleteTask(int taskid)
    {
        var task = await _context.Tasks.FindAsync(taskid);
        if (task == null)
        {
            return NotFound(new { message = $"Task with ID {taskid} not found." });
        }

        var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == currentEmail);
        if (currentUser == null) return Unauthorized(new { message = "User not found" });

        bool isAdmin = await _context.UserProjects
                .AnyAsync(ur => ur.MemberId == currentUser.Id && ur.RoleId == 1);

        if (!isAdmin) return Unauthorized(new { message = "Only admins can delete tasks." });

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
