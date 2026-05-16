namespace Bookify.Shared.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found in the database.
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class.
    /// </summary>
    /// <param name="entityType">The type of entity that was not found.</param>
    /// <param name="id">The ID of the entity that was not found.</param>
    public NotFoundException(string entityType, object id)
        : base($"{entityType} with ID '{id}' was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class with custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
