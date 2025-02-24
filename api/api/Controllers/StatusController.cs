using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<StatusController> _logger;

        public StatusController(TickItDbContext context, ILogger<StatusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StatusDTO>>> GetStatuses()
        {
            return await _context.Statuses
               .Select(s => new StatusDTO
               {
                   Id = s.Id,
                   StatusName = s.StatusName,
               })
               .ToListAsync();

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StatusDTO>> GetStatusById(int id)
        {
            try
            {
                var status = await _context.Statuses
                    .Where(s => s.Id == id)
                    .Select(s => new StatusDTO{
                        Id = s.Id,
                        StatusName = s.StatusName,
                    })
                    .FirstOrDefaultAsync();
                
                if(status == null)
                {
                    _logger.LogWarning("Status with ID{id} not found",id);
                    return NotFound($"Status with ID {id} not found");
                }

                return Ok(status);
            
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occured when fetching the project.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        
    }


}