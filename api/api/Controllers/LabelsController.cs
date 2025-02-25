using api.Data;
using api.DTOs;
using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<LabelsController> _logger;

        public LabelsController(TickItDbContext context, ILogger<LabelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LabelDTO>>> GetLabels()
        {
            try
            {
                var labels = await _context.Labels
                    .Select(l => new LabelDTO { ID = l.Id, LabelName = l.LabelName })
                    .ToListAsync();                
                
                if (labels == null || !labels.Any())
                {
                    var message = "No labels found.";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }
                return Ok(labels);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("labels", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLabelById(int id)
        {
            try
            {
                var label = await _context.Labels
                    .Where(l => l.Id == id)
                    .Select(l => new LabelDTO { ID = l.Id, LabelName = l.LabelName })
                    .FirstOrDefaultAsync();

                if (id <= 0)
                {
                    var message = "ID is required.";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }
                else if (label == null)
                {
                    _logger.LogWarning("Label doesn't exist.");
                    return NotFound("Label not found.");
                }
                return Ok(label);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("label", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }
    }
}