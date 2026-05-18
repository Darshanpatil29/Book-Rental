using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Bookify.Infrastructure.Data;

namespace Bookify.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Bookify Database Context and related infrastructure services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The environment name (Development, Staging, Production).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBookifyDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environment = null)
    {
        var dbOptionsSection = configuration.GetSection(Data.DbContextOptions.SectionName);
        var dbOptions = dbOptionsSection.Get<Data.DbContextOptions>() ?? new Data.DbContextOptions();

        if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
            throw new InvalidOperationException($"Connection string not found in configuration section '{Data.DbContextOptions.SectionName}:ConnectionString'");

        if (environment != null)
            dbOptions.EnvironmentName = environment;

        services.AddSingleton(dbOptions);

        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            optionsBuilder
                .UseNpgsql(dbOptions.ConnectionString, npgsqlOptions => ConfigureNpgsqlOptions(npgsqlOptions, dbOptions))
                .ConfigureWarnings(warnings => ConfigureWarnings(warnings, dbOptions));

            // Enable detailed error messages
            if (dbOptions.EnableDetailedErrors)
                optionsBuilder.EnableDetailedErrors();

            // Enable sensitive data logging (development only)
            if (dbOptions.EnableSensitiveDataLogging)
                optionsBuilder.EnableSensitiveDataLogging();
        },
        ServiceLifetime.Scoped);

        return services;
    }

    /// <summary>
    /// Configures PostgreSQL-specific options for the Npgsql provider.
    /// </summary>
    private static void ConfigureNpgsqlOptions(
        NpgsqlDbContextOptionsBuilder npgsqlOptions,
        Data.DbContextOptions dbOptions)
    {
        npgsqlOptions.CommandTimeout(dbOptions.CommandTimeoutSeconds);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }

    /// <summary>
    /// Configures warning behavior based on the environment.
    /// </summary>
    private static void ConfigureWarnings(
        WarningsConfigurationBuilder warnings,
        Data.DbContextOptions dbOptions)
    {
        if (dbOptions.EnvironmentName == "Development")
        {
            // Log all warnings in development
            warnings.Default(WarningBehavior.Log);
        }
        else
        {
            // Throw on client-evaluated queries in production
            warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
        }
    }
}
