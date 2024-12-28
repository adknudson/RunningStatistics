using System;
using System.Linq;
using System.Numerics;

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
    /// The mean of a CountMap of integers.
    /// </summary>
    public static double Mean(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    /// <summary>
    /// The mean of a CountMap of longs.
    /// </summary>
    public static double Mean(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    /// <summary>
    /// The mean of a CountMap of doubles.
    /// </summary>
    public static double Mean(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

    /// <summary>
    /// The mean of a CountMap of decimals.
    /// </summary>
    public static decimal Mean(this CountMap<decimal> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

#if NET7_0_OR_GREATER
    
    /// <summary>
    /// The mean of a CountMap of any generic type that supports addition, multiplication by a <see cref="long"/>,
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
    /// The bias-corrected variance of a CountMap of integers.
    /// </summary>
    public static double Variance(this CountMap<int> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }
    
    /// <summary>
    /// The bias-corrected variance of a CountMap of longs.
    /// </summary>
    public static double Variance(this CountMap<long> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }
    
    /// <summary>
    /// The bias-corrected variance of a CountMap of doubles.
    /// </summary>
    public static double Variance(this CountMap<double> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }
    
    /// <summary>
    /// The bias-corrected variance of a CountMap of integers.
    /// </summary>
    public static double Variance(this CountMap<int> countMap, double mean)
    {
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return v * Utils.Bessel(countMap.Nobs);
    }

    /// <summary>
    /// The bias-corrected variance of a CountMap of longs.
    /// </summary>
    public static double Variance(this CountMap<long> countMap, double mean)
    {
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return v * Utils.Bessel(countMap.Nobs);
    }

    /// <summary>
    /// The bias-corrected variance of a CountMap of doubles.
    /// </summary>
    public static double Variance(this CountMap<double> countMap, double mean)
    {
        var v = countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
        return v * Utils.Bessel(countMap.Nobs);
    }
    
    /// <summary>
    /// The sample standard deviation of a CountMap of integers.
    /// </summary>
    public static double StandardDeviation(this CountMap<int> countMap)
    {
        return Math.Sqrt(countMap.Variance());
    }
    
    /// <summary>
    /// The sample standard deviation of a CountMap of longs.
    /// </summary>
    public static double StandardDeviation(this CountMap<long> countMap)
    {
        return Math.Sqrt(countMap.Variance());
    }
    
    /// <summary>
    /// The sample standard deviation of a CountMap of doubles.
    /// </summary>
    public static double StandardDeviation(this CountMap<double> countMap)
    {
        return Math.Sqrt(countMap.Variance());
    }

    /// <summary>
    /// The sample standard deviation of a CountMap of integers.
    /// </summary>
    public static double StandardDeviation(this CountMap<int> countMap, double mean)
    {
        return Math.Sqrt(countMap.Variance(mean));
    }

    /// <summary>
    /// The sample standard deviation of a CountMap of longs.
    /// </summary>
    public static double StandardDeviation(this CountMap<long> countMap, double mean)
    {
        return Math.Sqrt(countMap.Variance(mean));
    }
    
    /// <summary>
    /// The sample standard deviation of a CountMap of doubles.
    /// </summary>
    public static double StandardDeviation(this CountMap<double> countMap, double mean)
    {
        return Math.Sqrt(countMap.Variance(mean));
    }
    
    /// <summary>
    /// The skewness of a CountMap of integers.
    /// </summary>
    public static double Skewness(this CountMap<int> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    /// <summary>
    /// The skewness of a CountMap of longs.
    /// </summary>
    public static double Skewness(this CountMap<long> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    /// <summary>
    /// The skewness of a CountMap of doubles.
    /// </summary>
    public static double Skewness(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    /// <summary>
    /// The skewness of a CountMap of integers.
    /// </summary>
    public static double Skewness(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }
    
    /// <summary>
    /// The skewness of a CountMap of longs.
    /// </summary>
    public static double Skewness(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }
    
    /// <summary>
    /// The skewness of a CountMap of doubles.
    /// </summary>
    public static double Skewness(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }
    
    /// <summary>
    /// The kurtosis of a CountMap of integers.
    /// </summary>
    public static double Kurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// The kurtosis of a CountMap of longs.
    /// </summary>
    public static double Kurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    /// <summary>
    /// The kurtosis of a CountMap of doubles.
    /// </summary>
    public static double Kurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }
    
    /// <summary>
    /// The kurtosis of a CountMap of integers.
    /// </summary>
    public static double Kurtosis(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }

    /// <summary>
    /// The kurtosis of a CountMap of longs.
    /// </summary>
    public static double Kurtosis(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }
    
    /// <summary>
    /// The kurtosis of a CountMap of doubles.
    /// </summary>
    public static double Kurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }

    /// <summary>
    /// The excess kurtosis of a CountMap of integers.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }

    /// <summary>
    /// The excess kurtosis of a CountMap of longs.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }

    /// <summary>
    /// The excess kurtosis of a CountMap of doubles.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }
    
    /// <summary>
    /// The excess kurtosis of a CountMap of integers.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }
    
    /// <summary>
    /// The excess kurtosis of a CountMap of longs.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }

    /// <summary>
    /// The excess kurtosis of a CountMap of doubles.
    /// </summary>
    public static double ExcessKurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }
}