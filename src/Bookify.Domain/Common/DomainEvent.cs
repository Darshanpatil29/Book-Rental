namespace Bookify.Domain.Common;

/// <summary>
/// Base class for domain events raised by entities.
/// </summary>
public interface IDomainEvent
{

    public Guid Id { get; } 
    /// <summary>
    /// Gets the UTC timestamp when this event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}