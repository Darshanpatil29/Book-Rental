namespace Bookify.Shared.Exceptions;

/// <summary>
/// Exception thrown when validation of input data fails.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with validation errors dictionary.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">Dictionary of property names and their validation errors.</param>
    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets the validation errors by property name.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; } = new();
}
