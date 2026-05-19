using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bookify.Infrastructure.Extensions;

namespace Bookify.Infrastructure;

/// <summary>
/// Configuration for the Infrastructure layer.
/// Registers all infrastructure services including database, caching, and external integrations.
/// </summary>
public static class InfrastructureConfiguration
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The environment name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environment = null)
    {
        // Register database context and related services
        services.AddBookifyDbContext(configuration, environment);

        // Register JWT authentication services (token generation, validation, blacklisting)
        services.AddJwtAuthentication();

        // Register JWT Bearer authentication scheme and token validation
        services.AddJwtBearerScheme(configuration);

        // Register authorization policies (RBAC, PBAC)
        services.AddAuthorizationPolicies();

        // Register authorization services (RbacService for permission/role checking)
        services.AddAuthorizationServices();

        // TODO: Add other infrastructure services here:
        // - Repository implementations
        // - Caching services (Redis)
        // - Google OAuth services
        // - Background job services (Quartz)
        // - Email service
        // - Payment service integration

        return services;
    }
}

