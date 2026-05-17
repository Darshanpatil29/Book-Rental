using Bookify.Domain.Entities.AccessControl;
using Bookify.Shared.Results;

namespace Bookify.Domain.Services.AccessControl;

/// <summary>
/// Domain service for Role-Based Access Control (RBAC) operations.
/// Provides methods to check permissions and manage role assignments.
/// </summary>
public interface IRbacService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="permissionKey">The permission key (e.g., "Bookings.Create").</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="permissionKeys">The permission keys to check.</param>
    /// <returns>True if the user has at least one of the permissions, otherwise false.</returns>
    Task<bool> HasAnyPermissionAsync(Guid userId, string[] permissionKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all of the specified permissions.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="permissionKeys">The permission keys to check.</param>
    /// <returns>True if the user has all of the permissions, otherwise false.</returns>
    Task<bool> HasAllPermissionsAsync(Guid userId, string[] permissionKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a specific user.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles assigned to a specific user.
    /// </summary>
    Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    Task<Result> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
}
