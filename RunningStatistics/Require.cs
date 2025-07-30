using System;

namespace RunningStatistics;

internal static class Require
{
    /// <summary>
    /// Requires that the value is finite (must not be NaN or infinity).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <exception cref="ArgumentException">When the value is NaN or infinity.</exception>
    public static void Finite(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value must be a finite number", nameof(value));
        }
    }
    
    /// <summary>
    /// Requires that the value is not NaN (must be a real number or infinity).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <exception cref="ArgumentException">When the value is NaN.</exception>
    public static void NotNaN(double value)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("Value must not be NaN", nameof(value));
        }
    }

    /// <summary>
    /// Requires that the value is non-negative (must be greater than or equal to zero).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <exception cref="ArgumentOutOfRangeException">When the value is negative.</exception>
    public static void NonNegative(long value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be non-negative");
        }
    }

    /// <summary>
    /// Requires that the value is in the interval [0, 1].
    /// </summary>
    /// <param name="p">The probability value to check.</param>
    /// <exception cref="ArgumentOutOfRangeException">When the value is not in the interval [0, 1].</exception>
    public static void ValidProbability(double p)
    {
        if (p is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(p), p, "Value must be in the interval [0, 1]");
        }
    }

    /// <summary>
    /// Requires that the value is not null and of the specified type.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <param name="value">The object successfully cast to the specified type.</param>
    /// <typeparam name="T">The type to cast the object to.</typeparam>
    /// <returns>The object successfully cast as the specified type.</returns>
    /// <exception cref="InvalidCastException">When the object is not of the specified type.</exception>
    public static void TypeToBe<T>(object? obj, out T value)
    {
        if (obj is not T typedValue)
        {
            throw new InvalidCastException($"The other stat must be of type {typeof(T)}. Got {obj?.GetType()}");
        }

        value = typedValue;
    }
}