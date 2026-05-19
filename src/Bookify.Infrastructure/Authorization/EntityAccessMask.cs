namespace Bookify.Infrastructure.Authorization;

/// <summary>
/// Represents the access permissions for a specific entity by a user.
/// Used in service layer to determine what operations are allowed on an entity.
/// This is Layer 2 of the two-layer authorization pattern:
/// Layer 1 (Controller): [Authorize(Policy = "Admin")] - checks JWT claims
/// Layer 2 (Service): EntityAccessMask - checks entity-specific permissions
/// </summary>
public sealed record EntityAccessMask
{
    /// <summary>
    /// Gets a value indicating whether the user can read/view this entity.
    /// </summary>
    public bool CanRead { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user can create entities of this type.
    /// </summary>
    public bool CanCreate { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user can modify this specific entity.
    /// </summary>
    public bool CanUpdate { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user can delete this specific entity.
    /// </summary>
    public bool CanDelete { get; init; }

    /// <summary>
    /// Creates an access mask denying all operations.
    /// </summary>
    public static EntityAccessMask None() => new()
    {
        CanRead = false,
        CanCreate = false,
        CanUpdate = false,
        CanDelete = false
    };

    /// <summary>
    /// Creates an access mask allowing all operations.
    /// </summary>
    public static EntityAccessMask Full() => new()
    {
        CanRead = true,
        CanCreate = true,
        CanUpdate = true,
        CanDelete = true
    };

    /// <summary>
    /// Creates an access mask allowing only read operations.
    /// </summary>
    public static EntityAccessMask ReadOnly() => new()
    {
        CanRead = true,
        CanCreate = false,
        CanUpdate = false,
        CanDelete = false
    };
}
