namespace Bookify.Shared.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// This is a value object - immutable and identified by its value, not identity.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    /// <summary>
    /// Initializes a new instance of the Money value object.
    /// </summary>
    /// <param name="amount">The numeric amount (must be non-negative).</param>
    /// <param name="currency">The currency code.</param>
    /// <exception cref="ArgumentException">Thrown if amount is negative.</exception>
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Gets the numeric amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code (e.g., "USD", "EUR").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Gets a zero Money value with the specified currency.
    /// </summary>
    public static Money Zero(string currency) => new(0, currency);

    /// <summary>
    /// Adds two Money values (must have same currency).
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts one Money value from another (must have same currency).
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the Money value by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative.", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Divides the Money value by a divisor.
    /// </summary>
    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new ArgumentException("Divisor cannot be zero.", nameof(divisor));

        return new Money(Amount / divisor, Currency);
    }

    /// <summary>
    /// Determines if this Money is greater than another Money value.
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies.");

        return Amount > other.Amount;
    }

    /// <summary>
    /// Determines if this Money is less than another Money value.
    /// </summary>
    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies.");

        return Amount < other.Amount;
    }

    /// <summary>
    /// Determines if this Money is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    public override bool Equals(object? obj) => obj is Money money && Equals(money);

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money? left, Money? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }

    public static bool operator <(Money left, Money right)
    {
        return left.IsLessThan(right);
    }

    public static bool operator >(Money left, Money right)
    {
        return left.IsGreaterThan(right);
    }

    public static bool operator <=(Money left, Money right)
    {
        return left.IsLessThan(right) || left.Equals(right);
    }

    public static bool operator >=(Money left, Money right)
    {
        return left.IsGreaterThan(right) || left.Equals(right);
    }
}
