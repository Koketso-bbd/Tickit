using api.Data;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

builder.Services.AddDbContext<TickItDbContext>(options =>
    options.UseSqlServer($"Server={databaseServer};Database={databaseName};User Id={databaseUserId};Password={databasePassword};TrustServerCertificate=True"));

builder.Services.AddControllers();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; 
});

builder.Services.AddControllersWithViews(options =>
    {
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
    });
    
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TickIt API", Version = "v1" });
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

app.UseAuthorization();

app.MapControllers();

app.Run();
 