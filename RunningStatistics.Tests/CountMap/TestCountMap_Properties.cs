using Xunit;

namespace RunningStatistics.Tests.CountMap;

public partial class TestCountMap
{
    [Fact]
    public void Accessing_NonExistentKey_UpdatesCountCorrectly()
    {
        var countMap = new CountMap<int>();
        
        countMap.Fit(1, 10);
        countMap.Fit(2, 100);
        countMap.Fit(1, 10);
        countMap.Fit(3, 1000);
        
        Assert.Equal(20, countMap[1]);
        Assert.Equal(100, countMap[2]);
        Assert.Equal(1000, countMap[3]);
    }

    // UniqueObs is the same number as Count, read on the long side like Nobs. A
    // zero-count fit and a read of a key the map never fitted add no unique
    // observation, the promise the class header already carries for Count.
    [Fact]
    public void UniqueObs_MatchesCount()
    {
        var countMap = new CountMap<int>();

        countMap.Fit(1, 10);
        countMap.Fit(2, 100);
        countMap.Fit(1, 0);
        Assert.Equal(0, countMap[7]);

        Assert.Equal(2, countMap.UniqueObs);
        Assert.Equal(countMap.Count, countMap.UniqueObs);
    }
}