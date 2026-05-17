namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Junction entity representing the many-to-many relationship between Users and Roles.
/// Used by Entity Framework Core to manage user-role associations in the database.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Gets the ID of the user.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the user associated with this mapping (navigation property).
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Gets the role associated with this mapping (navigation property).
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// Gets the UTC timestamp when this role assignment was created.
    /// </summary>
    public DateTime AssignedOnUtc { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp when this role assignment was removed. Null if still assigned.
    /// </summary>
    public DateTime? RemovedOnUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this role assignment is currently active.
    /// </summary>
    public bool IsActive => RemovedOnUtc == null;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRole"/> class.
    /// </summary>
    private UserRole() { }

    /// <summary>
    /// Creates a new user-role assignment.
    /// </summary>
    public static UserRole Create(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks this role assignment as removed by setting RemovedOnUtc.
    /// </summary>
    public void Remove()
    {
        RemovedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores this role assignment by clearing RemovedOnUtc.
    /// </summary>
    public void Restore()
    {
        RemovedOnUtc = null;
    }
}
