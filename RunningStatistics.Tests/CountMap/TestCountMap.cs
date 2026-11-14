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
    public void Fit_NobsOverflowThrows()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => countMap.Fit(1, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, countMap.Nobs);
    }

    // Pins for the per-key counter vs the checked _nobs: the per-key count is bounded by
    // _nobs and the checked _nobs addition runs FIRST in UncheckedFit, so a per-key count
    // can never wrap independently of Nobs. Pinned at the exact/edge boundaries:
    [Fact]
    public void PerKeyCount_MatchesNobsAtMaxBoundary()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, long.MaxValue);

        Assert.Equal(long.MaxValue, countMap[1]);
        Assert.Equal(long.MaxValue, countMap.Nobs);
    }

    [Fact]
    public void PerKeyCount_ThrowsConsistentlyWhenSameKeyCrossesMax()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => countMap.Fit(1, long.MaxValue / 2 + 1));

        // the per-key counter did not wrap: it still equals _nobs
        Assert.Equal(long.MaxValue / 2 + 1, countMap[1]);
        Assert.Equal(countMap[1], countMap.Nobs);
    }

    [Fact]
    public void PerKeyCount_MergeMatchesNobsNearMaxBoundary()
    {
        var a = new CountMap<int>();
        a.Fit(1, long.MaxValue / 2);
        var b = new CountMap<int>();
        b.Fit(1, long.MaxValue / 2);

        a.Merge(b);

        Assert.Equal(long.MaxValue - 1, a.Nobs);
        Assert.Equal(long.MaxValue - 1, a[1]);

        var c = new CountMap<int>();
        c.Fit(1, 2);
        Assert.Throws<OverflowException>(() => a.Merge(c));

        // crossing throws from _nobs before any per-key update: a is untouched
        Assert.Equal(long.MaxValue - 1, a.Nobs);
        Assert.Equal(long.MaxValue - 1, a[1]);
    }

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