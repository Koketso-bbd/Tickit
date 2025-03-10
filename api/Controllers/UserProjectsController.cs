using api.Data;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserProjectsController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<UserProjectsController> _logger;

        public UserProjectsController(TickItDbContext context, ILogger<UserProjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Adds a user to a project")]
        public async Task<ActionResult> AddUserToProject([Required] int userId, [Required] int projectId, [Required] int roleId)
        {
            try
            {
                var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var currentUser = await _context.Users
                    .Where(u => u.GitHubId == currentEmail)
                    .FirstOrDefaultAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User not found in the system" });
                }

                var currentUserId = currentUser.Id;

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

                var availableRoles = await _context.Roles
                    .Select(r => new { r.Id, r.RoleName })
                    .ToListAsync();
                var availableRolesMessage = string.Join(", ", availableRoles.Select(r => $"({r.Id}){r.RoleName}"));

                if (!userExists) return NotFound(new { message = "User does not exist" });
                if (project == null) return NotFound(new { message = "Project does not exist" });
                if (!roleExists) return NotFound(new { message = $"Role does not exist. Available roles are: {availableRolesMessage}" });

                bool userAlreadyInProject = await _context.UserProjects
                    .AnyAsync(up => up.ProjectId == projectId && up.MemberId == userId);

                if (userAlreadyInProject) return BadRequest(new { message = "User is already assigned to this project" });

                bool isProjectOwner = project.OwnerId == currentUserId;
                bool isAdmin = await _context.UserProjects
                        .AnyAsync(ur => ur.MemberId == currentUserId && ur.RoleId == 1);

                if (!isProjectOwner && !isAdmin)
                {
                    return StatusCode(403, new { message = "You don't have permission to add users to this project" });
                }

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_AddUserToProject @p0, @p1, @p2",
                    userId, projectId, roleId
                );

                return Ok(new { message = "User added to project successfully" });
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("adding user to a project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }


        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Removes a user from a project")]
        public async Task<ActionResult> RemoveUserFromProject([Required] int userId, [Required] int projectId)
        {
            if (userId <= 0) return BadRequest(new { message = "UserID is required." });
            if (projectId <= 0) return BadRequest(new { message = "ProjectID is required." });

            try
            {
                var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var currentUser = await _context.Users
                    .Where(u => u.GitHubId == currentEmail)
                    .FirstOrDefaultAsync();

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User not found." });
                }

                var currentUserId = currentUser.Id;

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                var project = await _context.Projects
                    .Where(p => p.Id == projectId)
                    .FirstOrDefaultAsync();

                if (!userExists) return NotFound(new { message = "User does not exist" });
                if (project == null) return NotFound(new { message = "Project does not exist" });

                var userProject = await _context.UserProjects
                    .FirstOrDefaultAsync(up => up.MemberId == userId && up.ProjectId == projectId);

                if (userProject == null) return NotFound(new { message = "User not found in the specific project" });

                bool isRemovingSelf = currentUserId == userId;
                bool isProjectOwner = project.OwnerId == currentUserId;
                bool isAdmin = await _context.UserProjects
                    .AnyAsync(ur => ur.MemberId == currentUserId && ur.RoleId == 1);

                if (!isRemovingSelf && !isProjectOwner && !isAdmin)
                {
                    return StatusCode(403, new { message = "You do not have permission to remove users from this project" });
                }

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_RemoveUserFromProject @p0, @p1",
                    userId, projectId
                );

                return Ok(new { message = "User successfully removed from project" });
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("removing user from project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update a user's role in the project")]
        public async Task<ActionResult> UpdateUserRole([Required] int userId, [Required] int projectId, [Required] int newRoleId)
        {
            try
            {
                var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == currentEmail);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User not found in the system" });
                }

                var currentUserId = currentUser.Id;

                var availableRoles = await _context.Roles
                    .Select(r => new { r.Id, r.RoleName })
                    .ToListAsync();
                var availableRolesMessage = string.Join(", ", availableRoles.Select(r => $"({r.Id}){r.RoleName}"));

                if (userId <= 0) return BadRequest(new { message = "UserID is required" });
                if (projectId <= 0) return BadRequest(new { message = "ProjectID is required" });
                if (newRoleId <= 0) return BadRequest(new { message = $"RoleID is required. Available roles: {availableRolesMessage}" });

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == newRoleId);

                if (!userExists) return NotFound(new { message = "User does not exist" });
                if (project == null) return NotFound(new { message = "Project does not exist" });
                if (!roleExists) return NotFound(new { message = $"Role does not exist. Available roles: {availableRolesMessage}" });

                var userProject = await _context.UserProjects.FirstOrDefaultAsync(up => up.MemberId == userId && up.ProjectId == projectId);
                if (userProject == null) return NotFound(new { message = "User not found in this project" });

                bool isProjectOwner = project.OwnerId == currentUserId;
                bool isAdmin = await _context.UserProjects
                        .AnyAsync(ur => ur.MemberId == currentUserId && ur.RoleId == 1);

                if (!isProjectOwner && !isAdmin)
                {
                    return StatusCode(403, new { message = "You do not have permission to modify this project" });
                }

                userProject.RoleId = newRoleId;
                await _context.SaveChangesAsync();

                return Ok(new { message = "User role updated successfully." });
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError("updating user's role", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }
    }
}