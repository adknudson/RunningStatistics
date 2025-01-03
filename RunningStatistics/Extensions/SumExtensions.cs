using System;
using System.Numerics;

namespace RunningStatistics;

public static class GenericSumExtensions
{
    /// <summary>
    /// Calculate the average of the observations.
    /// </summary>
    public static double Mean(this Sum sum)
    {
        return sum.Nobs > 0 ? sum.Value / sum.Nobs : double.NaN;
    }
    
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
        
        throw new DivideByZeroException("Average is undefined when the number of observations is zero.");
    }
    
    /// <summary>
    /// Calculate the average of the sum of any generic <see cref="T"/> that supports division by a <see cref="long"/>.
    /// </summary>
    /// <exception cref="DivideByZeroException">Thrown when the number of observations is zero.</exception>
    public static T Mean<T>(this Sum<T> sum)
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, IDivisionOperators<T, long, T>
    {
        if (sum.Nobs > 0)
        {
            return sum.Value / sum.Nobs;
        }
        
        throw new DivideByZeroException("Average is undefined when the number of observations is zero.");
    }
}
