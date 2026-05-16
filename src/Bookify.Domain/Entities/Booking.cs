using Bookify.Domain.Common;
using Bookify.Shared.Enums;
using Bookify.Shared.Exceptions;
using Bookify.Shared.ValueObjects;

namespace Bookify.Domain.Entities;

/// <summary>
/// Represents a booking (reservation) of an apartment.
/// </summary>
public class Booking : DataEntity
{
    /// <summary>
    /// Gets the ID of the booked apartment.
    /// </summary>
    public Guid ApartmentId { get; private set; }

    /// <summary>
    /// Gets the ID of the guest making the booking.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the check-in and check-out dates.
    /// </summary>
    public DateRange DateRange { get; private set; } = null!;

    /// <summary>
    /// Gets the current status of the booking.
    /// </summary>
    public BookingStatus Status { get; private set; }

    /// <summary>
    /// Gets the total price for the booking.
    /// </summary>
    public Money TotalPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the base price (before surcharges).
    /// </summary>
    public Money BasePrice { get; private set; } = null!;

    /// <summary>
    /// Gets the amenity surcharge amount.
    /// </summary>
    public Money AmenitySurcharge { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the booking has been confirmed by payment.
    /// </summary>
    public bool IsPaid { get; private set; }

    /// <summary>
    /// Gets the date when payment was received. Null if not paid.
    /// </summary>
    public DateTime? PaidOnUtc { get; private set; }
}
