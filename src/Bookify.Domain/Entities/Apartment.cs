using Bookify.Domain.Common;
using Bookify.Shared.Enums;
using Bookify.Shared.ValueObjects;

namespace Bookify.Domain.Entities;

/// <summary>
/// Represents a rental apartment that can be booked by guests.
/// </summary>
public class Apartment : DataEntity
{
    /// <summary>
    /// Gets the ID of the owner (User).
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Gets the apartment's title/name.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the apartment's detailed description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the apartment's address (immutable value object).
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the price per night.
    /// </summary>
    public Money PricePerNight { get; private set; } = null!;

    /// <summary>
    /// Gets the apartment's amenities as bit flags.
    /// </summary>
    public AmenityType Amenities { get; private set; }

    /// <summary>
    /// Gets the maximum number of guests allowed.
    /// </summary>
    public int MaxGuests { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this apartment is available for booking.
    /// </summary>
    public bool IsActive { get; private set; } = true;

}
