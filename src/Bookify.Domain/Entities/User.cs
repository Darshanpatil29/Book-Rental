using Bookify.Shared.Enums;
using Bookify.Shared.ValueObjects;
using Bookify.Domain.Common;
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
    /// Gets the user's role in the system.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this user account is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
}
