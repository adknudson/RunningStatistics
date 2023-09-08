using System;
using System.Linq;
using System.Numerics;

namespace RunningStatistics;

public static partial class Extensions
{
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


    public static double Variance(this CountMap<double> countMap)
    {
        return countMap.Variance(countMap.Mean());
    }

    public static double Variance(this CountMap<double> countMap, double mean)
    {
        return countMap.Sum(x => x.Value * Math.Pow(x.Key - mean, 2) / countMap.Nobs);
    }


    public static double Skewness(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Skewness(mean, variance);
    }

    public static double Skewness(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 3) / countMap.Nobs);
    }
    
    
    public static double Kurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.Kurtosis(mean, variance);
    }

    public static double Kurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        var std = Math.Sqrt(variance);
        return countMap.Sum(x => x.Value * Math.Pow((x.Key - mean) / std, 4) / countMap.Nobs);
    }
    
    
    public static double ExcessKurtosis(this CountMap<double> countMap)
    {
        var mean = countMap.Mean();
        var variance = countMap.Variance(mean);
        return countMap.ExcessKurtosis(mean, variance);
    }

    public static double ExcessKurtosis(this CountMap<double> countMap, double mean, double variance)
    {
        return countMap.Kurtosis(mean, variance) - 3;
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
        return countMap.MyGenericSum(kvp => kvp.Key * kvp.Value);
    }
    
    /// <summary>
    /// Calculate the mean of the countmap of any generic <see cref="T"/> that supports multiplication and division by a <see cref="long"/>.
    /// </summary>
    public static T Mean<T>(this CountMap<T> countMap) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, 
        IMultiplyOperators<T, long, T>, IDivisionOperators<T, long, T>
    {
        return countMap.MyGenericSum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

#endif

}