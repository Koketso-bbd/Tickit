using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserProjectsController : ControllerBase
    {

        private readonly TickItDbContext _context;
        private readonly ILogger<UserProjectsController> _logger;

        public UserProjectsController(TickItDbContext context, ILogger<UserProjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /*
         * This is experimental, I want to ask Rudolph or Thabang if this is an advisible way of
         * executing our prepared statements.
         */
        [HttpPost]
        public async Task<ActionResult> AddUserToProject(int userId, int projectId,  int roleId)
        {
            if (userId <= 0) return BadRequest("UserID is required.");
            if (projectId <= 0) return BadRequest("ProjectID is required.");
            if (roleId <= 0) return BadRequest("RoleID is required.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

            if (!userExists) return BadRequest("User does not exist");
            if (!projectExists) return BadRequest("Project does not exist");
            if (!roleExists) return BadRequest("Role does not exist");

            try
            {
                bool userAlreadyInProject = await _context.UserProjects
                    .AnyAsync(up => up.ProjectId == projectId && up.MemberId == userId);

                if (userAlreadyInProject)
                {
                    _logger.LogWarning($"User {userId} already in project {projectId}.");
                    return BadRequest("User is already assigned to this project");
                }

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_AddUserToProject @p0, @p1, @p2",
                    userId, projectId, roleId
                );

                _logger.LogInformation($"{userId} added to Project: {projectId} with Role: {roleId}");
                return Ok("User added to project successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to a project");
                return StatusCode(500, "Internal Server Error.");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveUserFromProject(int userId, int projectId)
        {
            if (userId <= 0) return BadRequest("UserID is required.");
            if (projectId <= 0) return BadRequest("ProjectID is required.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);

            if (!userExists) return BadRequest("User does not exist");
            if (!projectExists) return BadRequest("Project does not exist");

            try
            {
                var userProject = await _context.UserProjects
                    .FirstOrDefaultAsync(up => up.MemberId == userId && up.ProjectId == projectId);

                if (userProject == null) return BadRequest("User not found in the specific project");

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_RemoveUserFromProject @p0, @p1",
                    userId, projectId
                    );

                _logger.LogInformation($"{userId} removed from Project: {projectId}");
                return Ok("User successfully removed from project");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from project");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserRole(int userId, int projectId, int newRoleId)
        {
            if (userId <= 0) return BadRequest("UserID is required");
            if (projectId <= 0) return BadRequest("ProjectID is required");
            if (newRoleId <= 0) return BadRequest("RoleID is required");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == newRoleId);

            if (!userExists) return BadRequest("User does not exist");
            if (!projectExists) return BadRequest("Project does not exist");
            if (!roleExists) return BadRequest("Role does not exist");

            try
            {
                var userProject = await _context.UserProjects
                    .FirstOrDefaultAsync(up => up.MemberId == userId && up.ProjectId == projectId);

                if (userProject == null) return BadRequest("User not found in this project");

                userProject.RoleId = newRoleId;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Role: {newRoleId} applied to User: {userId} in Project {projectId}");
                return Ok("User role updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user's role");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}