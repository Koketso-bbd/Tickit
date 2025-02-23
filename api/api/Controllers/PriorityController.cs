using api.Data;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriorityController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<PriorityController> _logger;

        public PriorityController(TickItDbContext context, ILogger<PriorityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Priority>>> GetPriority()
        {
            try
            {
                var Priority = await  _context.Priorities.ToListAsync();

                if(Priority.IsNullOrEmpty()){
                    _logger.LogWarning("oOps No Priority found");
                    return NotFound("No Priority found");
                }

                return Ok(Priority);
            }
            catch(Exception)
            {
                _logger.LogError("Error occured while fetching Priority.");
                return StatusCode(500,"Internal Error");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Priority>>GetPriorityById(int id)
        {   
            try
            {
                var Priority = await _context.Priorities.FindAsync(id);
                if(Priority == null)
                {   

                    _logger.LogWarning($"No Priority found with Id: {id}.");
                    return NotFound("No role found with that Id, try again.");
                }
                
                return Ok(Priority);

            }
            catch(Exception)
            {
                _logger.LogError("Error occured while fetching a role with that specific Id.");
                return StatusCode(500,"Internal Error");
            }
        }


        [HttpPost]
        public async Task<ActionResult<Priority>> PostRole(Priority priority)
        {   
            _context.Priorities.Add(priority);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPriorityById),new {id = priority.Id},priority);

        }


        // [HttpPut("{id}")]
        // public async Task<ActionResult> PutRole(int id, Priority priority)
        // {
        //     if (id != priority.Id)
        //     {
        //         return BadRequest();
        //     }

        //     var existingRole = await _context.Priorities.FindAsync(id);

        //     if (existingRole == null)
        //     {
        //         return NotFound();
        //     }

           
        //     _context.Entry(existingRole).CurrentValues.SetValues(priority);

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
        //     var role = await _context.Priority.FindAsync(id);
        //     if(role == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Priority.Remove(role);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

    }
    
}
