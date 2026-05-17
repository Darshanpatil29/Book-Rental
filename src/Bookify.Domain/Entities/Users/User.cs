using Bookify.Shared.ValueObjects;
using Bookify.Domain.Common;
using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Domain.Entities;

/// <summary>
/// Represents a system user (guest or property owner).
/// </summary>
public class User : DataEntity
{
    /// <summary>
    /// Gets the user's email address. Used as unique identifier.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's password hash (bcrypt or similar).
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets the user's assigned roles (many-to-many relationship).
    /// </summary>
    public IReadOnlyCollection<AccessControl.UserRole> UserRoles
    {
        get => _userRoles.AsReadOnly();
    }

    private readonly List<AccessControl.UserRole> _userRoles = new();

    /// <summary>
    /// Gets a value indicating whether this user account is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Assigns a role to this user.
    /// </summary>
    public void AssignRole(AccessControl.UserRole userRole)
    {
        if (userRole == null)
            throw new ArgumentNullException(nameof(userRole), "User role cannot be null");

        if (userRole.UserId != Id)
            throw new InvalidOperationException("User role does not belong to this user");

        if (_userRoles.Any(ur => ur.RoleId == userRole.RoleId && ur.IsActive))
            throw new InvalidOperationException("User already has this role assigned");

        _userRoles.Add(userRole);
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a role from this user.
    /// </summary>
    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId && ur.IsActive);
        if (userRole == null)
            throw new InvalidOperationException("User does not have this role assigned");

        userRole.Remove();
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public bool HasRole(Guid roleId)
    {
        return _userRoles.Any(ur => ur.RoleId == roleId && ur.IsActive);
    }

    /// <summary>
    /// Gets all active roles for this user.
    /// </summary>
    public IReadOnlyCollection<Role> GetActiveRoles()
    {
        return _userRoles
            .Where(ur => ur.IsActive)
            .Select(ur => ur.Role)
            .ToList()
            .AsReadOnly();
    }
}
