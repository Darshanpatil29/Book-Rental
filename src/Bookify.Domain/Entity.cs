// Domain layer - contains pure business logic with no dependencies
namespace Bookify.Domain;

/// <summary>
/// Base class for domain entities.
/// Entities have identity and lifecycle.
/// </summary>
public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = id;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }
    public DateTime CreatedOnUtc { get; protected set; }
}
