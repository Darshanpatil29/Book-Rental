using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.Bookings.DomainEvents;

/// <summary>
/// Domain event raised when a booking is cancelled.
/// </summary>
public record BookingCancelledDomainEvent(
    Guid BookingId,
    Guid ApartmentId,
    Guid UserId,
    DateTime CancelledOnUtc) : DomainEvent;
