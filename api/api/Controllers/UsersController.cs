using api.Data;
using api.DTOs;
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
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserDTO { ID = u.Id, GitHubID = u.GitHubId })
                    .ToListAsync();

                if (users == null || !users.Any())
                {
                    var message = "No users found.";
                    _logger.LogWarning(message);
                    return NotFound(message);
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
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id)
                    .Select(u => new UserDTO { ID = u.Id, GitHubID = u.GitHubId })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    var message = "User not found";
                    _logger.LogWarning(message);
                    return NotFound(message);
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while fetching user");
                return StatusCode(500, "Internal Error");
            }
        }
    }
}
