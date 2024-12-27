﻿using System;
using Xunit;

namespace RunningStatistics.Tests;

public class TestExtrema
{
    [Fact]
    public void ConstructorInitializesProperties()
    {
        Extrema extrema = new();

        Assert.Equal(double.PositiveInfinity, extrema.Min);
        Assert.Equal(double.NegativeInfinity, extrema.Max);
        Assert.Equal(0, extrema.MinCount);
        Assert.Equal(0, extrema.MaxCount);
        Assert.Equal(0, extrema.Nobs);
    }
    
    [Fact]
    public void FittingSingleValue()
    {
        Extrema extrema = new();

        extrema.Fit(10.0);
        Assert.Equal(10.0, extrema.Min);
        Assert.Equal(10.0, extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(1, extrema.Nobs);
    }
    
    [Fact]
    public void FitUpdatesMinAndMaxCorrectly()
    {
        Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);

        Assert.Equal(3.0, extrema.Min);
        Assert.Equal(7.0, extrema.Max);
    }

    [Fact]
    public void FitUpdatesMinCountAndMaxCountCorrectly()
    {
        Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);
        extrema.Fit(7.0);
        extrema.Fit(7.0);

        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
    }

    [Fact]
    public void FitUpdatesNobsCorrectly()
    {
        Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);

        Assert.Equal(3, extrema.Nobs);
    }
    
    [Fact]
    public void FitHandlesNaNValues()
    {
        Extrema extrema = new();
        Assert.Throws<ArgumentException>(() => extrema.Fit(double.NaN));
    }

    [Fact]
    public void FitHandlesExtremeValues()
    {
        Extrema extrema = new();
        extrema.Fit(double.MinValue);
        extrema.Fit(double.MaxValue);
        extrema.Fit(double.MinValue);

        Assert.Equal(double.MinValue, extrema.Min);
        Assert.Equal(double.MaxValue, extrema.Max);
        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(3, extrema.Nobs);
    }
    
    [Fact]
    public void MergeCombinesMinAndMaxCorrectly()
    {
        Extrema a = new();
        a.Fit(1.0);
        a.Fit(2.0);

        Extrema b = new();
        b.Fit(0.5);
        b.Fit(3.0);

        a.Merge(b);

        Assert.Equal(0.5, a.Min);
        Assert.Equal(3.0, a.Max);
    }

    [Fact]
    public void MergeCombinesMinCountAndMaxCountCorrectly()
    {
        Extrema a = new();
        a.Fit(1.0);
        a.Fit(1.0);
        a.Fit(2.0);

        Extrema b = new();
        b.Fit(1.0);
        b.Fit(3.0);
        b.Fit(3.0);

        a.Merge(b);

        Assert.Equal(3, a.MinCount);
        Assert.Equal(2, a.MaxCount);
    }

    [Fact]
    public void MergeCombinesNobsCorrectly()
    {
        Extrema a = new();
        a.Fit(1.0);
        a.Fit(2.0);

        Extrema b = new();
        b.Fit(0.5);
        b.Fit(3.0);

        a.Merge(b);

        Assert.Equal(4, a.Nobs);
    }

    [Fact]
    public void MergeEmptyAreInf()
    {
        Extrema a = new();
        Extrema b = new(); 
        a.Merge(b);

        Assert.Equal(double.PositiveInfinity, a.Min);
        Assert.Equal(double.NegativeInfinity, a.Max);
        Assert.Equal(0, a.MinCount);
        Assert.Equal(0, a.MaxCount);
        Assert.Equal(0, a.Nobs);
    }


    [Fact]
    public void MergePartsEqualsMergeAll()
    {
        const int n = 2000;
        var rng = new Random();

        Extrema a = new(); Extrema b = new(); Extrema c = new();

        double v;
        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            a.Fit(v);
            c.Fit(v);
        }

        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            b.Fit(v);
            c.Fit(v);
        }

        a.Merge(b);

        Assert.Equal(a.Min, c.Min);
        Assert.Equal(a.MinCount, c.MinCount);
        Assert.Equal(a.Max, c.Max);
        Assert.Equal(a.MaxCount, c.MaxCount);
    }

    [Fact]
    public void StaticMergeDoesNotAffectOriginals()
    {
        Extrema a = new();
        a.Fit([1, 1, 2, 3, 5, 8, 13, 21, 34, 55]);

        Extrema b = new();
        b.Fit([-1, -1, -1, 34]);

        var c = Extrema.Merge(a, b);
            
        Assert.Equal(1, a.Min);
        Assert.Equal(55, a.Max);
            
        Assert.Equal(-1, b.Min);
        Assert.Equal(34, b.Max);
            
        Assert.Equal(-1, c.Min);
        Assert.Equal(55, c.Max);
    }
    
    [Fact]
    public void ResetMethodResetsProperties()
    {
        Extrema extrema = new();
        extrema.Fit(1.0);
        extrema.Fit(2.0);
        extrema.Fit(3.0);

        extrema.Reset();

        Assert.Equal(double.PositiveInfinity, extrema.Min);
        Assert.Equal(double.NegativeInfinity, extrema.Max);
        Assert.Equal(0, extrema.MinCount);
        Assert.Equal(0, extrema.MaxCount);
        Assert.Equal(0, extrema.Nobs);
    }
}