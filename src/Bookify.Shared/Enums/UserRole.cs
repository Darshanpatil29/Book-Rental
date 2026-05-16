namespace Bookify.Shared.Enums;

/// <summary>
/// Represents user roles in the system for authorization.
/// </summary>
public enum UserRole
{
    /// <summary>System administrator with full access.</summary>
    Admin = 0,

    /// <summary>Apartment owner/landlord who lists properties.</summary>
    Owner = 1,

    /// <summary>Guest/renter who searches and books apartments.</summary>
    Guest = 2
}
