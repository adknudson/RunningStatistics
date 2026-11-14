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
    
    // Commit-22 pins: the Merge contract (per-key sums via the counts accessor) and MinKey/
    // MaxKey selection. Merge(CountMap) delegates to the base Fit(IEnumerable<KeyValuePair<TObs,long>>)
    // which foreaches other's entries and calls Fit(key, count), so every key present in either
    // side ends with the SUM of its counts and Nobs is the sum of all counts.
    [Fact]
    public void Merge_DisjointKeys()
    {
        var a = new CountMap<int>();
        a.Fit(1, 2);
        a.Fit(3, 5);
        
        var b = new CountMap<int>();
        b.Fit(7, 4);
        
        a.Merge(b);
        
        // merged map is exactly {1:2, 3:5, 7:4}
        Assert.Equal(3, a.Count);
        Assert.Equal(2, a[1]);
        Assert.Equal(5, a[3]);
        Assert.Equal(4, a[7]);
        Assert.Equal(11, a.Nobs);
        
        // other's state is untouched after the merge
        Assert.Equal(1, b.Count);
        Assert.Equal(4, b.Nobs);
        Assert.Equal(4, b[7]);
        Assert.False(b.ContainsKey(1));
        Assert.False(b.ContainsKey(3));
        
        // merging into a fresh copy copies exactly b's one key {7:4} and leaves b untouched
        var fresh = new CountMap<int>();
        fresh.Merge(b);
        Assert.Equal(1, fresh.Count);
        Assert.Equal(4, fresh[7]);
        Assert.Equal(4, fresh.Nobs);
        Assert.False(fresh.ContainsKey(1));
        Assert.False(fresh.ContainsKey(3));
        Assert.Equal(1, b.Count);
        Assert.Equal(4, b.Nobs);
    }
    
    [Fact]
    public void Merge_OverlappingKeys_Sums()
    {
        var a = new CountMap<int>();
        a.Fit(1, 2);
        a.Fit(3, 5);
        
        var b = new CountMap<int>();
        b.Fit(3, 4);
        b.Fit(9, 1);
        
        a.Merge(b);
        
        // shared key 3 sums to 4+5, the map is exactly {1:2, 3:9, 9:1}, Nobs 12 = 2+5+4+1
        Assert.Equal(3, a.Count);
        Assert.Equal(2, a[1]);
        Assert.Equal(9, a[3]);
        Assert.Equal(1, a[9]);
        Assert.Equal(12, a.Nobs);
        
        // b is untouched
        Assert.Equal(2, b.Count);
        Assert.Equal(5, b.Nobs);
        Assert.Equal(4, b[3]);
        Assert.Equal(1, b[9]);
    }
    
    [Fact]
    public void Merge_CommutativeCounts_BothDirections()
    {
        var a1 = new CountMap<int>();
        a1.Fit(1, 2);
        a1.Fit(3, 5);
        var b1 = new CountMap<int>();
        b1.Fit(3, 4);
        b1.Fit(9, 1);
        
        var a2 = new CountMap<int>();
        a2.Fit(1, 2);
        a2.Fit(3, 5);
        var b2 = new CountMap<int>();
        b2.Fit(3, 4);
        b2.Fit(9, 1);
        
        a1.Merge(b1);
        b2.Merge(a2);
        
        // both directions give the identical map {1:2, 3:9, 9:1} and Nobs 12
        Assert.Equal(a1.Count, b2.Count);
        Assert.Equal(a1.Nobs, b2.Nobs);
        Assert.Equal(3, a1.Count);
        Assert.Equal(12, a1.Nobs);
        foreach (var key in a1.Keys)
        {
            Assert.Equal(a1[key], b2[key]);
        }
        foreach (var key in b2.Keys)
        {
            Assert.Equal(b2[key], a1[key]);
        }
    }
    
    // MinKey/MaxKey scan every key with the comparer and never consult counts, so selection
    // is by KEY ORDER (not "most common"): negative int keys order as plain ints and ties
    // in count have no effect on min/max.
    [Fact]
    public void MinKey_MaxKey_NegativeAndTied()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(0, 3);
        countMap.Fit(7, 3);
        countMap.Fit(-5, 3);
        
        // all counts tied: min/max follow key order only
        Assert.Equal(-5, countMap.MinKey());
        Assert.Equal(7, countMap.MaxKey());
        
        // counts are irrelevant to key min/max: the same keys with skewed counts agree
        var other = new CountMap<int>();
        other.Fit(0, 1);
        other.Fit(7, 1);
        other.Fit(-5, 1);
        Assert.Equal(countMap.MinKey(), other.MinKey());
        Assert.Equal(countMap.MaxKey(), other.MaxKey());
    }
    
    [Fact]
    public void CloneEmpty_Independence()
    {
        var stat = new CountMap<int>();
        stat.Fit(1, 2);
        stat.Fit(3, 5);
        
        var clone = stat.CloneEmpty();
        Assert.Equal(0, clone.Count);
        Assert.Equal(0, clone.Nobs);
        
        // fitting the clone never touches the original map
        clone.Fit(1, 100);
        Assert.Equal(100, clone[1]);
        Assert.Equal(100, clone.Nobs);
        Assert.Equal(2, stat[1]);
        Assert.Equal(5, stat[3]);
        Assert.Equal(2, stat.Count);
        Assert.Equal(7, stat.Nobs);
        
        // merging the original into the clone gives the union of counts, original still untouched
        clone.Merge(stat);
        Assert.Equal(2, clone.Count);
        Assert.Equal(102, clone[1]);
        Assert.Equal(5, clone[3]);
        Assert.Equal(107, clone.Nobs);
        Assert.Equal(2, stat[1]);
        Assert.Equal(5, stat[3]);
        Assert.Equal(7, stat.Nobs);
        
        // and the other direction: fitting/merging on the original never touches the clone
        stat.Fit(9, 4);
        Assert.Equal(4, stat[9]);
        Assert.Equal(11, stat.Nobs);
        Assert.False(clone.ContainsKey(9));
        Assert.Equal(107, clone.Nobs);
    }
    
    // Merge-into-self observed: the base foreach reads each entry (key, count) at its visit and
    // Fit accumulates into the same dictionary entry, so every key is doubled exactly once,
    // Nobs doubles, and the map stays stable (no new keys, no throw, no skipped key).
    [Fact]
    public void MergeIntoSelf_DoublesCounts()
    {
        var a = new CountMap<int>();
        a.Fit(1, 2);
        a.Fit(3, 5);
        
        a.Merge(a);
        
        Assert.Equal(4, a[1]);
        Assert.Equal(10, a[3]);
        Assert.Equal(2, a.Count);
        Assert.Equal(14, a.Nobs);
    }
}