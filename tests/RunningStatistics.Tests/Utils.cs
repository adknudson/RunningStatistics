using System;

namespace RunningStatistics.Tests
{
    internal static class Utils
    {
        internal static double RelError(double expected, double observed)
        {
            return (observed - expected) / expected;
        }
    }
}
