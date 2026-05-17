using Bookify.Shared.Enums;

namespace Bookify.Shared.Results;

/// <summary>
/// Represents the result of a booking operation (reserve, confirm, reject, complete, cancel).
/// </summary>
public record BookingOperationResult(
    bool IsSuccess,
    string Message,
    BookingStatus? NewStatus = null,
    DateTime? OperationTimestampUtc = null)
{
    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    public static BookingOperationResult Success(
        BookingStatus newStatus,
        string message = "Operation completed successfully")
        => new(true, message, newStatus, DateTime.UtcNow);

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    public static BookingOperationResult Failure(string message)
        => new(false, message);

    /// <summary>
    /// Creates a validation failure result for invalid state transitions.
    /// </summary>
    public static BookingOperationResult InvalidStateTransition(
        BookingStatus currentStatus,
        string attemptedAction)
        => new(false, $"Cannot {attemptedAction} booking with status '{currentStatus}'");
}
