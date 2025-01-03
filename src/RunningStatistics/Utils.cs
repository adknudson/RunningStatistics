using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

internal static class Utils
{
    /// <summary>
    /// Bessel correction for sample variance.
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static double Bessel(double n)
    {
        return n / (n - 1);
    }

    /// <summary>
    /// Weighted interpolation of two values.
    /// </summary>
    public static double Smooth(double a, double b, double w)
    {
        return a + w * (b - a);
    }

    /// <summary>
    /// Compute the (biased) variance of a collection of values.
    /// </summary>
    public static double Variance(IEnumerable<double> xs, double mean)
    {
        var meanSquare = xs.Average(s => s * s);
        return Math.Abs(meanSquare - mean * mean);
    }
}