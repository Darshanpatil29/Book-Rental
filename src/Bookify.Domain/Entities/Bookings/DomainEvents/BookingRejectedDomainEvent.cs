using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.Bookings.DomainEvents;

/// <summary>
/// Domain event raised when a booking is rejected.
/// </summary>
public record BookingRejectedDomainEvent(
    Guid BookingId,
    Guid ApartmentId,
    Guid UserId) : DomainEvent;
