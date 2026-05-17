namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Repository interface for permission persistence and querying.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Gets a permission by its ID.
    /// </summary>
    Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by its key (e.g., "Bookings.Create").
    /// </summary>
    Task<Permission?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active permissions.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions including inactive ones.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a specific feature (e.g., "Bookings").
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByFeatureAsync(
        string feature,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions assigned to a specific role.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions for a specific user (from their assigned roles).
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new permission to the repository.
    /// </summary>
    void Add(Permission permission);

    /// <summary>
    /// Updates an existing permission.
    /// </summary>
    void Update(Permission permission);

    /// <summary>
    /// Removes a permission from the repository.
    /// </summary>
    void Remove(Permission permission);

    /// <summary>
    /// Checks if a permission with the specified key exists.
    /// </summary>
    Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple permissions in bulk. Useful for seeding initial permissions.
    /// </summary>
    void AddRange(IEnumerable<Permission> permissions);
}
