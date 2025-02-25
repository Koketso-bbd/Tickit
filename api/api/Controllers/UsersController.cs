using api.Data;
using api.DTOs;
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
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserDTO { ID = u.Id, GitHubID = u.GitHubId })
                    .ToListAsync();

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
                    _logger.LogWarning("User has not been found or doesn't exist");
                    return NotFound("User not found");
                }

                return Ok(user);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while fetching user");
                return StatusCode(500, "Internal Error");
            }
        }

        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetUsersProjects(int id)
        {

            var userExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!userExists) return NotFound("User does not exist");

            try
            {
                var projects = await _context.Projects
                    .Where(p => p.OwnerId == id || p.UserProjects.Any(up => up.MemberId == id))
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        OwnerID = p.OwnerId,
                        Owner = new UserDTO { ID = p.OwnerId, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                                    .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId })
                                    .ToList()
                    }).ToListAsync();

                if (projects == null) return NotFound($"No projects found for user with ID {id}");

                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retreiving user projects for user {id}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
