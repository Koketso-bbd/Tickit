using api.Data;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Helpers;

namespace api.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class TaskLabelsController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<TaskLabelsController> _logger;

        public TaskLabelsController(TickItDbContext context, ILogger<TaskLabelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
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

        [HttpPost]
        public async Task<IActionResult> PostTaskLabels(TaskLabelDTO taskLabelDTO)
        {
           if( taskLabelDTO == null)
           {
               return BadRequest("Task label data is null");
           }

           bool taskLabelExists = await _context.TaskLabels
           .AnyAsync(tsl => tsl.Id == taskLabelDTO.ID && tsl.ProjectLabelId == taskLabelDTO.ProjectLabelId);

           if(taskLabelExists)
           {
                return Conflict("A task with this name already exists");
           }
           
           var tasklabel = new TaskLabel
           {
                Id = taskLabelDTO.ID,
                TaskId = taskLabelDTO.TaskId,
                ProjectLabelId = taskLabelDTO.ProjectLabelId,
           };

           _context.TaskLabels.Add(tasklabel);
           await _context.SaveChangesAsync();

           taskLabelDTO.ID = tasklabel.Id;

           return CreatedAtAction(nameof(GetTaskLabelById),new{id = tasklabel.Id},taskLabelDTO);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult>DeleteTaskLabel(int id)
        {
            try
            {
                var taskLabel = await _context.TaskLabels.FindAsync(id);

                if(taskLabel == null)
                {
                    return NotFound("Task Label does not exist");
                }

                _context.TaskLabels.Remove(taskLabel);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Task {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorDelete("task label", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }
    }
}