namespace Bookify.Shared.Constants;

/// <summary>
/// Contains constants related to apartment amenities.
/// </summary>
public static class AmenityConstants
{
    /// <summary>
    /// Premium amenity surcharge percentage (applied to amenities like scenic views).
    /// </summary>
    public const decimal PremiumAmenitySurchargePercentage = 0.05m; // 5%

    /// <summary>
    /// Standard amenity surcharge percentage (applied to convenience amenities like AC, Parking).
    /// </summary>
    public const decimal StandardAmenitySurchargePercentage = 0.01m; // 1%

    /// <summary>
    /// Premium amenities that qualify for premium surcharge.
    /// </summary>
    public static class PremiumAmenities
    {
        public const string GardenView = nameof(GardenView);
        public const string MountainView = nameof(MountainView);
        public const string SwimmingPool = nameof(SwimmingPool);
    }

    /// <summary>
    /// Standard amenities that qualify for standard surcharge.
    /// </summary>
    public static class StandardAmenities
    {
        public const string AirConditioning = nameof(AirConditioning);
        public const string Parking = nameof(Parking);
    }

    /// <summary>
    /// Free amenities with no surcharge.
    /// </summary>
    public static class FreeAmenities
    {
        public const string WiFi = nameof(WiFi);
        public const string PetsAllowed = nameof(PetsAllowed);
        public const string Gym = nameof(Gym);
    }
}
