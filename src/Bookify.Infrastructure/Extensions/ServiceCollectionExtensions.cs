using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System.Text;
using Bookify.Infrastructure.Configuration;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Services.Authentication;
using Bookify.Infrastructure.Services.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Bookify.Domain.Entities.AccessControl;

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

    /// <summary>
    /// Adds JWT-based authentication services to the DI container.
    /// Includes JwtTokenProvider for token generation/validation and TokenBlacklistService for revocation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services)
    {
        // Bind and validate authentication options from configuration
        services.AddOptions<AuthenticationOptions>()
        .BindConfiguration(AuthenticationOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

        services.AddScoped<JwtTokenProvider>();
        // services.AddScoped<TokenBlacklistService>();
        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication scheme to the authentication builder.
    /// Configures token validation parameters, events, and error handling.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtBearerScheme(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authOptionsSection = configuration.GetSection(AuthenticationOptions.SectionName);
        var authOptions = authOptionsSection.Get<AuthenticationOptions>();

        if (authOptions?.Jwt == null)
            throw new InvalidOperationException(
                $"JWT options not found in configuration section '{AuthenticationOptions.SectionName}:Jwt'");

        if (string.IsNullOrWhiteSpace(authOptions.Jwt.Secret))
            throw new InvalidOperationException("JWT Secret is not configured");

        if (authOptions.Jwt.Secret.Length < 32)
            throw new InvalidOperationException(
                "JWT Secret must be at least 32 characters long for HMAC-SHA256 security");

        var jwtKey = Encoding.UTF8.GetBytes(authOptions.Jwt.Secret);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = authOptions.Jwt.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey),

                    ValidateIssuer = authOptions.Jwt.ValidateIssuer,
                    ValidIssuer = authOptions.Jwt.Issuer,

                    ValidateAudience = authOptions.Jwt.ValidateAudience,
                    ValidAudience = authOptions.Jwt.Audience,

                    ValidateLifetime = authOptions.Jwt.ValidateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(authOptions.Jwt.ClockSkewSeconds),

                    RequireExpirationTime = true
                };

                // Log token validation errors
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning(
                            context.Exception,
                            "Authentication failed for request to {Path}",
                            context.HttpContext.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogDebug("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    /// Adds authorization policies for role-based access control.
    /// Fine-grained permissions are managed through RbacService and AccessControl database entities.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Role-based policies (coarse-grained, quick checks)
            options.AddPolicy("Admin", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("Owner", policy =>
                policy.RequireRole("Owner", "Admin"));

            // Note: Fine-grained permissions (apartments:read, apartments:create, etc.)
            // are managed through RbacService.HasPermissionAsync() and database AccessControl entities
            // This ensures a single source of truth and dynamic permission management
        });

        return services;
    }

    /// <summary>
    /// Adds authorization services including RBAC (Role-Based Access Control).
    /// Registers RbacService for role and permission checking in service layer.
    /// Authorization is two-layered: 
    /// Layer 1 (Controller): [Authorize(Policy = "Admin")] checks JWT claims
    /// Layer 2 (Service): Entity access mask validation in business logic
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthorizationServices(
        this IServiceCollection services)
    {
        // Register repositories for access control
        services.AddScoped<IUserRoleRepository, Bookify.Infrastructure.Repositories.UserRoleRepository>();
        services.AddScoped<IPermissionRepository, Bookify.Infrastructure.Repositories.PermissionRepository>();
        services.AddScoped<IRoleRepository, Bookify.Infrastructure.Repositories.RoleRepository>();

        // Register RbacService for role/permission checking in service layer
        services.AddScoped<IRbacService, RbacService>();

        return services;
    }
}
