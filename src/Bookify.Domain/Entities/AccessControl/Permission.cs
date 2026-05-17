using Bookify.Domain.Common;
using Bookify.Shared.Results;

namespace Bookify.Domain.Entities.AccessControl;

/// <summary>
/// Represents a permission in the system.
/// Permissions define what actions users can perform in the system.
/// Examples: "Bookings.Create", "Apartments.Update", "Reviews.Delete", "Users.ManageRoles"
/// </summary>
public class Permission : DataEntity
{
    /// <summary>
    /// Gets the unique key for this permission (e.g., "Bookings.Create", "Apartments.Update").
    /// Format: "Feature.Action" for consistency and easy querying.
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name of this permission.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of what this permission allows.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the feature/module this permission belongs to (e.g., "Bookings", "Apartments", "Users").
    /// </summary>
    public string Feature { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the action this permission controls (e.g., "Create", "Update", "Delete", "View").
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this permission is active and can be assigned to roles.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the sort order for displaying permissions in UI.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission"/> class.
    /// </summary>
    private Permission() { }

    /// <summary>
    /// Creates a new permission with the specified details.
    /// </summary>
    /// <param name="feature">The feature/module name (e.g., "Bookings", "Apartments").</param>
    /// <param name="action">The action name (e.g., "Create", "Update", "Delete").</param>
    /// <param name="name">The display name of the permission.</param>
    /// <param name="description">The description of what this permission allows.</param>
    /// <param name="sortOrder">Optional sort order for UI display.</param>
    public static Result<Permission> Create(
        string feature,
        string action,
        string name,
        string description,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(feature))
            return Result.Failure<Permission>("Feature cannot be empty");

        if (string.IsNullOrWhiteSpace(action))
            return Result.Failure<Permission>("Action cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Permission>("Name cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Permission>("Description cannot be empty");

        if (name.Length > 100)
            return Result.Failure<Permission>("Name cannot exceed 100 characters");

        if (description.Length > 500)
            return Result.Failure<Permission>("Description cannot exceed 500 characters");

        var key = $"{feature.Trim()}.{action.Trim()}";

        return Result.Success(new Permission
        {
            Key = key,
            Feature = feature.Trim(),
            Action = action.Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            SortOrder = sortOrder
        });
    }

    /// <summary>
    /// Updates the permission's information.
    /// </summary>
    public Result UpdateInfo(string name, string description, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Name cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description cannot be empty");

        Name = name.Trim();
        Description = description.Trim();
        SortOrder = sortOrder;
        ModifiedOnUtc = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Activates this permission.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates this permission.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedOnUtc = DateTime.UtcNow;
    }
}
