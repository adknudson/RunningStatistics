using System;
using System.Diagnostics.CodeAnalysis;

namespace RunningStatistics;

internal static class Require
{
    /// <summary>
    /// Requires that the value is finite (must not be NaN or infinity).
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Finite(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value must be a finite number", nameof(value));
        }
    }

    /// <summary>
    /// Requires that the value is non-negative (must be greater than or equal to zero).
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
    /// <param name="p"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
    /// <param name="obj"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The object successfully cast to the specified type.</returns>
    /// <exception cref="InvalidCastException"></exception>
    public static void TypeToBe<T>(object? obj, [MaybeNullWhen(false)] out T value)
    {
        if (obj is not T typedValue)
        {
            throw new InvalidCastException($"The other stat must be of type {typeof(T)}. Got {obj?.GetType()}");
        }

        value = typedValue;
    }

    /// <summary>
    /// Requires that the value is not NaN (must be a real number or infinity).
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void NotNaN(double value)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("Value must not be NaN", nameof(value));
        }
    }
}