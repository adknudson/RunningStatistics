using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

internal static class Utils
{
    public static double BesselCorrection(long n)
    {
        return (double) n / (n - 1);
    }

    /// <summary>
    /// Weighted interpolation of two values.
    /// </summary>
    public static double Smooth(double a, double b, double w)
    {
        return a + w * (b - a);
    }
    
    public static double Mean(IEnumerable<double> values, IEnumerable<double> probabilities)
    {
        return values.Zip(probabilities, (vs, ps) => (vs, ps)).Sum(x => x.vs * x.ps);
    }

    /// <summary>
    /// Compute the (biased) variance of a collection of values.
    /// </summary>
    public static double Variance(IEnumerable<double> xs, double mean)
    {
        var meanSquare = xs.Average(s => s * s);
        return Math.Abs(meanSquare - mean * mean);
    }
    
    public static double Variance(IEnumerable<double> values, IEnumerable<double> probabilities, double mean)
    {
        return values.Zip(probabilities, (vs, ps) => (vs, ps)).Sum(x => x.ps * Math.Pow(x.vs - mean, 2.0));
    }

    public static double Skewness(IEnumerable<double> values, IEnumerable<double> probabilities, 
        double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return values.Zip(probabilities, (vs, ps) => (vs, ps)).Sum(x => x.ps * Math.Pow((x.vs - mean) / std, 3));
    }

    public static double Kurtosis(IEnumerable<double> values, IEnumerable<double> probabilities,
        double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return values.Zip(probabilities, (vs, ps) => (vs, ps)).Sum(x => x.ps * Math.Pow((x.vs - mean) / std, 4));
    }

    public static double ExcessKurtosis(IEnumerable<double> values, IEnumerable<double> probabilities,
        double mean, double variance)
    {
        return Kurtosis(values, probabilities, mean, variance) - 3;
    }
}