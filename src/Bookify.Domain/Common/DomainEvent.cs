namespace Bookify.Domain.Common;

/// <summary>
/// Base class for domain events raised by entities.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp when this event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
