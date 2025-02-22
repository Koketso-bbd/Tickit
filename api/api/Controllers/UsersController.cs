using api.Data;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(TickItDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                if (users == null || !users.Any())
                {
                    _logger.LogWarning("No users have been found.");
                    return NotFound("No users found.");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while fetching users");
                return StatusCode(500, "Internal Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found");
                    return NotFound($"User with {id} not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user with ID {id}");
                return StatusCode(500, "Internal Error");
            }
        }
    }
}
