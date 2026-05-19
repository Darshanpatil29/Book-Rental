using Serilog;
using Bookify.Infrastructure;
using Bookify.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, config) =>
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .Enrich.FromLogContext()
);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bookify API",
        Version = "v1",
        Description = "Apartment Rental Booking System API"
    });
});

builder.Services.AddHealthChecks();

// Infrastructure services (Database, JWT Authentication, Authorization, Caching, etc.)
builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment.EnvironmentName);

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
