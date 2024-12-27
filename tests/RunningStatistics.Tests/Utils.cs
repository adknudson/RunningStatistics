using System;
using Xunit;

namespace RunningStatistics.Tests;

internal static class Utils
{
    public static void AssertIsApproximate(double expected, double actual, double absTol)
    {
        var relTol = absTol > 0 ? 0 : 1e-8;
        var tol = Math.Max(absTol, relTol * Math.Max(Math.Abs(expected), Math.Abs(actual)));
        Assert.True(Math.Abs(expected - actual) <= tol, $"Expected: {expected}, Actual: {actual}, Tolerance: {tol}");
    }
    
    public static double RelError(double expected, double observed)
    {
        return (observed - expected) / expected;
    }
}