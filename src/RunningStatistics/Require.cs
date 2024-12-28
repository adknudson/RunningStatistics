using System;

namespace RunningStatistics;

internal static class Require
{
    public static void Finite(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value must be a finite number", nameof(value));
        }
    }

    public static void NonNegative(long value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be non-negative");
        }
    }

    public static void ValidProbability(double p)
    {
        if (p is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(p), p, "Value must be in the interval [0, 1]");
        }
    }

    public static T Type<T>(object? obj)
    {
        if (obj is not T typed)
        {
            throw new ArgumentException($"The other stat must be of type {typeof(T)}", nameof(obj));
        }

        return typed;
    }

    public static void NotNaN(double value)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("Value must not be NaN", nameof(value));
        }
    }
}