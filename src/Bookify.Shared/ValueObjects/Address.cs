namespace Bookify.Shared.ValueObjects;

/// <summary>
/// Represents a physical address.
/// This is a value object - immutable and identified by its value.
/// </summary>
public sealed class Address : IEquatable<Address>
{
    /// <summary>
    /// Initializes a new instance of the Address value object.
    /// </summary>
    /// <param name="country">The country name.</param>
    /// <param name="state">The state or province.</param>
    /// <param name="city">The city name.</param>
    /// <param name="zipCode">The postal/zip code.</param>
    /// <param name="street">The street address.</param>
    public Address(string country, string state, string city, string zipCode, string street)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.", nameof(state));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code cannot be empty.", nameof(zipCode));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));

        Country = country;
        State = state;
        City = city;
        ZipCode = zipCode;
        Street = street;
    }

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Gets the state or province.
    /// </summary>
    public string State { get; }

    /// <summary>
    /// Gets the city.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the postal/zip code.
    /// </summary>
    public string ZipCode { get; }

    /// <summary>
    /// Gets the street address.
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// Gets the full formatted address.
    /// </summary>
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";

    public override bool Equals(object? obj) => obj is Address address && Equals(address);

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Country == other.Country &&
               State == other.State &&
               City == other.City &&
               ZipCode == other.ZipCode &&
               Street == other.Street;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Country, State, City, ZipCode, Street);
    }

    public override string ToString() => FullAddress;

    public static bool operator ==(Address? left, Address? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !(left == right);
    }
}
