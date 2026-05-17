namespace Bookify.Shared.ValueObjects;

/// <summary>
/// Represents a price range with minimum and maximum values.
/// Useful for filtering apartments by price range.
/// </summary>
public sealed class PriceRange : IEquatable<PriceRange>
{
    /// <summary>
    /// Gets the minimum price in the range.
    /// </summary>
    public Money MinPrice { get; }

    /// <summary>
    /// Gets the maximum price in the range.
    /// </summary>
    public Money MaxPrice { get; }

    /// <summary>
    /// Initializes a new instance of the PriceRange class.
    /// </summary>
    /// <param name="minPrice">The minimum price.</param>
    /// <param name="maxPrice">The maximum price.</param>
    /// <exception cref="ArgumentException">Thrown when maxPrice is less than minPrice or currencies don't match.</exception>
    public PriceRange(Money minPrice, Money maxPrice)
    {
        if (minPrice == null)
            throw new ArgumentNullException(nameof(minPrice));

        if (maxPrice == null)
            throw new ArgumentNullException(nameof(maxPrice));

        if (minPrice.Currency != maxPrice.Currency)
            throw new ArgumentException($"Currency mismatch: {minPrice.Currency} vs {maxPrice.Currency}.", nameof(maxPrice));

        if (maxPrice.IsLessThan(minPrice))
            throw new ArgumentException("Max price must be greater than or equal to min price.", nameof(maxPrice));

        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }

    /// <summary>
    /// Determines whether a price falls within this range (inclusive).
    /// </summary>
    public bool Contains(Money price)
    {
        if (price == null)
            throw new ArgumentNullException(nameof(price));

        if (price.Currency != MinPrice.Currency)
            throw new ArgumentException($"Price currency ({price.Currency}) must match range currency ({MinPrice.Currency}).", nameof(price));
        return MinPrice >= price && MaxPrice >= price;
    }

    /// <summary>
    /// Returns the string representation of this price range.
    /// </summary>
    public override string ToString() => $"{MinPrice} - {MaxPrice}";

    /// <summary>
    /// Determines whether this price range is equal to another.
    /// </summary>
    public bool Equals(PriceRange? other)
    {
        if (other is null) return false;
        return MinPrice == other.MinPrice && MaxPrice == other.MaxPrice;
    }

    /// <summary>
    /// Determines whether the specified object is equal to this price range.
    /// </summary>
    public override bool Equals(object? obj) => obj is PriceRange range && Equals(range);

    /// <summary>
    /// Returns the hash code for this price range.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(MinPrice, MaxPrice);

    /// <summary>
    /// Determines whether two price ranges are equal.
    /// </summary>
    public static bool operator ==(PriceRange? left, PriceRange? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two price ranges are not equal.
    /// </summary>
    public static bool operator !=(PriceRange? left, PriceRange? right) => !(left == right);
}
