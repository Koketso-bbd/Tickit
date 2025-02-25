using api.Data;
using api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationTypeController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<NotificationTypeController> _logger;

        public NotificationTypeController(TickItDbContext context, ILogger<NotificationTypeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationTypeDTO>>> GetNotificationTypes()
        {
           return await _context.NotificationTypes
            .Select(nt => new NotificationTypeDTO
            {
                ID = nt.Id,
                NotificationName = nt.NotificationName,
            })
            .ToListAsync();
        }
             
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationTypeDTO>> GetNotificationTypeById(int id)
        {
            try
            {
                var notificationType = await _context.NotificationTypes
                    .Where(nt => nt.Id == id)
                    .Select(nt => new NotificationTypeDTO
                    {
                        ID = nt.Id,
                        NotificationName = nt.NotificationName,
                    })
                    .FirstOrDefaultAsync();
                
                if(notificationType  == null)
                {
                    _logger.LogWarning("Notification type with ID{id} not found",id);
                    return NotFound($"Notification type with ID {id} not found");
                }

                return Ok(notificationType);            
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occured when fetching the project.");
                return StatusCode(500, "Internal Server Error");
            }
        }        
    }
}