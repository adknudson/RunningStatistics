using System;
using MathNet.Numerics.Random;
using RunningStatistics.Unchecked;
using Xunit;

namespace RunningStatistics.Tests.Beta;

public partial class TestUncheckedBeta() 
    : AbstractRunningStatsTest<bool, UncheckedBeta>(
        () => Random.Shared.NextBoolean(),
        () => new UncheckedBeta())
{
    [Fact]
    public void DefaultConstructor_InitializesParametersCorrectly()
    {
        UncheckedBeta beta = new();
        Assert.Equal(0, beta.Successes);
        Assert.Equal(0, beta.Failures);
        Assert.Equal(0, beta.Nobs);
    }
    
    [Fact]
    public void Constructor_InitializesParametersCorrectly()
    {
        UncheckedBeta beta = new(5, 10);
        Assert.Equal(5, beta.Successes);
        Assert.Equal(10, beta.Failures);
    }
    
    [Fact]
    public void Constructor_DoesNotCheckParameters()
    {
        UncheckedBeta beta = new(-1, 10);
        Assert.Equal(-1, beta.Successes);
        Assert.Equal(10, beta.Failures);
        Assert.Equal(9, beta.Nobs);
        
        beta = new UncheckedBeta(10, -1);
        Assert.Equal(10, beta.Successes);
        Assert.Equal(-1, beta.Failures);
        Assert.Equal(9, beta.Nobs);
        
        beta = new UncheckedBeta(-1, -1);
        Assert.Equal(-1, beta.Successes);
        Assert.Equal(-1, beta.Failures);
        Assert.Equal(-2, beta.Nobs);
    }
}