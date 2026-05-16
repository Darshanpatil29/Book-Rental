namespace Bookify.Shared.Enums;

/// <summary>
/// Represents apartment amenities using bitmask flags for efficient storage and querying.
/// </summary>
[Flags]
public enum AmenityType
{
    /// <summary>No amenities selected.</summary>
    None = 0,

    /// <summary>WiFi internet access.</summary>
    WiFi = 1 << 0,  // 1

    /// <summary>Parking space available.</summary>
    Parking = 1 << 1,  // 2

    /// <summary>Air conditioning system.</summary>
    AirConditioning = 1 << 2,  // 4

    /// <summary>Garden or outdoor view.</summary>
    GardenView = 1 << 3,  // 8

    /// <summary>Mountain or scenic view.</summary>
    MountainView = 1 << 4,  // 16

    /// <summary>Swimming pool access.</summary>
    SwimmingPool = 1 << 5,  // 32

    /// <summary>Gym or fitness facilities.</summary>
    Gym = 1 << 6,  // 64

    /// <summary>Pets allowed.</summary>
    PetsAllowed = 1 << 7   // 128
}
