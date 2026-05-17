using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.Bookings.DomainEvents;

/// <summary>
/// Domain event raised when a booking is reserved (initial status after creation).
/// </summary>
public record BookingReservedDomainEvent(Guid BookingId) : DomainEvent;
