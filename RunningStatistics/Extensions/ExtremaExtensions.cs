using System;
using System.Numerics;

namespace RunningStatistics;

public static class ExtremaExtensions
{
    public static double Range(this Extrema extrema)
    {
        return extrema.Max - extrema.Min;
    }
    
    public static T Range<T>(this Extrema<T> extrema) 
        where T : IMinMaxValue<T>, IComparable<T>, ISubtractionOperators<T, T, T>
    {
        return extrema.Max - extrema.Min;
    }
}