using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.Bookings.DomainEvents;

/// <summary>
/// Domain event raised when a booking is confirmed (payment received).
/// </summary>
public record BookingConfirmedDomainEvent(
    Guid BookingId,
    Guid ApartmentId,
    Guid UserId,
    DateTime ConfirmedOnUtc) : DomainEvent;
