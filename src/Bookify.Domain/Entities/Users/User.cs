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
    /// Gets apartments owned by this user.
    /// </summary>
    public IReadOnlyCollection<Apartment> OwnedApartments
    {
        get => _ownedApartments.AsReadOnly();
    }

    private readonly List<Apartment> _ownedApartments = new();

    /// <summary>
    /// Gets bookings made by this user.
    /// </summary>
    public IReadOnlyCollection<Booking> Bookings
    {
        get => _bookings.AsReadOnly();
    }

    private readonly List<Booking> _bookings = new();

    /// <summary>
    /// Gets reviews written by this user.
    /// </summary>
    public IReadOnlyCollection<Review> Reviews
    {
        get => _reviews.AsReadOnly();
    }

    private readonly List<Review> _reviews = new();

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

    /// <summary>
    /// Factory method to create a new user with validation.
    /// </summary>
    /// <param name="email">The user's email address (must be unique).</param>
    /// <param name="passwordHash">The user's password hash (bcrypt or similar).</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="phoneNumber">Optional phone number.</param>
    /// <returns>A Result containing the new user or an error message.</returns>
    public static Shared.Results.Result<User> CreateUser(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        // Validate email
        if (string.IsNullOrWhiteSpace(email))
            return Shared.Results.Result<User>.Failure("Email is required");

        if (email.Length > 256)
            return Shared.Results.Result<User>.Failure("Email cannot exceed 256 characters");

        // Validate password hash
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Shared.Results.Result<User>.Failure("Password hash is required");

        // Validate names
        if (string.IsNullOrWhiteSpace(firstName))
            return Shared.Results.Result<User>.Failure("First name is required");

        if (firstName.Length > 100)
            return Shared.Results.Result<User>.Failure("First name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(lastName))
            return Shared.Results.Result<User>.Failure("Last name is required");

        if (lastName.Length > 100)
            return Shared.Results.Result<User>.Failure("Last name cannot exceed 100 characters");

        // Validate phone number if provided
        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length > 20)
            return Shared.Results.Result<User>.Failure("Phone number cannot exceed 20 characters");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedOnUtc = DateTime.UtcNow,
            ModifiedOnUtc = DateTime.UtcNow
        };

        return Shared.Results.Result<User>.Success(user);
    }
}
