using System;
using Xunit;

namespace RunningStatistics.Tests.Sum;

public partial class TestSumGeneric() 
    : AbstractRunningStatsTest<double, Sum<double>>(
        () => Random.Shared.NextDouble(), 
        () => new Sum<double>())
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var sum = new Sum<double>();
        sum.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => sum.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, sum.Nobs);
    }
}