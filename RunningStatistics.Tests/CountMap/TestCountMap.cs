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

    // A per-key count can never overflow quietly on its own. The code checks the total count
    // first when fitting, and every per-key count is bounded by that total. A rejected fit
    // therefore cannot leave one key's count wrapped while Nobs still looks fine. These
    // facts pin that behavior at the boundary itself, and just past it.
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

        // the per-key counter did not wrap: it still equals the total observation count
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

        // A merge that would cross the limit throws while the code checks the total count. That
        // check runs before the merge touches any per-key count, so the receiving map is left
        // exactly as it was.
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
    public void TryMinKey_FalseWhenEmptyAndMatchesMinKeyWhenFilled()
    {
        var empty = new CountMap<int>();
        Assert.False(empty.TryGetMinKey(out var emptyKey));
        Assert.Equal(0, emptyKey);

        var countMap = new CountMap<int>();
        countMap.Fit(5, 1);
        countMap.Fit(2, 1);
        Assert.True(countMap.TryGetMinKey(out var min));
        Assert.Equal(2, min);
        Assert.Equal(countMap.MinKey(), min);
    }

    [Fact]
    public void TryMaxKey_FalseWhenEmptyAndMatchesMaxKeyWhenFilled()
    {
        var empty = new CountMap<int>();
        Assert.False(empty.TryGetMaxKey(out var emptyKey));
        Assert.Equal(0, emptyKey);

        var countMap = new CountMap<int>();
        countMap.Fit(5, 1);
        countMap.Fit(2, 1);
        Assert.True(countMap.TryGetMaxKey(out var max));
        Assert.Equal(5, max);
        Assert.Equal(countMap.MaxKey(), max);
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
    
    // This fact pins the Merge contract and the smallest and largest key selection. Merging
    // another map feeds that map's entries back through the pair fitting, one key and count
    // at a time. Every key that appears on either side therefore ends up with the sum of its
    // counts from both sides. The total count is the sum of all counts.
    [Fact]
    public void Merge_DisjointKeys()
    {
        var a = new CountMap<int>();
        a.Fit(1, 2);
        a.Fit(3, 5);
        
        var b = new CountMap<int>();
        b.Fit(7, 4);
        
        a.Merge(b);
        
        // the merged map holds exactly the key 1 with count 2, the key 3 with count 5 and the
        // key 7 with count 4
        Assert.Equal(3, a.Count);
        Assert.Equal(2, a[1]);
        Assert.Equal(5, a[3]);
        Assert.Equal(4, a[7]);
        Assert.Equal(11, a.Nobs);
        
        // other's state is untouched after the merge
        Assert.Single(b);
        Assert.Equal(4, b.Nobs);
        Assert.Equal(4, b[7]);
        Assert.False(b.ContainsKey(1));
        Assert.False(b.ContainsKey(3));
        
        // merging into a fresh copy carries exactly the source's single key 7 with count 4, and
        // leaves the source untouched
        var fresh = new CountMap<int>();
        fresh.Merge(b);
        Assert.Single(fresh);
        Assert.Equal(4, fresh[7]);
        Assert.Equal(4, fresh.Nobs);
        Assert.False(fresh.ContainsKey(1));
        Assert.False(fresh.ContainsKey(3));
        Assert.Single(b);
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
        
        // The key both maps share ends up with the sum of its two counts, 4 + 5. The merged
        // map holds exactly these three keys with these counts. The total count is the sum of
        // all four fitted counts.
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
        
        // Both directions give the identical map. Key 1 holds count 2, key 3 holds count 9
        // and key 9 holds count 1. The combined total is 12 observations.
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
    
    // The minimum-key and maximum-key getters scan every key with the comparer. They never look
    // at counts, so the winner is chosen by key order, not by how often a key occurs. Negative
    // integer keys order like plain integers, and equal counts have no effect on the outcome.
    [Fact]
    public void MinKey_MaxKey_NegativeAndTied()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(0, 3);
        countMap.Fit(7, 3);
        countMap.Fit(-5, 3);
        
        // with every count tied, the minimum and maximum keys follow key order alone
        Assert.Equal(-5, countMap.MinKey());
        Assert.Equal(7, countMap.MaxKey());
        
        // counts do not sway the outcome: the same keys with skewed counts give the same minimum
        // and maximum keys
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
        Assert.Empty(clone);
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
        
        // and the other direction: fitting and merging on the original never touches the clone
        stat.Fit(9, 4);
        Assert.Equal(4, stat[9]);
        Assert.Equal(11, stat.Nobs);
        Assert.False(clone.ContainsKey(9));
        Assert.Equal(107, clone.Nobs);
    }

    // These facts check the CloneEmpty promise for statistics that carry configuration. The
    // clone keeps the same configuration but starts with no observations. For a count map
    // built with a custom comparer, that comparer is the ordering configuration. A clone that
    // dropped it would quietly fall back to the default ordering. That fallback would change
    // what the minimum-key and maximum-key readouts mean on the clone.
    [Fact]
    public void CloneEmpty_PreservesComparerOrdering()
    {
        var comparer = Comparer<int>.Create((x, y) => Comparer<int>.Default.Compare(-x, -y));
        var stat = new CountMap<int>(comparer);
        stat.Fit(1, 2);
        stat.Fit(3, 5);

        var clone = stat.CloneEmpty();

        // the clone carries the same comparer object it was built with, not a default one
        Assert.Same(comparer, clone.Comparer);
        Assert.Empty(clone);
        Assert.Equal(0, clone.Nobs);

        // The ordering on the clone follows the comparer. Only the key 2 is present, so the
        // minimum and the maximum coincide. After the merge they swap, so the minimum key is
        // the largest integer and the maximum key is the smallest.
        clone.Fit(2, 4);
        Assert.Equal(2, clone.MinKey());
        Assert.Equal(2, clone.MaxKey());

        // and it survives the clone-then-merge accumulation as well
        clone.Merge(stat);
        Assert.Equal(3, clone.MinKey());
        Assert.Equal(1, clone.MaxKey());
        Assert.Equal(11, clone.Nobs);
        Assert.Equal(3, clone.Count);
        Assert.Equal(7, stat.Nobs);
    }

    // This fact merges a map into itself. The merge walks the entries and feeds each one back
    // into the same dictionary, reading an entry's count at the moment it visits it. The
    // result doubles every key's count exactly once, and the total doubles with it. Nothing
    // else changes. No new keys appear, nothing throws, and no key is skipped.
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

    // Null-collection rejection (both collection overloads) lives in
    // AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}