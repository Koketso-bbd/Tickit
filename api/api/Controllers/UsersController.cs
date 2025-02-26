using api.Data;
using api.DTOs;
using api.Helpers;
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
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("users", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id)
                    .Select(u => new UserDTO {
                        ID = u.Id,
                        GitHubID = u.GitHubId,
                        })
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
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("user", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }

        [HttpGet("{userId}/notifications")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {   
            try
            {
                var user = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .Select(n => new NotificationsDTO
                    {
                        Id = n.Id,
                        UserId = n.UserId,
                        ProjectId = n.ProjectId,
                        TaskId = n.TaskId,
                        NotificationTypeId = n.NotificationTypeId,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    })
                    .ToListAsync();

                if (user == null)
                {
                    _logger.LogWarning("User has not been found or doesn't exist");
                    return NotFound("User not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("user's notification", _logger, ex);
                return StatusCode(statusCode, message);
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
                var (statusCode, message) = HttpResponseHelper.InternalServerErrorGet("user's projects", _logger, ex);
                return StatusCode(statusCode, message);
            }
        }
    }
}
