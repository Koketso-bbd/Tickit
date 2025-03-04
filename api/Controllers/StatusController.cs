using api.Data;
using api.DTOs;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            try
            {
                return await _context.Statuses
                       .Select(s => new StatusDTO
                       {
                           Id = s.Id,
                           StatusName = s.StatusName,
                       })
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerError("fetching statuses", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }        
    }
}