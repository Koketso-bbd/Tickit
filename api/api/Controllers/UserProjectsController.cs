using api.Data;
using api.DTOs;
using api.Models;
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
            if (userId <= 0)
            {
                _logger.LogWarning("UserID not entered.");
                return BadRequest("UserID is required.");
            }

            if (projectId <= 0)
            {
                _logger.LogWarning("ProjectID not entered.");
                return BadRequest("ProjectID is required.");
            }

            if (roleId <= 0)
            {
                _logger.LogWarning("RoleID not entered.");
                return BadRequest("RoleID is required.");
            }

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

                return Ok("User added to project successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to a project");
                return StatusCode(500, "Internal Service Error.");
            }
        }
        
    }
}