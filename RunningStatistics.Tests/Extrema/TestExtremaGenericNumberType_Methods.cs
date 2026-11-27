using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtremaGenericNumberType
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5), long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => extrema.Fit(new MyNum(5), long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, extrema.Nobs);
    }

    [Fact]
    public void FitUpdates_MinAndMaxCorrectlyForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.Equal(new MyNum(3.0m), extrema.Min);
        Assert.Equal(new MyNum(7.0m), extrema.Max);
    }

    [Fact]
    public void FitUpdates_MinCountAndMaxCountCorrectlyForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));
        extrema.Fit(new MyNum(7.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
    }

    [Fact]
    public void FitUpdates_NobsCorrectlyForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.Equal(3, extrema.Nobs);
    }
        
    [Fact]
    public void Fit_ExtremeValuesForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(decimal.MaxValue));
        extrema.Fit(new MyNum(decimal.MinValue));

        Assert.Equal(new MyNum(decimal.MinValue), extrema.Min);
        Assert.Equal(new MyNum(decimal.MaxValue), extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(2, extrema.Nobs);
    }

    [Fact]
    public void MergeEmpty_WithEmptyOther_PropertiesAreCorrect()
    {
        Extrema<MyNum> a = new();
        Extrema<MyNum> b = new();
        
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
        Extrema<MyNum> a = new();
        Extrema<MyNum> b = new();
        b.Fit(new MyNum(1.0m));
        b.Fit(new MyNum(2.0m));
        
        // pre-conditions
        Assert.Equal(0, a.Nobs);
        Assert.Throws<InvalidOperationException>(() => a.Min);
        Assert.Throws<InvalidOperationException>(() => a.Max);
        Assert.Equal(0, a.MinCount);
        Assert.Equal(0, a.MaxCount);

        Assert.Equal(2, b.Nobs);
        Assert.Equal(new MyNum(1.0m), b.Min);
        Assert.Equal(new MyNum(2.0m), b.Max);
        Assert.Equal(1, b.MinCount);
        Assert.Equal(1, b.MaxCount);
        
        a.Merge(b);
        
        // post-conditions
        Assert.Equal(2, a.Nobs);
        Assert.Equal(new MyNum(1.0m), a.Min);
        Assert.Equal(new MyNum(2.0m), a.Max);
        Assert.Equal(1, a.MinCount);
        Assert.Equal(1, a.MaxCount);
    }
    
    [Fact]
    public void MergeNonEmpty_WithEmptyOther_PropertiesAreCorrect()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));
        Extrema<MyNum> b = new();
        
        // pre-conditions
        Assert.Equal(2, a.Nobs);
        Assert.Equal(new MyNum(1.0m), a.Min);
        Assert.Equal(new MyNum(2.0m), a.Max);
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
        Assert.Equal(new MyNum(1.0m), a.Min);
        Assert.Equal(new MyNum(2.0m), a.Max);
        Assert.Equal(1, a.MinCount);
        Assert.Equal(1, a.MaxCount);
    }

    [Fact]
    public void Merge_CombinesMinAndMaxCorrectlyForMyNum()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));

        Extrema<MyNum> b = new();
        b.Fit(new MyNum(0.5m));
        b.Fit(new MyNum(3.0m));

        a.Merge(b);

        Assert.Equal(new MyNum(0.5m), a.Min);
        Assert.Equal(new MyNum(3.0m), a.Max);
    }

    [Fact]
    public void Merge_CombinesMinCountAndMaxCountCorrectlyForMyNum()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));

        Extrema<MyNum> b = new();
        b.Fit(new MyNum(1.0m));
        b.Fit(new MyNum(3.0m));
        b.Fit(new MyNum(3.0m));

        a.Merge(b);

        Assert.Equal(3, a.MinCount);
        Assert.Equal(2, a.MaxCount);
    }

    [Fact]
    public void Merge_CombinesNobsCorrectlyForMyNum()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));

        Extrema<MyNum> b = new();
        b.Fit(new MyNum(0.5m));
        b.Fit(new MyNum(3.0m));

        a.Merge(b);

        Assert.Equal(4, a.Nobs);
    }
    
    [Fact]
    public void Range_ReturnsCorrectValueForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(1.0m));
        extrema.Fit(new MyNum(3.0m));
        
        var range = extrema.Range();
        
        Assert.Equal(new MyNum(2.0m), range);
    }

    [Fact]
    public void TryMinAndTryMax_ResolveTheThrowingAccessorsForMyNum()
    {
        Extrema<MyNum> extrema = new();

        // Empty readouts: Min and Max throw, so the Try reads are the non-throwing way to
        // ask. They report false and the out reference stays at the type default.
        Assert.False(extrema.TryMin(out var _));
        Assert.False(extrema.TryMax(out var _));

        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.True(extrema.TryMin(out var min));
        Assert.True(extrema.TryMax(out var max));
        Assert.Equal(new MyNum(3.0m), min);
        Assert.Equal(new MyNum(7.0m), max);
        Assert.Equal(min, extrema.Min);
        Assert.Equal(max, extrema.Max);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}