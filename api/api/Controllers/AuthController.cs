using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login(string returnurl = "/")
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/callback"},
        GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        if (!User.Identity.IsAuthenticated) return Unauthorized();

        var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var jwtToken = await HttpContext.GetTokenAsync("id_token");

        return Ok(new {
            GoogleId = googleId,
            Email = email,
            IdToken = jwtToken 
        });
    }
}