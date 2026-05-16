using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Domain.Common;

/// <summary>
/// Base class for all domain entities with system-populated audit properties and domain event support.
/// </summary>
public class DataEntity
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the UTC timestamp when this entity was created.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedOnUtc { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the user ID who created this entity. Null for system-created entities.
    /// </summary>
    public Guid? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets the UTC timestamp when this entity was last modified. Null if never modified.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? ModifiedOnUtc { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the user ID who last modified this entity. Null if never modified.
    /// </summary>
    public Guid? ModifiedBy { get; protected set; }
}
