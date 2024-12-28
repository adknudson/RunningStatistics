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

    public static void NotNaN(double value)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("Value must not be NaN", nameof(value));
        }
    }
}