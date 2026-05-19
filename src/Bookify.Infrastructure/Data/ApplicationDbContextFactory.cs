using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bookify.Infrastructure.Data;

/// <summary>
/// Design-time factory for ApplicationDbContext.
/// Used by Entity Framework Core tools (migrations, scaffolding) at design time.
/// This avoids DI container issues during migration generation.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Creates an instance of ApplicationDbContext for design-time operations.
    /// </summary>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings files
        var configuration = new ConfigurationBuilder()
            .SetBasePath(GetProjectRootPath())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        // Get database options
        var dbOptionsSection = configuration.GetSection(DbContextOptions.SectionName);
        var dbOptions = dbOptionsSection.Get<DbContextOptions>() ?? new DbContextOptions();

        if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
            throw new InvalidOperationException(
                $"Connection string not found in configuration section '{DbContextOptions.SectionName}:ConnectionString'");

        // Build DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            dbOptions.ConnectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(dbOptions.CommandTimeoutSeconds);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

        if (dbOptions.EnableDetailedErrors)
            optionsBuilder.EnableDetailedErrors();

        if (dbOptions.EnableSensitiveDataLogging)
            optionsBuilder.EnableSensitiveDataLogging();

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the project root path by traversing up from the current directory.
    /// </summary>
    private static string GetProjectRootPath()
    {
        // Start from the Infrastructure project directory and go up to the root
        var currentPath = Directory.GetCurrentDirectory();

        // If we're in the Infrastructure project, go to the API project's location
        if (currentPath.EndsWith("Bookify.Infrastructure"))
            currentPath = Path.Combine(currentPath, "..", "..", "src", "Bookify.API");

        // Traverse up to find appsettings.json
        var dir = new DirectoryInfo(currentPath);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }
}
