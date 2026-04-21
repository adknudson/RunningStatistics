#if NET7_0_OR_GREATER

using System.Numerics;

namespace RunningStatistics;

public static partial class CountMapExtensions
{
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
}

#endif
