using System.Security.Claims;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TickItDbContext _context;
    public AuthController(TickItDbContext context)
    {
        _context = context;
    }

    private async Task<bool> IsFrontendRunning()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://localhost:4200");
                return response.IsSuccessStatusCode;
            }
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/callback" }, 
        "GoogleOpenIdConnect"); 
    }

    [HttpGet("callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized();

        var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = authResult.Properties;
        var tokens = properties?.GetTokens();

        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var idToken = tokens?.FirstOrDefault(t => t.Name == "id_token")?.Value;

        if (string.IsNullOrEmpty(googleId)) return BadRequest(new { message = "Invalid Google ID" });

        try
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == email);

            if (existingUser == null)
            {
                var newUser = new User { GitHubId = email };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }

            bool isFrontendRunning = await IsFrontendRunning();

            if (isFrontendRunning)
            {
                var redirectUrl = $"http://localhost:4200/google-callback?token={idToken}";
                return Redirect(redirectUrl);
            }

            return Ok(new 
            { 
                GoogleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                IdToken = idToken
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An unexpected error occurred while attempting to add user: {ex.Message}" });
        }
    }
}