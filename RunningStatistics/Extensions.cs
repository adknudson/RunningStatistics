using System;
using System.Linq;

namespace RunningStatistics;

public static class Extensions
{
    #region Sum
    
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
    
    #endregion

    
    #region CountMap

    public static T Mode<T>(this CountMap<T> countMap) where T : notnull
    {
        return countMap.MaxBy(b => b.Value).Key;
    }

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
    
    public static T Median<T>(this CountMap<T> countMap) where T : notnull
    {
        var sortedData = countMap.OrderBy(kvp => kvp.Key).ToList();
        var nobs = countMap.Nobs;
        var sum = 0.0;
        
        if (sortedData.Count % 2 != 0) // if N is odd
        {
            foreach (var (key, count) in sortedData)
            {
                sum += (double)count / nobs;
                if (sum > 0.5) return key;
            }
        }
        else // if N is even
        {
            foreach (var (key, count) in sortedData)
            {
                sum += (double)count / nobs;
                if (sum >= 0.5) return key;
            }
        }

        throw new Exception("Median not found");
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
    
    #endregion
}