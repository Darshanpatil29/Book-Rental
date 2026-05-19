using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Authorization;

/// <summary>
/// Role-Based Access Control (RBAC) and Permission-Based Access Control (PBAC) service.
/// Provides methods to check user roles, permissions, and resource ownership.
/// </summary>
public sealed class RbacService : IRbacService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RbacService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RbacService"/> class.
    /// </summary>
    public RbacService(
        IUserRoleRepository userRoleRepository,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        ILogger<RbacService> logger)
    {
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="roleName">The name of the role to verify (e.g., "Admin", "Owner", "Guest").</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    public async Task<bool> HasRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("HasRoleAsync called with empty userId");
            return false;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            _logger.LogWarning("HasRoleAsync called with empty roleName");
            return false;
        }

        try
        {
            var hasRole = await _userRoleRepository.UserHasRoleByNameAsync(userId, roleName, cancellationToken);
            _logger.LogDebug("User {UserId} role check for '{RoleName}': {HasRole}", userId, roleName, hasRole);
            return hasRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role '{RoleName}' for user {UserId}", roleName, userId);
            return false;
        }
    }

    /// <summary>
    /// Checks if a user has a specific permission.
    /// Queries all permissions assigned to the user through their roles.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="permissionKey">The permission key to verify (e.g., "Apartments.Read", "Bookings.Create").</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permissionKey,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("HasPermissionAsync called with empty userId");
            return false;
        }

        if (string.IsNullOrWhiteSpace(permissionKey))
        {
            _logger.LogWarning("HasPermissionAsync called with empty permissionKey");
            return false;
        }

        try
        {
            var userPermissions = await _permissionRepository.GetByUserIdAsync(userId, cancellationToken);
            var hasPermission = userPermissions.Any(p => p.Key == permissionKey && p.IsActive);

            _logger.LogDebug("User {UserId} permission check for '{PermissionKey}': {HasPermission}",
                userId, permissionKey, hasPermission);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission '{PermissionKey}' for user {UserId}",
                permissionKey, userId);
            return false;
        }
    }

    /// <summary>
    /// Checks if a user has multiple permissions (AND logic - user must have all permissions).
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="permissionKeys">The permission keys to verify.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has all permissions; otherwise, false.</returns>
    public async Task<bool> HasAllPermissionsAsync(
        Guid userId,
        IEnumerable<string> permissionKeys,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || !permissionKeys?.Any() == true)
            return false;

        foreach (var permissionKey in permissionKeys)
        {
            var hasPermission = await HasPermissionAsync(userId, permissionKey, cancellationToken);
            if (!hasPermission)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a user has any of the specified permissions (OR logic - user must have at least one).
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="permissionKeys">The permission keys to verify.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the user has at least one of the permissions; otherwise, false.</returns>
    public async Task<bool> HasAnyPermissionAsync(
        Guid userId,
        IEnumerable<string> permissionKeys,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || !permissionKeys?.Any() == true)
            return false;

        foreach (var permissionKey in permissionKeys)
        {
            var hasPermission = await HasPermissionAsync(userId, permissionKey, cancellationToken);
            if (hasPermission)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a user is the owner of a resource.
    /// Compares the user ID with the resource owner ID.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="resourceOwnerId">The ID of the resource owner.</param>
    /// <returns>True if the user owns the resource; otherwise, false.</returns>
    public bool IsResourceOwner(Guid userId, Guid resourceOwnerId)
    {
        var isOwner = userId == resourceOwnerId;
        _logger.LogDebug("Resource ownership check: User {UserId} vs Owner {ResourceOwnerId} = {IsOwner}",
            userId, resourceOwnerId, isOwner);
        return isOwner;
    }

    /// <summary>
    /// Gets all active roles assigned to a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of active roles assigned to the user.</returns>
    public async Task<IReadOnlyList<Role>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("GetUserRolesAsync called with empty userId");
            return [];
        }

        try
        {
            var roles = await _userRoleRepository.GetActiveRolesForUserAsync(userId, cancellationToken);
            _logger.LogDebug("Retrieved {RoleCount} active roles for user {UserId}", roles.Count, userId);
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            return [];
        }
    }

    /// <summary>
    /// Gets all permissions assigned to a user through their roles.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of permissions assigned to the user.</returns>
    public async Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("GetUserPermissionsAsync called with empty userId");
            return [];
        }

        try
        {
            var permissions = await _permissionRepository.GetByUserIdAsync(userId, cancellationToken);
            var activePermissions = permissions.Where(p => p.IsActive).ToList();
            _logger.LogDebug("Retrieved {PermissionCount} active permissions for user {UserId}",
                activePermissions.Count, userId);
            return activePermissions.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            return [];
        }
    }

    /// <summary>
    /// Checks if a user is an admin or has the specified role.
    /// Admins bypass most permission checks.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="roleNameToCheck">The role name to check (if user is not admin).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if user is admin or has the specified role; otherwise, false.</returns>
    public async Task<bool> IsAdminOrHasRoleAsync(
        Guid userId,
        string roleNameToCheck,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = await HasRoleAsync(userId, "Admin", cancellationToken);
        if (isAdmin)
            return true;

        return await HasRoleAsync(userId, roleNameToCheck, cancellationToken);
    }

    /// <summary>
    /// Checks if a user is an admin or is the resource owner.
    /// Admins and owners can modify resources they own or are admins of.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <param name="resourceOwnerId">The ID of the resource owner.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if user is admin or owns the resource; otherwise, false.</returns>
    public async Task<bool> IsAdminOrResourceOwnerAsync(
        Guid userId,
        Guid resourceOwnerId,
        CancellationToken cancellationToken = default)
    {
        if (IsResourceOwner(userId, resourceOwnerId))
            return true;

        return await HasRoleAsync(userId, "Admin", cancellationToken);
    }

    // ========== CLAIMS-BASED AUTHORIZATION (Fast, No DB Queries) ==========

    /// <summary>
    /// Checks if a ClaimsPrincipal has a specific role (from JWT claims).
    /// Fast check without database queries - uses claims already in JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="roleName">The role name to check (e.g., "Admin", "Owner").</param>
    /// <returns>True if the principal has the role; otherwise, false.</returns>
    public bool HasRoleFromClaims(ClaimsPrincipal? principal, string roleName)
    {
        if (principal == null || string.IsNullOrWhiteSpace(roleName))
        {
            _logger.LogWarning("HasRoleFromClaims called with null principal or empty roleName");
            return false;
        }

        var hasRole = principal.IsInRole(roleName);
        _logger.LogDebug("Claims-based role check for '{RoleName}': {HasRole}", roleName, hasRole);
        return hasRole;
    }

    /// <summary>
    /// Checks if a ClaimsPrincipal has a specific permission (from JWT claims).
    /// Fast check without database queries - uses permission claims from JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="permissionKey">The permission key to check (e.g., "apartments:read").</param>
    /// <returns>True if the principal has the permission; otherwise, false.</returns>
    public bool HasPermissionFromClaims(ClaimsPrincipal? principal, string permissionKey)
    {
        if (principal == null || string.IsNullOrWhiteSpace(permissionKey))
        {
            _logger.LogWarning("HasPermissionFromClaims called with null principal or empty permissionKey");
            return false;
        }

        var hasPermission = principal.HasClaim("permission", permissionKey);
        _logger.LogDebug("Claims-based permission check for '{PermissionKey}': {HasPermission}",
            permissionKey, hasPermission);
        return hasPermission;
    }

    /// <summary>
    /// Checks if a ClaimsPrincipal has all specified permissions (from JWT claims).
    /// AND logic - principal must have all permissions.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="permissionKeys">The permission keys to check.</param>
    /// <returns>True if the principal has all permissions; otherwise, false.</returns>
    public bool HasAllPermissionsFromClaims(ClaimsPrincipal? principal, IEnumerable<string> permissionKeys)
    {
        if (principal == null || !permissionKeys?.Any() == true)
            return false;

        foreach (var permissionKey in permissionKeys)
        {
            if (!HasPermissionFromClaims(principal, permissionKey))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a ClaimsPrincipal has any of the specified permissions (from JWT claims).
    /// OR logic - principal must have at least one permission.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <param name="permissionKeys">The permission keys to check.</param>
    /// <returns>True if the principal has at least one permission; otherwise, false.</returns>
    public bool HasAnyPermissionFromClaims(ClaimsPrincipal? principal, IEnumerable<string> permissionKeys)
    {
        if (principal == null || !permissionKeys?.Any() == true)
            return false;

        foreach (var permissionKey in permissionKeys)
        {
            if (HasPermissionFromClaims(principal, permissionKey))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Extracts the user ID from a ClaimsPrincipal.
    /// Looks for "sub" or "NameIdentifier" claim.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>The user ID as Guid, or Guid.Empty if not found.</returns>
    public Guid GetUserIdFromClaims(ClaimsPrincipal? principal)
    {
        if (principal == null)
            return Guid.Empty;

        // Try "sub" claim first (standard JWT subject claim)
        var subClaim = principal.FindFirst("sub");
        if (subClaim?.Value != null && Guid.TryParse(subClaim.Value, out var userId))
        {
            _logger.LogDebug("Extracted user ID from 'sub' claim: {UserId}", userId);
            return userId;
        }

        // Fall back to NameIdentifier
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim?.Value != null && Guid.TryParse(idClaim.Value, out userId))
        {
            _logger.LogDebug("Extracted user ID from NameIdentifier claim: {UserId}", userId);
            return userId;
        }

        _logger.LogWarning("Could not extract user ID from claims");
        return Guid.Empty;
    }

    /// <summary>
    /// Gets all roles from a ClaimsPrincipal without database queries.
    /// Uses role claims already in the JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>List of role names from claims.</returns>
    public List<string> GetRolesFromClaims(ClaimsPrincipal? principal)
    {
        if (principal == null)
            return [];

        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        _logger.LogDebug("Retrieved {RoleCount} roles from claims", roles.Count);
        return roles;
    }

    /// <summary>
    /// Gets all permissions from a ClaimsPrincipal without database queries.
    /// Uses permission claims already in the JWT token.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>List of permission keys from claims.</returns>
    public List<string> GetPermissionsFromClaims(ClaimsPrincipal? principal)
    {
        if (principal == null)
            return [];

        var permissions = principal.FindAll("permission").Select(c => c.Value).ToList();
        _logger.LogDebug("Retrieved {PermissionCount} permissions from claims", permissions.Count);
        return permissions;
    }

    /// <summary>
    /// Checks if a ClaimsPrincipal is an admin (from JWT claims).
    /// Fast bypass check - admins bypass most permission checks.
    /// </summary>
    /// <param name="principal">The claims principal (from HttpContext.User).</param>
    /// <returns>True if the principal has Admin role; otherwise, false.</returns>
    public bool IsAdminFromClaims(ClaimsPrincipal? principal)
    {
        return HasRoleFromClaims(principal, "Admin");
    }
}