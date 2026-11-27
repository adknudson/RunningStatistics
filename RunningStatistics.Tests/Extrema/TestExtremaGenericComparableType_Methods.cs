using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtremaGenericComparableType
{
    [Fact]
    public void Fit_RejectsNullObservations()
    {
        Extrema<MyComparable> extrema = new();
        var nullValue = (MyComparable)null;
        Assert.Throws<ArgumentNullException>(() => extrema.Fit(nullValue));
        Assert.Throws<ArgumentNullException>(() => extrema.Fit(nullValue, 3));
        Assert.Equal(0, extrema.Nobs);
    }

    [Fact]
    public void FitUpdates_MinAndMaxCorrectlyForMyComparable()
    {
        Extrema<MyComparable> extrema = new();
        extrema.Fit(new MyComparable(5.0m));
        extrema.Fit(new MyComparable(3.0m));
        extrema.Fit(new MyComparable(7.0m));

        Assert.Equal(new MyComparable(3.0m), extrema.Min);
        Assert.Equal(new MyComparable(7.0m), extrema.Max);
    }

    [Fact]
    public void FitUpdates_MinCountAndMaxCountCorrectlyForMyComparable()
    {
        Extrema<MyComparable> extrema = new();
        extrema.Fit(new MyComparable(5.0m));
        extrema.Fit(new MyComparable(3.0m));
        extrema.Fit(new MyComparable(3.0m));
        extrema.Fit(new MyComparable(7.0m));
        extrema.Fit(new MyComparable(7.0m));
        extrema.Fit(new MyComparable(7.0m));

        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
    }

    [Fact]
    public void FitUpdates_NobsCorrectlyForMyComparable()
    {
        Extrema<MyComparable> extrema = new();
        extrema.Fit(new MyComparable(5.0m));
        extrema.Fit(new MyComparable(3.0m));
        extrema.Fit(new MyComparable(7.0m));

        Assert.Equal(3, extrema.Nobs);
    }
        
    [Fact]
    public void Fit_ExtremeValuesForMyComparable()
    {
        Extrema<MyComparable> extrema = new();
        extrema.Fit(new MyComparable(decimal.MaxValue));
        extrema.Fit(new MyComparable(decimal.MinValue));

        Assert.Equal(new MyComparable(decimal.MinValue), extrema.Min);
        Assert.Equal(new MyComparable(decimal.MaxValue), extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(2, extrema.Nobs);
    }

    [Fact]
    public void MergeEmpty_WithEmptyOther_PropertiesAreCorrect()
    {
        Extrema<MyComparable> a = new();
        Extrema<MyComparable> b = new();
        
        // pre-conditions
        Assert.Equal(0, a.Nobs);
        Assert.Throws<InvalidOperationException>(() => a.Min);
        Assert.Throws<InvalidOperationException>(() => a.Max);
        Assert.Equal(0, a.MinCount);
        Assert.Equal(0, a.MaxCount);
        
        a.Merge(b);
        
        // post-conditions
        Assert.Equal(0, a.Nobs);
        Assert.Throws<InvalidOperationException>(() => a.Min);
        Assert.Throws<InvalidOperationException>(() => a.Max);
        Assert.Equal(0, a.MinCount);
        Assert.Equal(0, a.MaxCount);
    }
    
    [Fact]
    public void MergeEmpty_WithNonEmptyOther_PropertiesAreCorrect()
    {
        Extrema<MyComparable> a = new();
        Extrema<MyComparable> b = new();
        b.Fit(new MyComparable(1.0m));
        b.Fit(new MyComparable(2.0m));
        
        // pre-conditions
        Assert.Equal(0, a.Nobs);
        Assert.Throws<InvalidOperationException>(() => a.Min);
        Assert.Throws<InvalidOperationException>(() => a.Max);
        Assert.Equal(0, a.MinCount);
        Assert.Equal(0, a.MaxCount);

        Assert.Equal(2, b.Nobs);
        Assert.Equal(new MyComparable(1.0m), b.Min);
        Assert.Equal(new MyComparable(2.0m), b.Max);
        Assert.Equal(1, b.MinCount);
        Assert.Equal(1, b.MaxCount);
        
        a.Merge(b);
        
        // post-conditions
        Assert.Equal(2, a.Nobs);
        Assert.Equal(new MyComparable(1.0m), a.Min);
        Assert.Equal(new MyComparable(2.0m), a.Max);
        Assert.Equal(1, a.MinCount);
        Assert.Equal(1, a.MaxCount);
    }
    
    [Fact]
    public void MergeNonEmpty_WithEmptyOther_PropertiesAreCorrect()
    {
        Extrema<MyComparable> a = new();
        a.Fit(new MyComparable(1.0m));
        a.Fit(new MyComparable(2.0m));
        Extrema<MyComparable> b = new();
        
        // pre-conditions
        Assert.Equal(2, a.Nobs);
        Assert.Equal(new MyComparable(1.0m), a.Min);
        Assert.Equal(new MyComparable(2.0m), a.Max);
        Assert.Equal(1, a.MinCount);
        Assert.Equal(1, a.MaxCount);
        
        Assert.Equal(0, b.Nobs);
        Assert.Throws<InvalidOperationException>(() => b.Min);
        Assert.Throws<InvalidOperationException>(() => b.Max);
        Assert.Equal(0, b.MinCount);
        Assert.Equal(0, b.MaxCount);
        
        a.Merge(b);
        
        // post-conditions
        Assert.Equal(2, a.Nobs);
        Assert.Equal(new MyComparable(1.0m), a.Min);
        Assert.Equal(new MyComparable(2.0m), a.Max);
        Assert.Equal(1, a.MinCount);
        Assert.Equal(1, a.MaxCount);
    }

    [Fact]
    public void Merge_CombinesMinAndMaxCorrectlyForMyComparable()
    {
        Extrema<MyComparable> a = new();
        a.Fit(new MyComparable(1.0m));
        a.Fit(new MyComparable(2.0m));

        Extrema<MyComparable> b = new();
        b.Fit(new MyComparable(0.5m));
        b.Fit(new MyComparable(3.0m));

        a.Merge(b);

        Assert.Equal(new MyComparable(0.5m), a.Min);
        Assert.Equal(new MyComparable(3.0m), a.Max);
    }

    [Fact]
    public void Merge_CombinesMinCountAndMaxCountCorrectlyForMyComparable()
    {
        Extrema<MyComparable> a = new();
        a.Fit(new MyComparable(1.0m));
        a.Fit(new MyComparable(1.0m));
        a.Fit(new MyComparable(2.0m));

        Extrema<MyComparable> b = new();
        b.Fit(new MyComparable(1.0m));
        b.Fit(new MyComparable(3.0m));
        b.Fit(new MyComparable(3.0m));

        a.Merge(b);

        Assert.Equal(3, a.MinCount);
        Assert.Equal(2, a.MaxCount);
    }

    [Fact]
    public void Merge_CombinesNobsCorrectlyForMyComparable()
    {
        Extrema<MyComparable> a = new();
        a.Fit(new MyComparable(1.0m));
        a.Fit(new MyComparable(2.0m));

        Extrema<MyComparable> b = new();
        b.Fit(new MyComparable(0.5m));
        b.Fit(new MyComparable(3.0m));

        a.Merge(b);

        Assert.Equal(4, a.Nobs);
    }

    [Fact]
    public void TryMinAndTryMax_ResolveTheThrowingAccessorsForMyComparable()
    {
        Extrema<MyComparable> extrema = new();

        // Empty readouts: Min and Max throw, so the Try reads are the non-throwing way to
        // ask. They report false and the out reference stays at the type default.
        Assert.False(extrema.TryMin(out var _));
        Assert.False(extrema.TryMax(out var _));

        extrema.Fit(new MyComparable(3.0m));
        extrema.Fit(new MyComparable(7.0m));

        Assert.True(extrema.TryMin(out var min));
        Assert.True(extrema.TryMax(out var max));
        Assert.Equal(new MyComparable(3.0m), min);
        Assert.Equal(new MyComparable(7.0m), max);
        Assert.Equal(min, extrema.Min);
        Assert.Equal(max, extrema.Max);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}