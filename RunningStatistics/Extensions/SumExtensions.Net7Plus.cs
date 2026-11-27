#if NET7_0_OR_GREATER

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
    /// <remarks>
    /// The empty convention across the numeric overloads: an undefined readout of an
    /// empty statistic answers <see cref="double.NaN"/> where the result type can hold
    /// it, and throws a specific exception where it cannot. <c>decimal</c> has no NaN,
    /// so this overload throws <see cref="InvalidOperationException"/> while the sum
    /// has no observations, the same exception an empty <c>Extrema&lt;T&gt;.Min</c>
    /// throws. It does not throw <c>DivideByZeroException</c>: the method rejects the
    /// empty state before any division, so no division by zero ever runs.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the number of observations is zero.</exception>
    public static decimal Mean(this Sum<decimal> sum)
    {
        if (sum.Nobs > 0)
        {
            return sum.Value / sum.Nobs;
        }
        
        throw new InvalidOperationException("Average is undefined when the number of observations is zero.");
    }
    
    // There is no average for an arbitrary observation type. One would have to ask the type
    // for a division by an observation count, an operator a summing type need not define:
    // a count is not a quantity of the measured thing, so dividing by it says nothing about
    // the type. A caller that knows its own arithmetic writes the average it wants, for
    // example sum.Value / sum.Nobs when the type divides by a long, or
    // (double)sum.Value / sum.Nobs when the answer should be a double.
}

#endif
