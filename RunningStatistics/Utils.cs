using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

internal static class Utils
{
    internal static double BesselCorrection(nint n)
    {
        return (double) n / (n - 1);
    }

    /// <summary>
    /// Weighted interpolation of two values.
    /// </summary>
    internal static double Smooth(double a, double b, double w)
    {
        return a + w * (b - a);
    }

    /// <summary>
    /// Compute the (biased) variance of a collection of values.
    /// </summary>
    internal static double Variance(IList<double> xs)
    {
        var mean = xs.Average();
        var meanSquare = xs.Average(s => s * s);

        return Math.Abs(meanSquare - mean * mean);
    }
}