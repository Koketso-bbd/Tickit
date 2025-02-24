using api.Data;
using api.Models;
using Microsoft.AspNetCore.Http;
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
                    _logger.LogWarning("oOps No roles found");
                    return NotFound("No roles found");
                }

                return Ok(roles);
            }
            catch(Exception)
            {
                _logger.LogError("Error occured while fetching roles.");
                return StatusCode(500,"Internal Error");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Role>>GetRoleById(int id)
        {   
            try
            {
                var roles = await _context.Roles.FindAsync(id);
                if(roles == null)
                {   

                    _logger.LogWarning($"No roles found with Id: {id}.");
                    return NotFound("No role found with that Id, try again.");
                }
                
                return Ok(roles);

            }
            catch(Exception)
            {
                _logger.LogError("Error occured while fetching a role with that specific Id.");
                return StatusCode(500,"Internal Error");
            }
        }

    }
}
