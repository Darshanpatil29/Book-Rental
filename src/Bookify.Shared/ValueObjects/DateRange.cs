namespace Bookify.Shared.ValueObjects;

/// <summary>
/// Represents a date range with start and end dates.
/// This is a value object - immutable and identified by its value.
/// </summary>
public sealed class DateRange : IEquatable<DateRange>
{
    /// <summary>
    /// Initializes a new instance of the DateRange value object.
    /// </summary>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <exception cref="ArgumentException">Thrown if endDate is before startDate.</exception>
    public DateRange(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Gets the start date (inclusive).
    /// </summary>
    public DateOnly StartDate { get; }

    /// <summary>
    /// Gets the end date (inclusive).
    /// </summary>
    public DateOnly EndDate { get; }

    /// <summary>
    /// Gets the number of nights in this date range.
    /// </summary>
    public int NumberOfNights => (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days;

    /// <summary>
    /// Gets the number of days (nights + 1 for the last day).
    /// </summary>
    public int NumberOfDays => NumberOfNights + 1;

    /// <summary>
    /// Determines if this date range overlaps with another date range.
    /// </summary>
    public bool OverlapsWith(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    /// <summary>
    /// Determines if a specific date falls within this date range (inclusive).
    /// </summary>
    public bool Contains(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }

    /// <summary>
    /// Determines if another date range is completely contained within this range.
    /// </summary>
    public bool Contains(DateRange other)
    {
        return StartDate <= other.StartDate && EndDate >= other.EndDate;
    }

    /// <summary>
    /// Gets the number of days between two date ranges (gap between them).
    /// Returns 0 if they overlap or are adjacent.
    /// </summary>
    public int DaysBetween(DateRange other)
    {
        if (OverlapsWith(other))
            return 0;

        if (EndDate < other.StartDate)
            return (other.StartDate.ToDateTime(TimeOnly.MinValue) - EndDate.ToDateTime(TimeOnly.MinValue)).Days - 1;

        return (StartDate.ToDateTime(TimeOnly.MinValue) - other.EndDate.ToDateTime(TimeOnly.MinValue)).Days - 1;
    }

    public override bool Equals(object? obj) => obj is DateRange range && Equals(range);

    public bool Equals(DateRange? other)
    {
        if (other is null) return false;
        return StartDate == other.StartDate && EndDate == other.EndDate;
    }

    public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";

    public static bool operator ==(DateRange? left, DateRange? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(DateRange? left, DateRange? right)
    {
        return !(left == right);
    }
}
