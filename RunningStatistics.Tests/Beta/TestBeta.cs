using System;
using MathNet.Numerics.Random;
using Xunit;

namespace RunningStatistics.Tests.Beta;

public partial class TestBeta()
    : AbstractRunningStatsTest<bool, RunningStatistics.Beta>(
        () => Random.Shared.NextBoolean(), 
        () => new RunningStatistics.Beta())
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var beta = new RunningStatistics.Beta();
        beta.Fit(true, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => beta.Fit(true, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, beta.Nobs);
    }

    [Fact]
    public void ParameterlessConstructor_InitializesParametersCorrectly()
    {
        RunningStatistics.Beta beta = new();
        Assert.Equal(0, beta.Successes);
        Assert.Equal(0, beta.Failures);
        Assert.Equal(0, beta.Nobs);
    }
    
    [Fact]
    public void Constructor_InitializesParametersCorrectly()
    {
        RunningStatistics.Beta beta = new(5, 10);
        Assert.Equal(5, beta.Successes);
        Assert.Equal(10, beta.Failures);
        Assert.Equal(15, beta.Nobs);
    }
    
    [Fact]
    public void Constructor_GuardsAgainstNegativeParameters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.Beta(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.Beta(10, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.Beta(-1, -1));
    }
    
    [Fact]
    public void Fit_GuardsAgainstNegativeCount()
    {
        RunningStatistics.Beta beta = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(true, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(false, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(-1, 1));
    }
    
    [Fact]
    public void Getters_AccurateAt3E9()
    {
        // Heavy-tail scenario: 3e9 successes, 1 failure. Mean and Variance are
        // double-accurate (the getter bug is the long product in Variance, and
        // a*b fits at this scale), pinned here so the fix must not change them.
        RunningStatistics.Beta beta = new();
        beta.Fit(true, 3_000_000_000L);
        beta.Fit(false, 1L);
        var a = 3_000_000_000.0;
        var b = 1.0;
        Utils.AssertIsApproximate(a / (a + b), beta.Mean, 0);
        Utils.AssertIsApproximate(a * b / (Math.Pow(a + b, 2) * (a + b + 1)), beta.Variance, 0);
        Assert.Equal(3_000_000_000L, beta.Successes);
        Assert.Equal(1L, beta.Failures);
        
        // Overflow scenario: the Variance getter's `_a * _b` is a long product, and
        // 3_100_000_000 * 3_100_000_000 = 9.61e18 > long.MaxValue wraps to
        // -8_836_744_073_709_551_616 -> Variance comes out negative (about -3.708e-11),
        // while the double-accurate value is about 4.032e-11. The sums (6.2e9) never
        // trip the checked Fit accumulation, so this is reachable through the public API.
        RunningStatistics.Beta overflow = new();
        overflow.Fit(true, 3_100_000_000L);
        overflow.Fit(false, 3_100_000_000L);
        var c = 3_100_000_000.0;
        Utils.AssertIsApproximate(c / (c + c), overflow.Mean, 0);
        Utils.AssertIsApproximate(c * c / (Math.Pow(c + c, 2) * (c + c + 1)), overflow.Variance, 0);
        Assert.Equal(3_100_000_000L, overflow.Successes);
        Assert.Equal(3_100_000_000L, overflow.Failures);
    }
    
    [Fact]
    public void Getters_ExactAtSmallCounts()
    {
        // Pin the class's documented convention at small counts against a double
        // reference: Mean = a/(a+b); Variance = a*b / ((a+b)^2 * (a+b+1)).
        RunningStatistics.Beta beta = new(7, 3);
        Assert.Equal(0.7, beta.Mean);
        Utils.AssertIsApproximate(21.0 / (Math.Pow(10.0, 2) * 11.0), beta.Variance, 0);
        Utils.AssertIsApproximate(7.0 * 3.0 / (Math.Pow(7.0 + 3.0, 2) * (7.0 + 3.0 + 1)), beta.Variance, 0);
    }
}