using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;
/// <summary>
/// Represents a review of an apartment by a guest after their stay.
/// </summary>
public class Review : DataEntity
{
    /// <summary>
    /// Minimum rating value.
    /// </summary>
    public const int MinRating = 1;

    /// <summary>
    /// Maximum rating value.
    /// </summary>
    public const int MaxRating = 5;

    /// <summary>
    /// Gets the ID of the booking this review is for.
    /// </summary>
    public Guid BookingId { get; private set; }

    /// <summary>
    /// Gets the ID of the guest who wrote the review.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the ID of the apartment being reviewed.
    /// </summary>
    public Guid ApartmentId { get; private set; }

    /// <summary>
    /// Gets the rating given by the guest (1-5).
    /// </summary>
    public int Rating { get; private set; }

    /// <summary>
    /// Gets the review comment text.
    /// </summary>
    public string Comment { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this review has been moderated/approved.
    /// </summary>
    public bool IsApproved { get; private set; }
}
