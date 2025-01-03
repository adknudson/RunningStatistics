using System.Numerics;

namespace RunningStatistics;

public static class GenericSumExtensions
{
    /// <summary>
    /// Calculate the mean of the sum of <see cref="int"/>s.
    /// </summary>
    public static double Mean(this Sum<int> sum)
    {
        return (double) sum.Value / sum.Nobs;
    }
    
    /// <summary>
    /// Calculate the mean of the sum of <see cref="long"/>s.
    /// </summary>
    public static double Mean(this Sum<long> sum)
    {
        return (double) sum.Value / sum.Nobs;
    }
    
    /// <summary>
    /// Calculate the mean of the sum of <see cref="double"/>s.
    /// </summary>
    public static double Mean(this Sum<double> sum)
    {
        return sum.Value / sum.Nobs;
    }

    /// <summary>
    /// Calculate the mean of the sum of <see cref="decimal"/>s.
    /// </summary>
    public static decimal Mean(this Sum<decimal> sum)
    {
        return sum.Value / sum.Nobs;
    }
    
    /// <summary>
    /// Calculate the mean of the sum of any generic <see cref="T"/> that supports division by a <see cref="long"/>.
    /// </summary>
    public static T Mean<T>(this Sum<T> sum)
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, IDivisionOperators<T, long, T>
    {
        return sum.Value / sum.Nobs;
    }
}
