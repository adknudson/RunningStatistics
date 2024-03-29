﻿using System;
using System.Linq;
using System.Numerics;

namespace RunningStatistics;

public static partial class Extensions
{
    public static int MinKey(this CountMap<int> countMap)
    {
        return countMap.Keys.Min();
    }
    
    public static long MinKey(this CountMap<long> countMap)
    {
        return countMap.Keys.Min();
    }
    
    public static double MinKey(this CountMap<double> countMap)
    {
        return countMap.Keys.Min();
    }
    
    public static decimal MinKey(this CountMap<decimal> countMap)
    {
        return countMap.Keys.Min();
    }
    
    public static T MinKey<T>(this CountMap<T> countMap) where T : notnull
    {
        var m = countMap.Keys.Min();
        
        if (m is null)
        {
            throw new NullReferenceException();
        }

        return m;
    }
    
    
    public static int MaxKey(this CountMap<int> countMap)
    {
        return countMap.Keys.Max();
    }
    
    public static long MaxKey(this CountMap<long> countMap)
    {
        return countMap.Keys.Max();
    }
    
    public static double MaxKey(this CountMap<double> countMap)
    {
        return countMap.Keys.Max();
    }
    
    public static decimal MaxKey(this CountMap<decimal> countMap)
    {
        return countMap.Keys.Max();
    }
    
    public static T MaxKey<T>(this CountMap<T> countMap) where T : notnull
    {
        var m = countMap.Keys.Max();
        
        if (m is null)
        {
            throw new NullReferenceException();
        }

        return m;
    }
    
    
    public static long Sum(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    public static long Sum(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    public static double Sum(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }

    public static decimal Sum(this CountMap<decimal> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value);
    }


    public static double Mean(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    public static double Mean(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    public static double Mean(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

    public static decimal Mean(this CountMap<decimal> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }


    public static double Variance(this CountMap<int> countMap, double mean)
    {
        return countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
    }

    public static double Variance(this CountMap<long> countMap, double mean)
    {
        return countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
    }

    public static double Variance(this CountMap<double> countMap, double mean)
    {
        return countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
    }

    public static double Variance(this CountMap<int> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }
    
    public static double Variance(this CountMap<long> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }
    
    public static double Variance(this CountMap<double> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }


    public static double StdDev(this CountMap<int> countMap)
    {
        return Math.Sqrt(countMap.Variance());
    }
    
    public static double StdDev(this CountMap<long> countMap)
    {
        return Math.Sqrt(countMap.Variance());
    }
    
    public static double StdDev(this CountMap<double> countMap)
    {
        return Math.Sqrt(countMap.Variance());
    }
    
    public static double StdDev(this CountMap<int> countMap, double mean)
    {
        return Math.Sqrt(countMap.Variance(mean));
    }
    
    public static double StdDev(this CountMap<long> countMap, double mean)
    {
        return Math.Sqrt(countMap.Variance(mean));
    }
    
    public static double StdDev(this CountMap<double> countMap, double mean)
    {
        return Math.Sqrt(countMap.Variance(mean));
    }


    public static double Skewness(this CountMap<int> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    public static double Skewness(this CountMap<long> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }

    public static double Skewness(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    public static double Skewness(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }
    
    public static double Skewness(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }
    
    public static double Skewness(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }
    

    public static double Kurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    public static double Kurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }

    public static double Kurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }
    
    public static double Kurtosis(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }
    
    public static double Kurtosis(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }
    
    public static double Kurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }

    
    public static double ExcessKurtosis(this CountMap<int> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }
    
    public static double ExcessKurtosis(this CountMap<long> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }
    
    public static double ExcessKurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
    }
    
    public static double ExcessKurtosis(this CountMap<int> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }
    
    public static double ExcessKurtosis(this CountMap<long> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }
    
    public static double ExcessKurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }

    
#if NET7_0_OR_GREATER
    
    /// <summary>
    /// Calculate the sum of the countmap of any generic <see cref="T"/> that supports multiplication by a <see cref="long"/>.
    /// </summary>
    /// <param name="countMap"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Sum<T>(this CountMap<T> countMap) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>,
        IMultiplyOperators<T, long, T>
    {
        return countMap.GenericSum(kvp => kvp.Key * kvp.Value);
    }
    
    /// <summary>
    /// Calculate the mean of the countmap of any generic <see cref="T"/> that supports multiplication and division by a <see cref="long"/>.
    /// </summary>
    public static T Mean<T>(this CountMap<T> countMap) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, 
        IMultiplyOperators<T, long, T>, IDivisionOperators<T, long, T>
    {
        return countMap.GenericSum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

#endif

}