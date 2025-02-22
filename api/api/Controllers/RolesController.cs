using api.Data;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(TickItDbContext context, ILogger<RolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await  _context.Roles.ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Role>>GetRole(int id){
            
            var role = await _context.Roles.FindAsync(id);
            if(role == null)
            {
                return NotFound();
            }
            return role;
        }


        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole),new {id = role.Id},role);
        }


        // [HttpPut("{id}")]
        // public async Task<ActionResult> PutRole(int id, Role role)
        // {
        //     if (id != role.Id)
        //     {
        //         return BadRequest();
        //     }

        //     var existingRole = await _context.Roles.FindAsync(id);

        //     if (existingRole == null)
        //     {
        //         return NotFound();
        //     }

           
        //     _context.Entry(existingRole).CurrentValues.SetValues(role);

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //         return NoContent();
        //     }
        //     catch (DbUpdateConcurrencyException ex)
        //     {
               
        //         Console.WriteLine("Concurrency exception occurred.");
        //         return StatusCode(500); 
                
        //     }
        //     catch (Exception ex) when (!(ex is DbUpdateConcurrencyException))
        //     {
        //         Console.WriteLine($"An error occurred: {ex.Message}");
        //         return StatusCode(500); 
        // }


        // [HttpDelete("{id}")]
        // public async Task<ActionResult>DeleteRole(int id)
        // {
        //     var role = await _context.Roles.FindAsync(id);
        //     if(role == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Roles.Remove(role);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

    }
}
