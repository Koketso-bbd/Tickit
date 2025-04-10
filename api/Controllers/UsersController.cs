using api.Data;
using api.DTOs;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(TickItDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get a user's details")]
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

                if (user == null) return NotFound( new { message = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("user", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpGet("notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Gets notifications for a user")]
        public async Task<IActionResult> GetUserNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;

                var user = await _context.Users
                    .Where(u => u.GitHubId == userId)
                    .FirstOrDefaultAsync();

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id)
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

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("user's notification", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Gets a list of all users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserDTO
                    {
                        ID = u.Id,
                        GitHubID = u.GitHubId
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("retrieving users", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }
    }
}
