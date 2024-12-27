using System.Numerics;

namespace RunningStatistics;

public static class ExtremaExtensions
{
#if NET7_0_OR_GREATER
    
    public static T Range<T>(this Extrema<T> extrema) 
        where T : IMinMaxValue<T>, IComparisonOperators<T, T, bool>, ISubtractionOperators<T, T, T>
    {
        return extrema.Max - extrema.Min;
    }
    
#endif
}