namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Repository interface for role persistence and querying.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by its ID, including all associated permissions.
    /// </summary>
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its name, including all associated permissions.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IReadOnlyList<Role>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles including inactive ones.
    /// </summary>
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles assigned to a specific user.
    /// </summary>
    Task<IReadOnlyList<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new role to the repository.
    /// </summary>
    void Add(Role role);

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    void Update(Role role);

    /// <summary>
    /// Removes a role from the repository.
    /// </summary>
    void Remove(Role role);

    /// <summary>
    /// Checks if a role with the specified name exists.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
