namespace Bookify.Shared.Enums;

/// <summary>
/// Represents the status of a booking throughout its lifecycle.
/// </summary>
public enum BookingStatus
{
    /// <summary>Guest has requested to book, awaiting owner confirmation.</summary>
    Reserved = 0,

    /// <summary>Owner has confirmed the booking.</summary>
    Confirmed = 1,

    /// <summary>Owner rejected the booking request.</summary>
    Rejected = 2,

    /// <summary>Guest has checked out, booking period completed.</summary>
    Completed = 3,

    /// <summary>Booking was cancelled by guest or system.</summary>
    Cancelled = 4
}
