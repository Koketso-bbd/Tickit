using api.Data;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;
using Microsoft.EntityFrameworkCore;
using api.Models;

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
                    _logger.LogWarning("Task Label with ID {id} not found.", id);
                    return NotFound($"Task Label with ID {id} not found");
                }

                return Ok(taskLabel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when fetching the project.");
                return StatusCode(500, "Internal Server Error");
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

    }

}