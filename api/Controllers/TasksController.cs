using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.ComponentModel.DataAnnotations;

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
                return NotFound(new { message = $"No tasks found for user {assigneeId}." });

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An internal server error occurred: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskDTO dto)
    {
        try
        {
            if (dto.PriorityId < 1 || dto.PriorityId > 4)
                return BadRequest(new { message = "Priority must be between 1 and 4, where 1='Low', 2='Medium', 3='High', and 4='Urgent'." });

            if (dto.AssigneeId <= 0)
                return BadRequest(new { message = "AssigneeId is required and must be a valid value." });

            var assigneeExists = await _context.Users.AnyAsync(u => u.Id == dto.AssigneeId);
            if (!assigneeExists) return NotFound(new { message = "Assignee does not exist." });

            DateTime finalDueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(7);
            int defaultStatusId = 1;

            await _context.CreateTaskAsync(
                dto.AssigneeId, dto.TaskName, dto.TaskDescription ?? "No description provided",
                finalDueDate, dto.PriorityId, dto.ProjectId, defaultStatusId);

            var createdTask = await _context.Tasks
                .Where(t => t.AssigneeId == dto.AssigneeId && t.TaskName == dto.TaskName && t.ProjectId == dto.ProjectId)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (createdTask == null)
                return StatusCode(500, new { message = "Task was not found after insertion." });

            if (dto.ProjectLabelIds != null && dto.ProjectLabelIds.Any())
            {
                var taskLabels = dto.ProjectLabelIds.Select(labelId => new TaskLabel
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
            return StatusCode(500, new { message = $"An unexpected error occurred while creating the task. Please try again later: {ex.Message}" });
        }
    }

}
