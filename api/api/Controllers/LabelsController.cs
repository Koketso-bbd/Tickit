using api.Data;
using api.DTOs;
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
                    _logger.LogWarning("No labels have been found.");
                    return NotFound("No labels found.");
                }
                return Ok(labels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while fetching labels");
                return StatusCode(500, "Internal Error");
            }
        }
    }
}