using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public static partial class CountMapExtensions
{
    /// <summary>
    /// Find the sum of all observations in a CountMap of integers.
    /// </summary>
    public static long Sum(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    /// <summary>
    /// Find the sum of all observations in a CountMap of longs.
    /// </summary>
    public static long Sum(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    /// <summary>
    /// Find the sum of all observations in a CountMap of doubles.
    /// </summary>
    public static double Sum(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    /// <summary>
    /// Find the sum of all observations in a CountMap of decimals.
    /// </summary>
    public static decimal Sum(this CountMap<decimal> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    /// <summary>
    /// Count the number of observations in a count map that satisfy the predicate.
    /// </summary>
    public static long Nobs<T>(this CountMap<T> countMap, Func<T, bool> predicate) where T : notnull
    {
        return countMap.Where(pair => predicate(pair.Key)).Sum(pair => pair.Value);
    }
    
    /// <summary>
    /// Compute the mean of a CountMap of integers.
    /// </summary>
    public static double Mean(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    /// <summary>
    /// Compute the mean of a CountMap of longs.
    /// </summary>
    public static double Mean(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    /// <summary>
    /// Compute the mean of a CountMap of doubles.
    /// </summary>
    public static double Mean(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

    /// <summary>
    /// Compute the mean of a CountMap of decimals.
    /// </summary>
    public static decimal Mean(this CountMap<decimal> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }
    
    /// <summary>
    /// Compute the sample variance of a CountMap of integers.
    /// </summary>
    public static double Variance(this CountMap<int> countMap, bool corrected = true)
    {
        return countMap.Variance(countMap.Mean(), corrected);
    }
    
    /// <summary>
    /// Compute the sample variance of a CountMap of longs.
    /// </summary>
    public static double Variance(this CountMap<long> countMap, bool corrected = true)
    {
        return countMap.Variance(countMap.Mean(), corrected);
    }
    
    /// <summary>
    /// Compute the sample variance of a CountMap of doubles.
    /// </summary>
    public static double Variance(this CountMap<double> countMap, bool corrected = true)
    {
        return countMap.Variance(countMap.Mean(), corrected);
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of decimals.
    /// </summary>
    public static decimal Variance(this CountMap<decimal> countMap, bool corrected = true)
    {
        return countMap.Variance(countMap.Mean(), corrected);
    }
    
    /// <summary>
    /// Compute the sample variance of a CountMap of integers. The corrected form answers NaN
    /// below two observations; the uncorrected form below one.
    /// </summary>
    public static double Variance(this CountMap<int> countMap, double mean, bool corrected = true)
    {
        if (countMap.Nobs < (corrected ? 2 : 1)) return double.NaN;
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return corrected ? v * Utils.Bessel(countMap.Nobs) : v;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of longs. The corrected form answers NaN
    /// below two observations; the uncorrected form below one.
    /// </summary>
    public static double Variance(this CountMap<long> countMap, double mean, bool corrected = true)
    {
        if (countMap.Nobs < (corrected ? 2 : 1)) return double.NaN;
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return corrected ? v * Utils.Bessel(countMap.Nobs) : v;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of doubles. The corrected form answers NaN
    /// below two observations; the uncorrected form below one.
    /// </summary>
    public static double Variance(this CountMap<double> countMap, double mean, bool corrected = true)
    {
        if (countMap.Nobs < (corrected ? 2 : 1)) return double.NaN;
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return corrected ? v * Utils.Bessel(countMap.Nobs) : v;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of decimals. The result is a decimal:
    /// the sum of squared deviations and the Bessel correction both stay in decimal
    /// arithmetic, so no binary rounding error enters the result.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// Thrown when the count map has too few observations: the corrected form needs at
    /// least two, the uncorrected form at least one. decimal has no NaN to hand back, so
    /// this follows the decimal Mean precedent of throwing.
    /// </exception>
    public static decimal Variance(this CountMap<decimal> countMap, decimal mean, bool corrected = true)
    {
        if (countMap.Nobs < (corrected ? 2 : 1))
        {
            throw new DivideByZeroException("Variance is undefined: the corrected form needs at least two observations, the uncorrected form at least one.");
        }

        var v = countMap.Sum(x => x.Value * (x.Key - mean) * (x.Key - mean)) / countMap.Nobs;
        return corrected ? v * countMap.Nobs / (countMap.Nobs - 1) : v;
    }
    
    /// <summary>
    /// Compute the sample standard deviation of a CountMap of integers.
    /// </summary>
    public static double StandardDeviation(this CountMap<int> countMap, bool corrected = true)
    {
        return Math.Sqrt(countMap.Variance(corrected));
    }
    
    /// <summary>
    /// Compute the sample standard deviation of a CountMap of longs.
    /// </summary>
    public static double StandardDeviation(this CountMap<long> countMap, bool corrected = true)
    {
        return Math.Sqrt(countMap.Variance(corrected));
    }
    
    /// <summary>
    /// Compute the sample standard deviation of a CountMap of doubles.
    /// </summary>
    public static double StandardDeviation(this CountMap<double> countMap, bool corrected = true)
    {
        return Math.Sqrt(countMap.Variance(corrected));
    }

    /// <summary>
    /// Compute the sample standard deviation of a CountMap of decimals. The result is a
    /// double: the square root of a decimal is generally irrational, so it has no exact
    /// decimal representation.
    /// </summary>
    public static double StandardDeviation(this CountMap<decimal> countMap, bool corrected = true)
    {
        return Math.Sqrt((double)countMap.Variance(corrected));
    }

    /// <summary>
    /// Compute the sample standard deviation of a CountMap of integers.
    /// </summary>
    public static double StandardDeviation(this CountMap<int> countMap, double mean, bool corrected = true)
    {
        return Math.Sqrt(countMap.Variance(mean, corrected));
    }

    /// <summary>
    /// Compute the sample standard deviation of a CountMap of longs.
    /// </summary>
    public static double StandardDeviation(this CountMap<long> countMap, double mean, bool corrected = true)
    {
        return Math.Sqrt(countMap.Variance(mean, corrected));
    }
    
    /// <summary>
    /// Compute the sample standard deviation of a CountMap of doubles.
    /// </summary>
    public static double StandardDeviation(this CountMap<double> countMap, double mean, bool corrected = true)
    {
        return Math.Sqrt(countMap.Variance(mean, corrected));
    }

    /// <summary>
    /// Compute the sample standard deviation of a CountMap of decimals. The result is a
    /// double for the same reason as the other standard deviation: the square root of a
    /// decimal is generally irrational.
    /// </summary>
    public static double StandardDeviation(this CountMap<decimal> countMap, decimal mean, bool corrected = true)
    {
        return Math.Sqrt((double)countMap.Variance(mean, corrected));
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of integers. Sample skewness needs
    /// at least three observations and returns NaN below that limit. A zero or negative
    /// variance also gives NaN: the standardization divides by the spread, and constant
    /// data has none, the same limit Moments guards on its central second moment.
    /// </summary>
    public static double Skewness(this CountMap<int> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of longs. Sample skewness needs
    /// at least three observations and returns NaN below that limit. A zero or negative
    /// variance also gives NaN: the standardization divides by the spread.
    /// </summary>
    public static double Skewness(this CountMap<long> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the standardized skewness of a CountMap of doubles. Sample skewness needs
    /// at least three observations and returns NaN below that limit. A zero or negative
    /// variance also gives NaN: the standardization divides by the spread.
    /// </summary>
    public static double Skewness(this CountMap<double> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the standardized skewness of a CountMap of decimals. Sample skewness needs
    /// at least three observations and returns NaN below that limit. A zero or negative
    /// variance also gives NaN: the standardization divides by the spread. The standardized
    /// result is a double because it involves division by a square root.
    /// </summary>
    public static double Skewness(this CountMap<decimal> countMap, decimal mean, decimal variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt((double)variance);
        return countMap.Sum(x => x.Value * Math.Pow(((double)x.Key - (double)mean) / std, 3) / countMap.Nobs);
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of integers.
    /// </summary>
    public static double Skewness(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Skewness(mean, variance);
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of longs.
    /// </summary>
    public static double Skewness(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Skewness(mean, variance);
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of doubles.
    /// </summary>
    public static double Skewness(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Skewness(mean, variance);
    }

    /// <summary>
    /// Compute the standardized skewness of a CountMap of decimals.
    /// </summary>
    public static double Skewness(this CountMap<decimal> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Skewness(mean, variance);
    }
    
    /// <summary>
    /// Compute the kurtosis of a CountMap of integers. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit. A zero or negative variance also gives
    /// NaN: the standardization divides by the spread.
    /// </summary>
    public static double Kurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of longs. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit. A zero or negative variance also gives
    /// NaN: the standardization divides by the spread.
    /// </summary>
    public static double Kurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of doubles. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit. A zero or negative variance also gives
    /// NaN: the standardization divides by the spread.
    /// </summary>
    public static double Kurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of decimals. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit. A zero or negative variance also gives
    /// NaN: the standardization divides by the spread. The standardized result is a double
    /// because it involves division by a square root.
    /// </summary>
    public static double Kurtosis(this CountMap<decimal> countMap, decimal mean, decimal variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        if (variance <= 0) return double.NaN;
        var std = Math.Sqrt((double)variance);
        return countMap.Sum(x => x.Value * Math.Pow(((double)x.Key - (double)mean) / std, 4) / countMap.Nobs);
    }
    
    /// <summary>
    /// Compute the kurtosis of a CountMap of integers.
    /// </summary>
    public static double Kurtosis(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Kurtosis(mean, variance);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of longs.
    /// </summary>
    public static double Kurtosis(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Kurtosis(mean, variance);
    }
    
    /// <summary>
    /// Compute the kurtosis of a CountMap of doubles.
    /// </summary>
    public static double Kurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Kurtosis(mean, variance);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of decimals.
    /// </summary>
    public static double Kurtosis(this CountMap<decimal> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.Kurtosis(mean, variance);
    }

    /// <summary>
    /// Compute the excess kurtosis of a CountMap of integers.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }

    /// <summary>
    /// Compute the excess kurtosis of a CountMap of longs.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }

    /// <summary>
    /// Compute the excess kurtosis of a CountMap of doubles.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }

    /// <summary>
    /// Compute the excess kurtosis of a CountMap of decimals.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<decimal> countMap, decimal mean, decimal variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }
    
    /// <summary>
    /// Compute the excess kurtosis of a CountMap of integers.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.ExcessKurtosis(mean, variance);
    }
    
    /// <summary>
    /// Compute the excess kurtosis of a CountMap of longs.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.ExcessKurtosis(mean, variance);
    }

    /// <summary>
    /// Compute the excess kurtosis of a CountMap of doubles.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.ExcessKurtosis(mean, variance);
    }

    /// <summary>
    /// Compute the excess kurtosis of a CountMap of decimals.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<decimal> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean, false);
        return countMap.ExcessKurtosis(mean, variance);
    }
}