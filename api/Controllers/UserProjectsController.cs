using api.Data;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Adds a user to a project")]
        public async Task<ActionResult> AddUserToProject(int userId, int projectId,  int roleId)
        {
            if (userId <= 0) return BadRequest(new { message = "UserID is required." });
            if (projectId <= 0) return BadRequest(new { message = "ProjectID is required." });
            if (roleId <= 0) return BadRequest(
                new 
                { message = "RoleID is required.Options to choose from: (1.Admin)(2.Contributor)(3.Viewer)" });
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

            if (!userExists) return NotFound(new { message = "User does not exist" });
            if (!projectExists) return NotFound(new { message = "Project does not exist" });
            if (!roleExists) return NotFound(new { message = "Role does not exist.Choose from these options:(1.Admin)(2.Contributor)(3.Viewer)" });

            try
            {
                bool userAlreadyInProject = await _context.UserProjects
                    .AnyAsync(up => up.ProjectId == projectId && up.MemberId == userId);

                if (userAlreadyInProject) return BadRequest(new { message = "User is already assigned to this project" });

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_AddUserToProject @p0, @p1, @p2",
                    userId, projectId, roleId
                );

                return Ok("User added to project successfully");
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Removes a user from a project")]
        public async Task<ActionResult> RemoveUserFromProject(int userId, int projectId)
        {
            if (userId <= 0) return BadRequest(new { message = "UserID is required." });
            if (projectId <= 0) return BadRequest(new { message = "ProjectID is required." });

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);

            if (!userExists) return NotFound(new { message = "User does not exist" });
            if (!projectExists) return NotFound(new { message = "Project does not exist" });

            try
            {
                var userProject = await _context.UserProjects
                    .FirstOrDefaultAsync(up => up.MemberId == userId && up.ProjectId == projectId);

                if (userProject == null) return NotFound(new { message = "User not found in the specific project" });

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_RemoveUserFromProject @p0, @p1",
                    userId, projectId
                    );

                return Ok("User successfully removed from project");
            }
            catch (Exception ex)
            {
                var (statusCode, errorMessage) = HttpResponseHelper.InternalServerError(" deleting user from project", _logger, ex);
                return StatusCode(statusCode, new { message = errorMessage });
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update a user's role in the project")]
        public async Task<ActionResult> UpdateUserRole(int userId, int projectId, int newRoleId)
        {
            if (userId <= 0) return BadRequest(new { message = "UserID is required" });
            if (projectId <= 0) return BadRequest(new { message = "ProjectID is required" });
            if (newRoleId <= 0) return BadRequest(new { message = "RoleID is required.Choose from these options:(1.Admin)(2.Contributor)(3.Viewer)" });

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == newRoleId);

            if (!userExists) return NotFound(new { message = "User does not exist" });
            if (!projectExists) return NotFound(new { message = "Project does not exist" });
            if (!roleExists) return NotFound(new { message = "Role does not exist.Choose from these options:(1.Admin)(2.Contributor)(3.Viewer)" });

            try
            {
                var userProject = await _context.UserProjects
                    .FirstOrDefaultAsync(up => up.MemberId == userId && up.ProjectId == projectId);

                if (userProject == null) return NotFound(new { message = "User not found in this project" });

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