using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {

        private readonly TickItDbContext _context;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(TickItDbContext context, ILogger<ProjectsController> logger) 
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Select(p => new ProjectDTO
                    {
                        ID = p.Id,
                        ProjectName = p.ProjectName,
                        ProjectDescription = p.ProjectDescription,
                        OwnerID = p.OwnerId,
                        Owner = new UserDTO { ID = p.Owner.Id, GitHubID = p.Owner.GitHubId },
                        AssignedUsers = p.UserProjects
                            .Select(up => new UserDTO { ID = up.MemberId, GitHubID = up.Member.GitHubId }) // Fixed (sometimes), I think?
                            .ToList(),
                    })
                    .ToListAsync();

                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when fetching projects.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}