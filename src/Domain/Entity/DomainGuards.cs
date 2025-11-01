using System.Runtime.CompilerServices;
using EduCare.Domain.Exceptions;

namespace EduCare.Domain.Entity;

public static class DomainGuards
{
    public static void AgainstNullOrWhiteSpace(string? value,
        [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{paramName} cannot be null or whitespace.");
    }

    public static void AgainstDefault<T>(T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        if (value.Equals(default(T)))
            throw new DomainException($"{paramName} must be specified.");
    }

    public static void AgainstNull<T>(T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : class
    {
        if (value is null)
            throw new DomainException($"{paramName} cannot be null.");
    }

    // Overload for value objects that are reference types (like Money)
    public static void AgainstNull(object? value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value is null)
            throw new DomainException($"{paramName} cannot be null.");
    }

    public static void AgainstCondition(bool condition, string message,
        [CallerArgumentExpression("condition")] string? conditionExpression = null)
    {
        if (condition)
            throw new DomainException($"{message} (Condition: {conditionExpression})");
    }

    // Additional guard methods that might be useful for your domain
    public static void AgainstNegative(decimal value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value < 0)
            throw new DomainException($"{paramName} cannot be negative.");
    }

    public static void AgainstNegativeOrZero(decimal value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be greater than zero.");
    }

    public static void AgainstOutOfRange(decimal value, decimal min, decimal max,
        [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value < min || value > max)
            throw new DomainException($"{paramName} must be between {min} and {max}.");
    }

    /// <summary>
    /// Validates that a string does not exceed the specified maximum length
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="paramName">The parameter name for the exception message</param>
    /// <exception cref="DomainException">Thrown when value exceeds maximum length</exception>
    public static void AgainstStringExceedingLength(string value, int maxLength, string? paramName = null)
    {
        if (value?.Length > maxLength)
            throw new DomainException($"{paramName ?? "Value"} cannot exceed {maxLength} characters");
    }

    /// <summary>
    /// Validates that a date is not in the past
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <param name="paramName">The parameter name for the exception message</param>
    /// <exception cref="DomainException">Thrown when date is in the past</exception>
    public static void AgainstPastDate(DateTime date, string? paramName = null)
    {
        if (date < DateTime.UtcNow.Date)
            throw new DomainException($"{paramName ?? "Date"} cannot be in the past");
    }

    /// <summary>
    /// Validates that a date is not in the future
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <param name="paramName">The parameter name for the exception message</param>
    /// <exception cref="DomainException">Thrown when date is in the future</exception>
    public static void AgainstFutureDate(DateTime date, string? paramName = null)
    {
        if (date > DateTime.UtcNow)
            throw new DomainException($"{paramName ?? "Date"} cannot be in the future");
    }

    /// <summary>
    /// Validates that a DateOnly is not in the future
    /// </summary>
    /// <param name="date">The DateOnly to validate</param>
    /// <param name="paramName">The parameter name for the exception message</param>
    /// <exception cref="DomainException">Thrown when date is in the future</exception>
    public static void AgainstFutureDate(DateOnly date, string? paramName = null)
    {
        if (date > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException($"{paramName ?? "Date"} cannot be in the future");
    }


}

