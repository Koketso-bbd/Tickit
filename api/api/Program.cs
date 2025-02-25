using api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add User secrets
builder.Configuration.AddUserSecrets<Program>();

// Database connection
builder.Services.AddDbContext<TickItDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie()
.AddOAuth("GitHub", options =>
{
    options.ClientId = builder.Configuration["GitHub:ClientId"];
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"];
    options.CallbackPath = new PathString("/auth/github/callback");

    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";

    options.ClaimActions.MapJsonKey("urn:github:login", "login");
    options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
    options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.Add("User-Agent", "GitHubOAuth");

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            context.RunClaimActions(user.RootElement);
        }
    };
});

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
    });
}

app.UseRouting();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", async context =>
{
    if (context.User.Identity?.IsAuthenticated ?? false)
    {
        var username = context.User.FindFirst("urn:github:login")?.Value;
        await context.Response.WriteAsync($"Hello {username}");
    }
    else
    {
        await context.Response.WriteAsync("Not signed in. <a href='/signin'>Sign in with GitHub</a>");
    }
});

app.MapGet("/signin", async context =>
{
    await context.ChallengeAsync("GitHub", new AuthenticationProperties
    {
        RedirectUri = "/auth/github/callback"
    });
});


app.Run();
