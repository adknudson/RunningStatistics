using System.Numerics;

namespace RunningStatistics;

public static class ExtremaExtensions
{
    extension<T>(Extrema<T> extrema) where T : ISubtractionOperators<T, T, T>
    {
        public T Range => extrema.Max - extrema.Min;
    }
}
