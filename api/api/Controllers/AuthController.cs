using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
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
        
        var idToken = tokens?.FirstOrDefault(t => t.Name == "id_token")?.Value;
        var accessToken = tokens?.FirstOrDefault(t => t.Name == "access_token")?.Value;
        
        return Ok(new 
        { 
            GoogleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            IdToken = idToken
        });
    }
}