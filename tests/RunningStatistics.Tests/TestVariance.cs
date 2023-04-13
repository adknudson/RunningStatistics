using System;
using Xunit;

namespace RunningStatistics.Tests;

public class TestVariance
{
    [Fact]
    public void EmptyVarianceReturnsOne()
    {
        Variance v = new();
        Assert.Equal(0, v.Nobs);
        Assert.Equal(1, v.Value);
    }

    [Fact]
    public void SingleObservation()
    {
        Variance v = new();
        v.Fit(10);
        Assert.Equal(1, v.Value, 2);
        v.Reset();
        v.Fit(double.PositiveInfinity);
        Assert.Equal(double.NaN, v.Value);
    }

    [Fact]
    public void MergingVariances()
    {
        Variance a = new(), b = new();
        a.Fit(10);
        b.Fit(20);
        var c = Variance.Merge(a, b);
        a.Merge(b);
        
        Assert.Equal(2, a.Nobs);
        Assert.Equal(1, b.Nobs);
        Assert.Equal(2, c.Nobs);
        
        Assert.Equal(a.Value, c.Value, 2);
    }

    [Fact]
    public void ResetVariance()
    {
        Variance a = new();
        a.Fit(1);
        a.Fit(2);
        a.Fit(3);
        
        a.Reset();
        
        Assert.Equal(1, a.Value);
        Assert.Equal(0, a.Nobs);
    }
}