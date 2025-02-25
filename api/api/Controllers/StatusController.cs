using api.Data;
using api.DTOs;
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
    }
}