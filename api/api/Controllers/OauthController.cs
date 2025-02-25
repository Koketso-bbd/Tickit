using System.Security.Claims;
using api.Data;
using api.DTOs;
using api.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly TickItDbContext _context;
        private readonly ILogger<OauthController> _logger;

        public OauthController(TickItDbContext context, ILogger<OauthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/auth/github/callback"
            }, "GitHub");
        }

        [HttpGet("auth/github/callback")]
        public IActionResult GitHubCallback()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims); // Return claims or redirect to your frontend
        }

    }

}
