using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests;

public class TestCountMap
{
    [Fact]
    public void Initialize_WithDictionary_SetsCountsCorrectly()
    {
        var initialData = new Dictionary<int, long>
        {
            { 1, 2 },
            { 2, 3 },
            { 3, 1 }
        };

        var countMap = new CountMap<int>(initialData);

        Assert.Equal(2, countMap[1]);
        Assert.Equal(3, countMap[2]);
        Assert.Equal(1, countMap[3]);
    }
    
    [Fact]
    public void Initialize_WithDictionaryContainingNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        var initialData = new Dictionary<int, long>
        {
            { 1, -2 },
            { 2, 3 },
            { 3, 1 }
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => new CountMap<int>(initialData));
    }
    
    [Fact]
    public void Initialize_WithDictionaryContainingZeroCount_DoesNotAddObservation()
    {
        var initialData = new Dictionary<int, long>
        {
            { 1, 0 },
            { 2, 3 },
            { 3, 1 }
        };

        var countMap = new CountMap<int>(initialData);

        Assert.False(countMap.ContainsKey(1));
        Assert.Equal(3, countMap[2]);
        Assert.Equal(1, countMap[3]);
        Assert.Equal(2, countMap.Count);
    }
    
    [Fact]
    public void AddObservation_IncreasesCount()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1);
        Assert.Equal(1, countMap[1]);
    }

    [Fact]
    public void AddObservation_MultipleTimes_IncreasesCountCorrectly()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1);
        countMap.Fit(1);
        Assert.Equal(2, countMap[1]);
    }

    [Fact]
    public void GetCount_ForNonExistingObservation_ReturnsZero()
    {
        var countMap = new CountMap<int>();
        Assert.Equal(0, countMap[2]);
    }

    [Fact]
    public void AddObservation_DifferentObservations_TracksEachCorrectly()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1);
        countMap.Fit(2);
        Assert.Equal(1, countMap[1]);
        Assert.Equal(1, countMap[2]);
    }

    [Fact]
    public void AddObservation_Zero_IncreasesCount()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(0);
        Assert.Equal(1, countMap[0]);
    }

    [Fact]
    public void AddObservation_NegativeObservation_IncreasesCount()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(-1);
        Assert.Equal(1, countMap[-1]);
    }
    
    [Fact]
    public void Fit_WithCountGreaterThanOne_IncreasesCountCorrectly()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 3);
        Assert.Equal(3, countMap[1]);
    }

    [Fact]
    public void Fit_WithCountZero_DoesNotChangeCount()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 0);
        Assert.Equal(0, countMap[1]);
    }
    
    [Fact]
    public void Fit_WithCountZero_DoesNotAddObservation()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 0);
        Assert.Empty(countMap);
    }

    [Fact]
    public void Fit_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        var countMap = new CountMap<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => countMap.Fit(1, -1));
    }

    [Fact]
    public void Merge_TwoCountMaps_CombinesCountsCorrectly()
    {
        var countMap1 = new CountMap<int>();
        countMap1.Fit(1, 2);
        countMap1.Fit(2, 3);

        var countMap2 = new CountMap<int>();
        countMap2.Fit(1, 1);
        countMap2.Fit(3, 4);

        countMap1.Merge(countMap2);

        Assert.Equal(3, countMap1[1]);
        Assert.Equal(3, countMap1[2]);
        Assert.Equal(4, countMap1[3]);
    }

    [Fact]
    public void Reset_ClearsAllObservationsAndCounts()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);

        countMap.Reset();

        Assert.Equal(0, countMap[1]);
        Assert.Equal(0, countMap[2]);
        Assert.Empty(countMap);
    }
    
    [Fact]
    public void Clone_CreatesExactCopy()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);

        var clone = countMap.Clone();

        Assert.Equal(countMap[1], clone[1]);
        Assert.Equal(countMap[2], clone[2]);
        Assert.Equal(countMap.Count, clone.Count);
        Assert.Equal(countMap.Nobs, clone.Nobs);
    }

    [Fact]
    public void Clone_ModifyingCloneDoesNotAffectOriginal()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);

        var clone = countMap.Clone();
        clone.Fit(1, 1);
        clone.Fit(3, 4);

        // Original should remain unchanged
        Assert.Equal(2, countMap[1]);
        Assert.Equal(3, countMap[2]);
        Assert.Equal(0, countMap[3]);
        Assert.Equal(2, countMap.Count);

        // Clone should reflect changes
        Assert.Equal(3, clone[1]);
        Assert.Equal(3, clone[2]);
        Assert.Equal(4, clone[3]);
        Assert.Equal(3, clone.Count);
    }

    [Fact]
    public void Mode_ReturnsObservationWithHighestCount()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(2, countMap.Mode());
    }

    [Fact]
    public void Median_ReturnsCorrectMedianObservation()
    {
        // odd number of observations
        var countMap = new CountMap<int>();
        countMap.Fit(1, 1);
        countMap.Fit(2, 1);
        countMap.Fit(3, 1);

        Assert.Equal(2, countMap.Median());
        
        // even number of observations returns first observation where PDF >= 0.5
        countMap.Fit(4, 1);
        Assert.Equal(2, countMap.Median());
    }
}