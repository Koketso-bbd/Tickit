using api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
        ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
        ValidAudience = googleClientId
    };
});

builder.Services.AddDbContext<TickItDbContext>(options =>
    options.UseSqlServer($"Server={databaseServer};Database={databaseName};User Id={databaseUserId};Password={databasePassword};TrustServerCertificate=True"));

builder.Services.AddControllers()
.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; 
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("https://localhost:7151","http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TickIt", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' followed by your token in the text input below.\nExample: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
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
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

app.Run();
 