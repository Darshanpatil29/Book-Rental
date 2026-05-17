using Bookify.Domain.Common;
using Bookify.Shared.Results;

namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Represents a role in the system with associated permissions.
/// Examples: Admin, Owner, Guest
/// </summary>
public class Role : DataEntity
{
    /// <summary>
    /// Gets the unique name of the role (e.g., "Admin", "Owner", "Guest").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of what this role represents.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the associated permissions for this role.
    /// </summary>
    public IReadOnlyCollection<Permission> Permissions
    {
        get => _permissions.AsReadOnly();
    }

    private readonly List<Permission> _permissions = new();

    /// <summary>
    /// Gets a value indicating whether this role is active and can be assigned to users.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Role"/> class.
    /// </summary>
    private Role() { }

    /// <summary>
    /// Creates a new role with the specified name and description.
    /// </summary>
    public static Result<Role> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Role>("Role name cannot be empty");

        if (name.Length > 100)
            return Result.Failure<Role>("Role name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Role>("Role description cannot be empty");

        if (description.Length > 500)
            return Result.Failure<Role>("Role description cannot exceed 500 characters");

        return Result.Success(new Role
        {
            Name = name.Trim(),
            Description = description.Trim()
        });
    }

    /// <summary>
    /// Adds a permission to this role.
    /// </summary>
    public Result AddPermission(Permission permission)
    {
        if (permission == null)
            return Result.Failure("Permission cannot be null");

        if (_permissions.Any(p => p.Id == permission.Id))
            return Result.Failure("Permission already assigned to this role");

        _permissions.Add(permission);
        return Result.Success();
    }

    /// <summary>
    /// Removes a permission from this role.
    /// </summary>
    public Result RemovePermission(Permission permission)
    {
        if (permission == null)
            return Result.Failure("Permission cannot be null");

        if (!_permissions.Remove(permission))
            return Result.Failure("Permission not found in this role");

        return Result.Success();
    }

    /// <summary>
    /// Checks if this role has a specific permission.
    /// </summary>
    public bool HasPermission(string permissionKey)
    {
        return _permissions.Any(p => p.Key == permissionKey && p.IsActive);
    }

    /// <summary>
    /// Updates the role's basic information.
    /// </summary>
    public Result UpdateInfo(string name, string description)
    {
        var result = Create(name, description);
        if (!result.IsSuccess)
            return result;

        Name = result.Value.Name;
        Description = result.Value.Description;
        ModifiedOnUtc = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Activates this role.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates this role.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedOnUtc = DateTime.UtcNow;
    }
}
