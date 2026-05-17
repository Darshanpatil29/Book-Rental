namespace Bookify.Shared.ValueObjects;

/// <summary>
/// Value object representing a user's permission claim.
/// Used for caching and passing permission information throughout the application.
/// </summary>
public record PermissionClaim(
    string PermissionKey,
    string Feature,
    string Action,
    string Name,
    bool IsActive)
{
    /// <summary>
    /// Checks if this claim matches the specified permission key.
    /// </summary>
    public bool Matches(string permissionKey)
    {
        return PermissionKey == permissionKey && IsActive;
    }

    /// <summary>
    /// Checks if this claim matches the specified feature and action.
    /// </summary>
    public bool Matches(string feature, string action)
    {
        return Feature == feature && Action == action && IsActive;
    }
}
