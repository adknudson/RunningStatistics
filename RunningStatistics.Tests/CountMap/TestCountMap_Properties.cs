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
}