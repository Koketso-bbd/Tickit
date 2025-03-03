using api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add secrets.json
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Properties/secrets.json", optional:true, reloadOnChange:true);

/*
 * This code here establishes a connection to our database ;)
 */
string databaseServer = builder.Configuration["DBSERVER"];
string databaseName = builder.Configuration["DBNAME"];
string databaseUserId = builder.Configuration["DBUSERID"];
string databasePassword = builder.Configuration["DBPASSWORD"];
string googleClientId = builder.Configuration["CLIENTID"];
string googleClientSecret = builder.Configuration["CLIENTSECRET"];

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GoogleOpenIdConnect";
})
.AddCookie()
.AddOpenIdConnect("GoogleOpenIdConnect", options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.Authority = "https://accounts.google.com";
    options.ResponseType = "code id_token";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.Authority = "https://accounts.google.com";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
        ValidAudience = googleClientId
    };
});

builder.Services.AddDbContext<TickItDbContext>(options =>
    options.UseSqlServer($"Server={databaseServer};Database={databaseName};User Id={databaseUserId};Password={databasePassword};TrustServerCertificate=True"));

builder.Services.AddControllers();


builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; 
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TickIt API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
        options.OAuthClientId(googleClientId);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
 