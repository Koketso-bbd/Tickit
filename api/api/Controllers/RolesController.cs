using api.Data;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public RolesController(TickItDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Role>> GetRoles()
        {
            return _context.Roles.ToList();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id){
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if(role == null)
            {
                return NotFound();
            }
            return new ObjectResult(role);
        }
    }
}
