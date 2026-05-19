using System.Security.Claims;
using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Authorization;

/// <summary>
/// Role-Based Access Control (RBAC) and Permission-Based Access Control (PBAC) service interface.
/// Provides methods to check user roles, permissions, and resource ownership.
/// </summary>
public interface IRbacService
{
    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="roleName">The name of the role to verify (e.g., "Admin", "Owner", "Guest").</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    Task<bool> HasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="permissionKey">The permission key to verify (e.g., "Apartments.Read", "Bookings.Create").</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has multiple permissions (AND logic - user must have all permissions).
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="permissionKeys">The permission keys to verify.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has all permissions; otherwise, false.</returns>
    Task<bool> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions (OR logic - user must have at least one).
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="permissionKeys">The permission keys to verify.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has at least one of the permissions; otherwise, false.</returns>
    Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is the owner of a resource.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="resourceOwnerId">The ID of the resource owner.</param>
    /// <returns>True if the user owns the resource; otherwise, false.</returns>
    bool IsResourceOwner(Guid userId, Guid resourceOwnerId);

    /// <summary>
    /// Gets all active roles assigned to a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of active roles assigned to the user.</returns>
    Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions assigned to a user through their roles.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of permissions assigned to the user.</returns>
    Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is an admin or has the specified role.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="roleNameToCheck">The role name to check (if user is not admin).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if user is admin or has the specified role; otherwise, false.</returns>
    Task<bool> IsAdminOrHasRoleAsync(Guid userId, string roleNameToCheck, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is an admin or is the resource owner.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="resourceOwnerId">The ID of the resource owner.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if user is admin or owns the resource; otherwise, false.</returns>
    Task<bool> IsAdminOrResourceOwnerAsync(Guid userId, Guid resourceOwnerId, CancellationToken cancellationToken = default);

    // ========== CLAIMS-BASED AUTHORIZATION (Fast, No DB Queries) ==========

    /// <summary>
    /// Checks if a ClaimsPrincipal has a specific role (from JWT claims).
    /// Fast check without database queries - uses claims already in JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="roleName">The role name to check (e.g., "Admin", "Owner").</param>
    /// <returns>True if the principal has the role; otherwise, false.</returns>
    bool HasRoleFromClaims(ClaimsPrincipal? principal, string roleName);

    /// <summary>
    /// Checks if a ClaimsPrincipal has a specific permission (from JWT claims).
    /// Fast check without database queries - uses permission claims from JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="permissionKey">The permission key to check (e.g., "apartments:read").</param>
    /// <returns>True if the principal has the permission; otherwise, false.</returns>
    bool HasPermissionFromClaims(ClaimsPrincipal? principal, string permissionKey);

    /// <summary>
    /// Checks if a ClaimsPrincipal has all specified permissions (from JWT claims).
    /// AND logic - principal must have all permissions.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="permissionKeys">The permission keys to check.</param>
    /// <returns>True if the principal has all permissions; otherwise, false.</returns>
    bool HasAllPermissionsFromClaims(ClaimsPrincipal? principal, IEnumerable<string> permissionKeys);

    /// <summary>
    /// Checks if a ClaimsPrincipal has any of the specified permissions (from JWT claims).
    /// OR logic - principal must have at least one permission.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="permissionKeys">The permission keys to check.</param>
    /// <returns>True if the principal has at least one permission; otherwise, false.</returns>
    bool HasAnyPermissionFromClaims(ClaimsPrincipal? principal, IEnumerable<string> permissionKeys);

    /// <summary>
    /// Extracts the user ID from a ClaimsPrincipal.
    /// Looks for "sub" or "NameIdentifier" claim.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>The user ID as Guid, or Guid.Empty if not found.</returns>
    Guid GetUserIdFromClaims(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets all roles from a ClaimsPrincipal without database queries.
    /// Uses role claims already in the JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>List of role names from claims.</returns>
    List<string> GetRolesFromClaims(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets all permissions from a ClaimsPrincipal without database queries.
    /// Uses permission claims already in the JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>List of permission keys from claims.</returns>
    List<string> GetPermissionsFromClaims(ClaimsPrincipal? principal);

    /// <summary>
    /// Checks if a ClaimsPrincipal is an admin (from JWT claims).
    /// Fast bypass check - admins bypass most permission checks.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>True if the principal has Admin role; otherwise, false.</returns>
    bool IsAdminFromClaims(ClaimsPrincipal? principal);
}
