using Bookify.Shared.Constants;
using Bookify.Shared.Enums;
using Bookify.Shared.ValueObjects;

namespace Bookify.Domain.Entities.Bookings;

/// <summary>
/// Domain service responsible for calculating booking prices based on apartment details and booking duration.
/// Encapsulates pricing logic including base price, amenity surcharges, cleaning fees, and service fees.
/// </summary>
internal class PricingService
{
    /// <summary>
    /// Standard cleaning fee applied to all bookings (fixed amount per booking).
    /// </summary>
    private const decimal CleaningFeeAmount = 75m;

    /// <summary>
    /// Service fee percentage applied to base price (platform fee).
    /// </summary>
    private const decimal ServiceFeePercentage = 0.12m; // 12%

    /// <summary>
    /// Calculates the complete price breakdown for a booking.
    /// </summary>
    /// <param name="apartment">The apartment being booked.</param>
    /// <param name="bookingDates">The check-in and check-out dates.</param>
    /// <returns>A PricingDetails object containing all price components and total.</returns>
    /// <exception cref="ArgumentNullException">Thrown if apartment or bookingDates is null.</exception>
    public static PricingDetails CalculatePriceForBooking(Apartment apartment, DateRange bookingDates)
    {
        if (apartment == null)
            throw new ArgumentNullException(nameof(apartment), "Apartment cannot be null");

        if (bookingDates == null)
            throw new ArgumentNullException(nameof(bookingDates), "Booking dates cannot be null");

        // Get currency from apartment price to maintain consistency
        var currency = apartment.PricePerNight.Currency;

        // Step 1: Calculate base price (per night × number of nights)
        var numberOfNights = bookingDates.NumberOfNights;
        var basePrice = apartment.PricePerNight.Multiply(numberOfNights);

        // Step 2: Calculate amenity surcharge based on apartment amenities
        var amenitySurcharge = CalculateAmenitySurcharge(apartment.Amenities, basePrice, currency);

        // Step 3: Add fixed cleaning fee
        var cleaningFee = new Money(CleaningFeeAmount, currency);

        // Step 4: Calculate service fee (percentage of base price)
        var serviceFee = basePrice.Multiply(ServiceFeePercentage);

        // Step 5: Create and return PricingDetails (which auto-calculates total)
        return new PricingDetails(basePrice, amenitySurcharge, cleaningFee, serviceFee);
    }

    /// <summary>
    /// Calculates the amenity surcharge based on which amenities the apartment has.
    /// Premium amenities (views, pool, gym) = 5% each.
    /// Standard amenities (AC, parking) = 1% each.
    /// Free amenities (WiFi, pets) = 0%.
    /// </summary>
    /// <param name="amenities">The apartment's amenities flags.</param>
    /// <param name="basePrice">The base price to calculate surcharge from.</param>
    /// <param name="currency">The currency for the result.</param>
    /// <returns>The total amenity surcharge amount.</returns>
    private static Money CalculateAmenitySurcharge(AmenityType amenities, Money basePrice, string currency)
    {
        decimal totalSurchargePercentage = 0m;

        // Premium Amenities (5% each)
        if (amenities.HasFlag(AmenityType.GardenView))
            totalSurchargePercentage += AmenityConstants.PremiumAmenitySurchargePercentage;

        if (amenities.HasFlag(AmenityType.MountainView))
            totalSurchargePercentage += AmenityConstants.PremiumAmenitySurchargePercentage;

        if (amenities.HasFlag(AmenityType.SwimmingPool))
            totalSurchargePercentage += AmenityConstants.PremiumAmenitySurchargePercentage;

        // Standard Amenities (1% each)
        if (amenities.HasFlag(AmenityType.AirConditioning))
            totalSurchargePercentage += AmenityConstants.StandardAmenitySurchargePercentage;

        if (amenities.HasFlag(AmenityType.Parking))
            totalSurchargePercentage += AmenityConstants.StandardAmenitySurchargePercentage;

        // Free Amenities (0% surcharge)
        // - WiFi (utility)
        // - Gym (building facility)
        // - PetsAllowed (policy)

        // Calculate and return the total surcharge amount
        return basePrice.Multiply(totalSurchargePercentage);
    }
}