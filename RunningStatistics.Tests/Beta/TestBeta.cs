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
}