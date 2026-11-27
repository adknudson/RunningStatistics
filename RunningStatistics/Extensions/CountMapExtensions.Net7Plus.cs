#if NET7_0_OR_GREATER

using System;
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

    /// <summary>
    /// Compute the sample variance of a CountMap of any generic type that supports subtraction,
    /// multiplication, and division by a <see cref="long"/>. The corrected form needs at least
    /// two observations, the uncorrected form at least one; below the limit the method throws,
    /// the way the decimal Variance does, because a generic type has no NaN to hand back.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// Thrown when the count map has too few observations for the chosen form.
    /// </exception>
    public static T Variance<T>(this CountMap<T> countMap, T mean, bool corrected = true)
        where T :
        IAdditionOperators<T, T, T>,
        IAdditiveIdentity<T, T>,
        ISubtractionOperators<T, T, T>,
        IMultiplyOperators<T, T, T>,
        IMultiplyOperators<T, long, T>,
        IDivisionOperators<T, long, T>
    {
        if (countMap.Nobs < (corrected ? 2 : 1))
        {
            throw new DivideByZeroException("Variance is undefined: the corrected form needs at least two observations, the uncorrected form at least one.");
        }

        var total = T.AdditiveIdentity;

        foreach (var (x, k) in countMap)
        {
            var d = x - mean;
            total += d * d * k;
        }

        // Folding the Bessel factor into the denominator keeps the divide in the T
        // domain: total / Nobs * Nobs / (Nobs - 1) is exactly total / (Nobs - 1).
        return corrected ? total / (countMap.Nobs - 1) : total / countMap.Nobs;
    }

    /// <summary>
    /// Compute the sample variance of a CountMap of any generic type that supports the
    /// arithmetic above and the mean's division by a <see cref="long"/>.
    /// </summary>
    public static T Variance<T>(this CountMap<T> countMap, bool corrected = true)
        where T :
        IAdditionOperators<T, T, T>,
        IAdditiveIdentity<T, T>,
        ISubtractionOperators<T, T, T>,
        IMultiplyOperators<T, T, T>,
        IMultiplyOperators<T, long, T>,
        IDivisionOperators<T, long, T>
    {
        return countMap.Variance(countMap.Mean(), corrected);
    }
}

#endif
