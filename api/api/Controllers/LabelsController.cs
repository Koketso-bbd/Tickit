using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<LabelsController> _logger;

        public LabelsController(TickItDbContext context, ILogger<LabelsController> logger)
        {
            _context = context;
            _logger = logger;
        }
    }
}