using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.CountMap;

public partial class TestCountMap()
    : AbstractRunningStatsTest<int, CountMap<int>>(
        () => Random.Shared.Next(0, 100),
        () => new CountMap<int>())
{
    [Fact]
    public void MinKey_ThrowsIfEmpty()
    {
        var countMap = new CountMap<int>();
        Assert.Throws<KeyNotFoundException>(() => countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ThrowsIfEmpty()
    {
        var countMap = new CountMap<int>();
        Assert.Throws<KeyNotFoundException>(() => countMap.MaxKey());
    }
    
    [Fact]
    public void MinKey_ThrowsIfObservationIsNotComparable()
    {
        var countMap = new CountMap<object>();
        countMap.Fit(new object(), 1);
        countMap.Fit(new object(), 2);
        Assert.Throws<ArgumentException>(() => countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ThrowsIfObservationIsNotComparable()
    {
        var countMap = new CountMap<object>();
        countMap.Fit(new object(), 1);
        countMap.Fit(new object(), 2);
        Assert.Throws<ArgumentException>(() => countMap.MaxKey());
    }
    
    [Fact]
    public void MinKey_ReturnsMinimumKeyForNumerics()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(1, countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ReturnsMaximumKeyForNumerics()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(3, countMap.MaxKey());
    }
    
    [Fact]
    public void MinKey_ReturnsMinimumKeyForStrings()
    {
        var countMap = new CountMap<string>();
        countMap.Fit("a", 2);
        countMap.Fit("b", 3);
        countMap.Fit("c", 1);

        Assert.Equal("a", countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ReturnsMaximumKeyForStrings()
    {
        var countMap = new CountMap<string>();
        countMap.Fit("a", 2);
        countMap.Fit("b", 3);
        countMap.Fit("c", 1);

        Assert.Equal("c", countMap.MaxKey());
    }
    
    [Fact]
    public void MinKey_UsesCustomComparer()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        Assert.Equal(3, countMap.MinKey(comparer));
    }

    [Fact]
    public void MaxKey_UsesCustomComparer()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        Assert.Equal(1, countMap.MaxKey(comparer));
    }
    
    [Fact]
    public void CountMap_WithCustomComparer_MinKey_ReturnsCorrectValue()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var countMap = new CountMap<int>(comparer);
        
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        
        Assert.Equal(3, countMap.MinKey());
    }
    
    [Fact]
    public void CountMap_WithCustomComparer_MaxKey_ReturnsCorrectValue()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var countMap = new CountMap<int>(comparer);
        
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        
        Assert.Equal(1, countMap.MaxKey());
    }
    
    [Fact]
    public void CountMap_MinKey_NullComparer_UsesDefaultComparer()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var countMap = new CountMap<int>(comparer);
        
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        
        Assert.Equal(1, countMap.MinKey(null));
    }

    [Fact]
    public void CountMap_MaxKey_NullComparer_UsesDefaultComparer()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var countMap = new CountMap<int>(comparer);
        
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        
        Assert.Equal(3, countMap.MaxKey(null));
    }
}