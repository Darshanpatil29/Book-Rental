using Bookify.Domain.Common;
using Bookify.Domain.Entities.Bookings;
using Bookify.Domain.Entities.Bookings.DomainEvents;
using Bookify.Shared.Enums;
using Bookify.Shared.Exceptions;
using Bookify.Shared.Results;
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
    /// Gets the apartment being booked (navigation property).
    /// </summary>
    public Apartment Apartment { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the guest making the booking.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the guest making the booking (navigation property).
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Gets the check-in and check-out dates.
    /// </summary>
    public DateRange Duration { get; private set; } = null!;

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
    /// Gets the cleaning fee (one-time charge per booking).
    /// </summary>
    public Money? CleaningFee { get; private set; }

    /// <summary>
    /// Gets the service fee (platform fee, typically 10-15% of base).
    /// </summary>
    public Money? ServiceFee { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the booking has been confirmed by payment.
    /// </summary>
    public bool IsPaid { get; private set; }

    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Gets the date when payment was received. Null if not paid.
    /// </summary>
    public DateTime? PaidOnUtc { get; private set; }

    public DateTime? ConfirmedOnUtc { get; private set; }

    public DateTime? CancelledOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }
    public DateTime? RejectedOnUtc { get; private set; }

    public Money? RefundAmount { get; private set; }

    /// <summary>   
    /// Reserves the booking (initial status after creation).
    /// </summary>
    /// <returns>Operation result indicating success or failure.</returns>
    public BookingOperationResult Reserve(Apartment apartment, DateRange bookingDates, Guid userId)
    {
        var pricingDetails = PricingService.CalculatePriceForBooking(apartment, bookingDates);
        if (pricingDetails == null)
        {
            return BookingOperationResult.Failure("Failed to calculate pricing for the booking.");
        }
        var booking = new Booking
        {
            ApartmentId = apartment.Id,
            UserId = userId,
            Duration = bookingDates,
            BasePrice = pricingDetails.BasePrice,
            AmenitySurcharge = pricingDetails.AmenitySurcharge,
            CleaningFee = pricingDetails.CleaningFee,
            ServiceFee = pricingDetails.ServiceFee,
            TotalPrice = pricingDetails.TotalPrice,
            Status = BookingStatus.Reserved
        };
        RaiseDomainEvent(new BookingReservedDomainEvent(booking.Id));
        return BookingOperationResult.Success(BookingStatus.Reserved, "Booking reserved successfully");
    }

    /// <summary>
    /// Confirms the booking and marks payment as received.
    /// </summary>
    /// <returns>Operation result indicating success or failure.</returns>
    public BookingOperationResult Confirm(DateTime dateTimeUtc)
    {
        // Can only confirm from Reserved status
        if (Status != BookingStatus.Reserved)
        {
            return BookingOperationResult.InvalidStateTransition(Status, "confirm");
        }

        Status = BookingStatus.Confirmed;
        IsPaid = true;
        PaidOnUtc = dateTimeUtc;
        ConfirmedOnUtc = DateTime.UtcNow;

        // Raise domain event
        RaiseDomainEvent(new BookingConfirmedDomainEvent(Id, ApartmentId, UserId, ConfirmedOnUtc.Value));

        return BookingOperationResult.Success(BookingStatus.Confirmed, "Booking confirmed successfully");
    }

    /// <summary>
    /// Rejects the booking.
    /// </summary>
    /// <returns>Operation result indicating success or failure.</returns>
    public BookingOperationResult Reject()
    {
        // Can only reject from Reserved or Confirmed status (not yet completed)
        if (Status is not (BookingStatus.Reserved or BookingStatus.Confirmed))
        {
            return BookingOperationResult.InvalidStateTransition(Status, "reject");
        }

        Status = BookingStatus.Rejected;
        IsPaid = false;
        PaidOnUtc = null;
        RejectedOnUtc = DateTime.UtcNow;

        // Raise domain event
        RaiseDomainEvent(new BookingRejectedDomainEvent(Id, ApartmentId, UserId));

        return BookingOperationResult.Success(BookingStatus.Rejected, "Booking rejected successfully");
    }

    /// <summary>
    /// Completes the booking (guest has checked out).
    /// </summary>
    /// <returns>Operation result indicating success or failure.</returns>
    public BookingOperationResult Complete(DateTime? dateTimeUtc)
    {
        // Can only complete from Confirmed status
        if (Status != BookingStatus.Confirmed)
        {
            return BookingOperationResult.InvalidStateTransition(Status, "complete");
        }

        Status = BookingStatus.Completed;
        CompletedOnUtc = dateTimeUtc ?? DateTime.UtcNow;

        // Raise domain event
        RaiseDomainEvent(new BookingCompletedDomainEvent(Id, ApartmentId, UserId, CompletedOnUtc.Value));

        return BookingOperationResult.Success(BookingStatus.Completed, "Booking completed successfully");
    }

    /// <summary>
    /// Cancels the booking.
    /// </summary>
    /// <returns>Operation result indicating success or failure.</returns>
    public BookingOperationResult Cancel(Money? refundAmount)
    {
        // Can only cancel from Reserved or Confirmed status (not completed or already cancelled)
        if (Status is not (BookingStatus.Reserved or BookingStatus.Confirmed))
        {
            return BookingOperationResult.InvalidStateTransition(Status, "cancel");
        }
        IsCancelled = true;
        RefundAmount = refundAmount;
        Status = BookingStatus.Cancelled;
        CancelledOnUtc = DateTime.UtcNow;

        // If already paid, refund logic would be handled at application layer
        if (IsPaid && Status == BookingStatus.Confirmed)
        {
            // Payment reversal would be initiated, but not executed here
            // This is handled in the application/infrastructure layer
        }

        // Raise domain event
        RaiseDomainEvent(new BookingCancelledDomainEvent(Id, ApartmentId, UserId, CancelledOnUtc.Value));

        return BookingOperationResult.Success(BookingStatus.Cancelled, "Booking cancelled successfully");
    }
}
