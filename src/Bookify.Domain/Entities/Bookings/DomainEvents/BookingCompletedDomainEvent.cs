using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.Bookings.DomainEvents;

/// <summary>
/// Domain event raised when a booking is completed (guest checked out).
/// </summary>
public record BookingCompletedDomainEvent(
    Guid BookingId,
    Guid ApartmentId,
    Guid UserId,
    DateTime CompletedOnUtc) : DomainEvent;
