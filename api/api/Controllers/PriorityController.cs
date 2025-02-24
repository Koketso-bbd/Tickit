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


    }
    
}
