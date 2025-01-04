#if NET7_0_OR_GREATER
using System;
using System.Numerics;
#endif

namespace RunningStatistics;

public static class ExtremaExtensions
{
    public static double Range(this Extrema extrema)
    {
        return extrema.Max - extrema.Min;
    }

#if NET7_0_OR_GREATER
    
    public static T Range<T>(this Extrema<T> extrema) 
        where T : IMinMaxValue<T>, IComparable<T>, ISubtractionOperators<T, T, T>
    {
        return extrema.Max - extrema.Min;
    }
    
#endif
}