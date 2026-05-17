namespace Bookify.Shared.ValueObjects;

/// <summary>
/// Represents the breakdown of pricing for a booking.
/// </summary>
public sealed class PricingDetails : IEquatable<PricingDetails>
{
    /// <summary>
    /// Gets the base price (price per night × number of nights).
    /// </summary>
    public Money BasePrice { get; }

    /// <summary>
    /// Gets the amenity surcharge based on apartment amenities.
    /// </summary>
    public Money AmenitySurcharge { get; }

    /// <summary>
    /// Gets the cleaning fee (one-time charge per booking).
    /// </summary>
    public Money CleaningFee { get; }

    /// <summary>
    /// Gets the service fee (platform fee, typically 10-15% of base).
    /// </summary>
    public Money ServiceFee { get; }

    /// <summary>
    /// Gets the total price (sum of all components).
    /// </summary>
    public Money TotalPrice { get; }

    /// <summary>
    /// Initializes a new instance of the PricingDetails class.
    /// </summary>
    /// <param name="basePrice">Base price for the stay.</param>
    /// <param name="amenitySurcharge">Amenity surcharge amount.</param>
    /// <param name="cleaningFee">One-time cleaning fee.</param>
    /// <param name="serviceFee">Platform service fee.</param>
    /// <exception cref="ArgumentException">Thrown when prices are negative or currencies don't match.</exception>
    public PricingDetails(
        Money basePrice,
        Money amenitySurcharge,
        Money cleaningFee,
        Money serviceFee)
    {
        ValidatePricing(basePrice, amenitySurcharge, cleaningFee, serviceFee);

        BasePrice = basePrice;
        AmenitySurcharge = amenitySurcharge;
        CleaningFee = cleaningFee;
        ServiceFee = serviceFee;
        TotalPrice = basePrice.Add(amenitySurcharge).Add(cleaningFee).Add(serviceFee);
    }

    /// <summary>
    /// Gets the subtotal (base + amenity + cleaning, before service fee).
    /// </summary>
    public Money Subtotal => BasePrice.Add(AmenitySurcharge).Add(CleaningFee);

    /// <summary>
    /// Gets the percentage breakdown of service fee relative to subtotal.
    /// </summary>
    public decimal ServiceFeePercentage
    {
        get
        {
            if (Subtotal.Amount == 0) return 0;
            return (ServiceFee.Amount / Subtotal.Amount) * 100;
        }
    }

    /// <summary>
    /// Gets the percentage breakdown of amenity surcharge relative to base price.
    /// </summary>
    public decimal AmenitySurchargePercentage
    {
        get
        {
            if (BasePrice.Amount == 0) return 0;
            return (AmenitySurcharge.Amount / BasePrice.Amount) * 100;
        }
    }

    /// <summary>
    /// Returns the string representation of this pricing details.
    /// </summary>
    public override string ToString()
    {
        return $"Base: {BasePrice} | Amenities: {AmenitySurcharge} | Cleaning: {CleaningFee} | Service: {ServiceFee} | Total: {TotalPrice}";
    }

    /// <summary>
    /// Determines whether this pricing details is equal to another.
    /// </summary>
    public bool Equals(PricingDetails? other)
    {
        if (other is null) return false;
        return BasePrice == other.BasePrice
            && AmenitySurcharge == other.AmenitySurcharge
            && CleaningFee == other.CleaningFee
            && ServiceFee == other.ServiceFee;
    }

    /// <summary>
    /// Determines whether the specified object is equal to this pricing details.
    /// </summary>
    public override bool Equals(object? obj) => obj is PricingDetails pricing && Equals(pricing);

    /// <summary>
    /// Returns the hash code for this pricing details.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(BasePrice, AmenitySurcharge, CleaningFee, ServiceFee);
    }

    /// <summary>
    /// Determines whether two pricing details are equal.
    /// </summary>
    public static bool operator ==(PricingDetails? left, PricingDetails? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two pricing details are not equal.
    /// </summary>
    public static bool operator !=(PricingDetails? left, PricingDetails? right) => !(left == right);

    /// <summary>
    /// Validates pricing components.
    /// </summary>
    private static void ValidatePricing(
        Money basePrice,
        Money amenitySurcharge,
        Money cleaningFee,
        Money serviceFee)
    {
        if (basePrice == null)
            throw new ArgumentNullException(nameof(basePrice));

        if (amenitySurcharge == null)
            throw new ArgumentNullException(nameof(amenitySurcharge));

        if (cleaningFee == null)
            throw new ArgumentNullException(nameof(cleaningFee));

        if (serviceFee == null)
            throw new ArgumentNullException(nameof(serviceFee));

        // All prices must use the same currency
        var currency = basePrice.Currency;
        if (amenitySurcharge.Currency != currency)
            throw new ArgumentException($"Amenity surcharge currency ({amenitySurcharge.Currency}) must match base price currency ({currency}).", nameof(amenitySurcharge));

        if (cleaningFee.Currency != currency)
            throw new ArgumentException($"Cleaning fee currency ({cleaningFee.Currency}) must match base price currency ({currency}).", nameof(cleaningFee));

        if (serviceFee.Currency != currency)
            throw new ArgumentException($"Service fee currency ({serviceFee.Currency}) must match base price currency ({currency}).", nameof(serviceFee));
    }
}
