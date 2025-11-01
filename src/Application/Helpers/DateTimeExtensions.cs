namespace EduCare.Application.Helpers;

public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to UTC, handling Unspecified kind by specifying it as UTC
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>UTC DateTime</returns>
    public static DateTime ToUtcKind(this DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            : dateTime.ToUniversalTime();
    }

    /// <summary>
    /// Converts DateTime to UTC, with optional null handling
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>UTC DateTime or null if input was null</returns>
    public static DateTime? ToUtcKind(this DateTime? dateTime)
    {
        return dateTime?.ToUtcKind();
    }

    /// <summary>
    /// Safely converts DateTime to UTC, with a fallback value if null
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <param name="fallback">Fallback value if input is null</param>
    /// <returns>UTC DateTime</returns>
    public static DateTime ToUtcKind(this DateTime? dateTime, DateTime fallback)
    {
        return dateTime?.ToUtcKind() ?? fallback.ToUtcKind();
    }

    /// <summary>
    /// Converts DateTime to UTC and returns the start of the day (00:00:00)
    /// </summary>
    public static DateTime ToUtcStartOfDay(this DateTime dateTime)
    {
        return dateTime.ToUtcKind().Date;
    }

    /// <summary>
    /// Converts DateTime to UTC and returns the end of the day (23:59:59.999)
    /// </summary>
    public static DateTime ToUtcEndOfDay(this DateTime dateTime)
    {
        var utcDate = dateTime.ToUtcKind();
        return utcDate.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Checks if the DateTime is in UTC format
    /// </summary>
    public static bool IsUtc(this DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc;
    }

    /// <summary>
    /// Ensures the DateTime is UTC, throws exception if it's not
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when DateTime is not UTC</exception>
    public static DateTime EnsureUtc(this DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"DateTime must be UTC. Current kind: {dateTime.Kind}");
        }
        return dateTime;
    }
}