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
    /// Compute the sample variance of a CountMap of integers.
    /// </summary>
    public static double Variance(this CountMap<int> countMap, double mean, bool corrected = true)
    {
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return corrected ? v * Utils.Bessel(countMap.Nobs) : v;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of longs.
    /// </summary>
    public static double Variance(this CountMap<long> countMap, double mean, bool corrected = true)
    {
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return corrected ? v * Utils.Bessel(countMap.Nobs) : v;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of doubles.
    /// </summary>
    public static double Variance(this CountMap<double> countMap, double mean, bool corrected = true)
    {
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return corrected ? v * Utils.Bessel(countMap.Nobs) : v;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of decimals. The result is a decimal:
    /// the sum of squared deviations and the Bessel correction both stay in decimal
    /// arithmetic, so no binary rounding error enters the result.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// Thrown by the corrected form when the count map holds a single observation, since
    /// decimal division by zero throws instead of yielding infinity.
    /// </exception>
    public static decimal Variance(this CountMap<decimal> countMap, decimal mean, bool corrected = true)
    {
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
    /// at least three observations and returns NaN below that limit.
    /// </summary>
    public static double Skewness(this CountMap<int> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of longs. Sample skewness needs
    /// at least three observations and returns NaN below that limit.
    /// </summary>
    public static double Skewness(this CountMap<long> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the standardized skewness of a CountMap of doubles. Sample skewness needs
    /// at least three observations and returns NaN below that limit.
    /// </summary>
    public static double Skewness(this CountMap<double> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the standardized skewness of a CountMap of decimals. Sample skewness needs
    /// at least three observations and returns NaN below that limit. The standardized
    /// result is a double because it involves division by a square root.
    /// </summary>
    public static double Skewness(this CountMap<decimal> countMap, decimal mean, decimal variance)
    {
        if (countMap.Nobs < 3) return double.NaN;
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
    /// observations and returns NaN below that limit.
    /// </summary>
    public static double Kurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of longs. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit.
    /// </summary>
    public static double Kurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of doubles. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit.
    /// </summary>
    public static double Kurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of decimals. Sample kurtosis needs at least four
    /// observations and returns NaN below that limit. The standardized result is a double
    /// because it involves division by a square root.
    /// </summary>
    public static double Kurtosis(this CountMap<decimal> countMap, decimal mean, decimal variance)
    {
        if (countMap.Nobs < 4) return double.NaN;
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
    
    /// <summary>
    /// Compute the mode of a CountMap.
    /// </summary>
    /// <param name="countMap">The count map to compute the mode for.</param>
    /// <typeparam name="TObs">The type of the observations in the count map.</typeparam>
    /// <returns>The observation with the highest count.</returns>
    /// <exception cref="Exception">Thrown when the count map has no observations. The mode does not exist.</exception>
    /// <exception cref="NullReferenceException">Thrown when the caller passes a null count map.</exception>
    public static TObs Mode<TObs>(this CountMap<TObs> countMap) where TObs : notnull
    {
        if (countMap.Nobs == 0)
        {
            throw new Exception("Nobs = 0. The mode does not exist.");
        }
        
        TObs? mode = default;
        var maxCount = 0L;

        foreach (var kvp in countMap)
        {
            if (kvp.Value <= maxCount) continue;
            mode = kvp.Key;
            maxCount = kvp.Value;
        }
        
        return mode ?? throw new NullReferenceException();
    }

    /// <summary>
    /// Compute the p-th quantile of a CountMap.
    /// If <c>p</c> is less than or equal to 0, the method returns the minimum key.
    /// If <c>p</c> is greater than or equal to 1, the method returns the maximum key.
    /// </summary>
    /// <param name="countMap">The count map to compute the quantile for.</param>
    /// <param name="p">The quantile to compute. Pass a value between 0 and 1.</param>
    /// <param name="comparer">The comparer to use for ordering the keys.
    /// If null, the method uses the default comparer.</param>
    /// <typeparam name="TObs">The type of the observations in the count map.</typeparam>
    /// <returns>The p-th quantile of the count map.</returns>
    /// <exception cref="Exception">
    /// Thrown if the count map is empty or if the quantile cannot be found.
    /// </exception>
    public static TObs Quantile<TObs>(this CountMap<TObs> countMap, double p, IComparer<TObs>? comparer) where TObs : notnull
    {
        if (countMap.Nobs == 0)
        {
            throw new Exception("The count map is empty. The quantile does not exist.");
        }

        if (p <= 0) return countMap.MinKey(comparer);
        if (p >= 1) return countMap.MaxKey(comparer);
        
        var cdf = 0.0;
        var nobs = (double)countMap.Nobs;
        
        if (countMap.Count % 2 == 0)
        {
            foreach (var kvp in countMap.OrderBy(kvp => kvp.Key, comparer))
            {
                cdf += kvp.Value / nobs;
                if (cdf >= p) return kvp.Key;
            }
        }
        else
        {
            foreach (var kvp in countMap.OrderBy(kvp => kvp.Key, comparer))
            {
                cdf += kvp.Value / nobs;
                if (cdf > p) return kvp.Key;
            }
        }
        
        // This should be unreachable...
        throw new Exception($"Not able to find the {p} quantile of the count map.");
    }
    
    /// <summary>
    /// Compute the p-th quantile of a CountMap.
    /// If <c>p</c> is less than or equal to 0, the method returns the minimum key.
    /// If <c>p</c> is greater than or equal to 1, the method returns the maximum key.
    /// </summary>
    /// <param name="countMap">The count map to compute the quantile for.</param>
    /// <param name="p">The quantile to compute. Pass a value between 0 and 1.</param>
    /// <typeparam name="TObs">The type of the observations in the count map.</typeparam>
    /// <returns>The p-th quantile of the count map.</returns>
    /// <remarks>
    /// This method uses the comparer associated with the CountMap. If the CountMap
    /// has no comparer, the method uses the default comparer instead.
    /// </remarks>
    public static TObs Quantile<TObs>(this CountMap<TObs> countMap, double p) where TObs : notnull
    {
        return countMap.Quantile(p, countMap.Comparer);
    }
    
    /// <summary>
    /// Compute the median of a CountMap using the specified comparer.
    /// This method returns the result of <c>Quantile(0.5, comparer)</c>.
    /// </summary>
    /// <param name="countMap">The count map to compute the median for.</param>
    /// <param name="comparer">The comparer to use for ordering the keys.
    /// If null, the method uses the default comparer.</param>
    /// <typeparam name="TObs">The type of the observations in the count map.</typeparam>
    /// <returns>The median of the count map.</returns>
    public static TObs Median<TObs>(this CountMap<TObs> countMap, IComparer<TObs>? comparer) where TObs : notnull
    {
        return countMap.Quantile(0.5, comparer);
    }
    
    /// <summary>
    /// Compute the median of a CountMap.
    /// </summary>
    /// <param name="countMap">The count map to compute the median for.</param>
    /// <typeparam name="TObs">The type of the observations in the count map.</typeparam>
    /// <returns>The median of the count map.</returns>
    /// <remarks>
    /// This method uses the comparer associated with the CountMap. If the CountMap
    /// has no comparer, the method uses the default comparer instead.
    /// </remarks>
    public static TObs Median<TObs>(this CountMap<TObs> countMap) where TObs : notnull
    {
        return countMap.Median(countMap.Comparer);
    }
}