#if NET7_0_OR_GREATER

using System;
using System.Numerics;

namespace RunningStatistics;

public static class ExtremaExtensions
{
    public static T Range<T>(this Extrema<T> extrema) 
        where T : IComparable<T>, ISubtractionOperators<T, T, T>
    {
        return extrema.Max - extrema.Min;
    }
}

#endif
