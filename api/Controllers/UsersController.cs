using api.Data;
using api.DTOs;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpGet("{userId}/notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Gets notifications for a user")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            try
            {

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists) return StatusCode(404, new { message = "User not found"});

                var notifications = await _context.Notifications
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

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("user's notification", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }
    }
}
