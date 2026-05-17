namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Repository interface for user role assignment persistence and querying.
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// Gets all active roles for a specific user.
    /// </summary>
    Task<IReadOnlyList<Role>> GetActiveRolesForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all role assignments (including inactive) for a specific user.
    /// </summary>
    Task<IReadOnlyList<UserRole>> GetAllRoleAssignmentsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user-role assignment.
    /// </summary>
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role assigned.
    /// </summary>
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a role with the specified name.
    /// </summary>
    Task<bool> UserHasRoleByNameAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with a specific role.
    /// </summary>
    Task<IReadOnlyList<User>> GetUsersWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a role assignment for a user.
    /// </summary>
    void AddUserRole(UserRole userRole);

    /// <summary>
    /// Updates a user role assignment (e.g., soft delete by setting RemovedOnUtc).
    /// </summary>
    void UpdateUserRole(UserRole userRole);

    /// <summary>
    /// Removes a user role assignment.
    /// </summary>
    void RemoveUserRole(UserRole userRole);

    /// <summary>
    /// Gets the count of users assigned to a specific role.
    /// </summary>
    Task<int> GetUserCountForRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
