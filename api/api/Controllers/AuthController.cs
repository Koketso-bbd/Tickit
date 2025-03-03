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
}