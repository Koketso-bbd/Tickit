using api.Data;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(TickItDbContext context, ILogger<RolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            try
            {
                var roles = await  _context.Roles.ToListAsync();

                if(roles.IsNullOrEmpty()){
                    var message = "No roles found";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(roles);
            }
            catch(Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorFetching("roles", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }
    }
}
