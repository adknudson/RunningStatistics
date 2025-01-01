using System;
using Xunit;

namespace RunningStatistics.Tests.CountMap;

public partial class TestCountMap()
    : AbstractRunningStatsTest<int, CountMap<int>>(
        () => new CountMap<int>(), 
        () => Random.Shared.Next(0, 100))
{
    [Fact]
    public void Fit_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        var countMap = new CountMap<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => countMap.Fit(1, -1));
    }
}