using System;
using System.Linq;

namespace RunningStatistics;

public static class Extensions
{

#if NET7_0_OR_GREATER
    
    public static double Mean(this Sum<double> sum)
    {
        return sum.Value / sum.Nobs;
    }
    
    public static double Mean(this Sum<int> sum)
    {
        return (double) sum.Value / sum.Nobs;
    }
    
    public static double Mean(this Sum<long> sum)
    {
        return (double) sum.Value / sum.Nobs;
    }
    
    public static decimal Mean(this Sum<decimal> sum)
    {
        return sum.Value / sum.Nobs;
    }

#endif

    public static double Mean(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

    public static double Mean(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }
    
    public static double Mean(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
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
}