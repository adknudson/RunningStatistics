using System;
using System.Linq;

#if NET7_0_OR_GREATER
using System.Numerics;
#endif

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace RunningStatistics;

public static class CountMapExtensions
{
    /// <summary>
    /// Find the minimum key in a CountMap.
    /// </summary>
    public static T MinKey<T>(this CountMap<T> countMap) where T : notnull
    {
        var minKey = countMap.Keys.Min();
        return minKey ?? throw new NullReferenceException();
    }
    
    /// <summary>
    /// Find the maximum key in a CountMap.
    /// </summary>
    public static T MaxKey<T>(this CountMap<T> countMap) where T : notnull
    {
        var maxKey = countMap.Keys.Max();
        return maxKey ?? throw new NullReferenceException();
    }
    
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

#if NET7_0_OR_GREATER

    /// <summary>
    /// Find the sum of all observations in a CountMap of any generic type that supports addition and
    /// multiplication by a <see cref="long"/>.
    /// </summary>
    public static T Sum<T>(this CountMap<T> countMap) 
        where T : 
        IAdditionOperators<T, T, T>,
        IAdditiveIdentity<T, T>,
        IMultiplyOperators<T, long, T>
    {
        var s = T.AdditiveIdentity;

        foreach (var (x, k) in countMap)
        {
            s += x * k;
        }

        return s;
    }

#endif
    
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

#if NET7_0_OR_GREATER
    
    /// <summary>
    /// Compute the mean of a CountMap of any generic type that supports addition, multiplication by a <see cref="long"/>,
    /// and division by a <see cref="long"/>.
    /// </summary>
    public static T Mean<T>(this CountMap<T> countMap) 
        where T : 
        IAdditionOperators<T, T, T>, 
        IAdditiveIdentity<T, T>, 
        IMultiplyOperators<T, long, T>, 
        IDivisionOperators<T, long, T>
    {
        var m = T.AdditiveIdentity;
        var n = countMap.Nobs;

        foreach (var (x, k) in countMap)
        {
            m += x * k / n;
        }

        return m;
    }

#endif
    
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
    /// Compute the standardized skewness of a CountMap of integers.
    /// </summary>
    public static double Skewness(this CountMap<int> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    /// <summary>
    /// Compute the standardized skewness of a CountMap of longs.
    /// </summary>
    public static double Skewness(this CountMap<long> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the standardized skewness of a CountMap of doubles.
    /// </summary>
    public static double Skewness(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
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
    /// Compute the kurtosis of a CountMap of integers.
    /// </summary>
    public static double Kurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of longs.
    /// </summary>
    public static double Kurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// Compute the kurtosis of a CountMap of doubles.
    /// </summary>
    public static double Kurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
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
    /// Compute the mode of a CountMap.
    /// </summary>
    /// <param name="countMap"></param>
    /// <typeparam name="TObs"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NullReferenceException"></exception>
    public static TObs Mode<TObs>(this CountMap<TObs> countMap) where TObs : notnull
    {
        if (countMap.Nobs == 0)
        {
            throw new Exception("Nobs = 0. The mode does not exist.");
        }

#if NET6_0_OR_GREATER
        return countMap.MaxBy(kvp => kvp.Value).Key;
#else
        TObs? mode = default;
        var maxCount = 0L;

        foreach (var kvp in countMap)
        {
            if (kvp.Value <= maxCount) continue;
            mode = kvp.Key;
            maxCount = kvp.Value;
        }
        
        return mode ?? throw new NullReferenceException();
#endif
    }
    
    public static TObs Median<TObs>(this CountMap<TObs> countMap) where TObs : notnull
    {
        if (countMap.Nobs == 0)
        {
            throw new Exception("Nobs = 0. The median does not exist.");
        }

        if (countMap.Count % 2 == 0)
        {
            var cdf = 0.0;
        
            foreach (var kvp in countMap.OrderBy(kvp => kvp.Key)) 
            {
                cdf += (double)kvp.Value / countMap.Nobs;
                if (cdf >= 0.5) return kvp.Key;
            }
        }
        else
        {
            var cdf = 0.0;
        
            foreach (var kvp in countMap.OrderBy(kvp => kvp.Key))
            {
                cdf += (double)kvp.Value / countMap.Nobs;
                if (cdf > 0.5) return kvp.Key;
            }
        } 

        // This should be unreachable...
        throw new Exception("Not able to find the median of the count map.");
    }
}