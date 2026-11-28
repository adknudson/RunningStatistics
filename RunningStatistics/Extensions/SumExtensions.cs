using System;

namespace RunningStatistics;

public static class GenericSumExtensions
{
    /// <summary>
    /// Calculate the average of the sum of <see cref="int"/>s.
    /// </summary>
    public static double Mean(this Sum<int> sum)
    {
        return sum.Nobs > 0 ? (double)sum.Value / sum.Nobs : double.NaN;
    }
    
    /// <summary>
    /// Calculate the average of the sum of <see cref="long"/>s.
    /// </summary>
    public static double Mean(this Sum<long> sum)
    {
        return sum.Nobs > 0 ? (double)sum.Value / sum.Nobs : double.NaN;
    }
    
    /// <summary>
    /// Calculate the average of the sum of <see cref="double"/>s.
    /// </summary>
    public static double Mean(this Sum<double> sum)
    {
        return sum.Nobs > 0 ? sum.Value / sum.Nobs : double.NaN;
    }
    
    /// <summary>
    /// Calculate the average of the sum of <see cref="decimal"/>s.
    /// </summary>
    /// <exception cref="DivideByZeroException">Thrown when the number of observations is zero.</exception>
    public static decimal Mean(this Sum<decimal> sum)
    {
        if (sum.Nobs > 0)
        {
            return sum.Value / sum.Nobs;
        }
        
        // decimal doesn't have NaN values, so this is its proxy. Leave as DivideByZeroException.
        throw new DivideByZeroException("Average is undefined when the number of observations is zero.");
    }
}