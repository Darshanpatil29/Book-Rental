namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Junction entity representing the many-to-many relationship between Roles and Permissions.
/// Used by Entity Framework Core to manage role-permission associations in the database.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Gets the role associated with this mapping.
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the permission.
    /// </summary>
    public Guid PermissionId { get; private set; }

    /// <summary>
    /// Gets the permission associated with this mapping.
    /// </summary>
    public Permission Permission { get; private set; } = null!;

    /// <summary>
    /// Gets the UTC timestamp when this mapping was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermission"/> class.
    /// </summary>
    private RolePermission() { }

    /// <summary>
    /// Creates a new role-permission mapping.
    /// </summary>
    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}
