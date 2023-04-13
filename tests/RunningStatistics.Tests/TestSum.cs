using System;
using Xunit;

namespace RunningStatistics.Tests;

public class TestSum
{
    [Fact]
    public void EmptySumIsZero()
    {
        Sum a = new();
        Assert.Equal(0, a.Value);
        Assert.Equal(0, a.Nobs);
    }

    [Fact]
    public void MergeSums()
    {
        Sum<int> a = new(), b = new(), c = new();
        var rng = new Random();
        
        for (var i = 0; i < 1000; i++)
        {
            a.Fit(rng.Next());
            b.Fit(rng.Next());
            c.Fit(rng.Next());
        }

        var d = Sum<int>.Merge(a, b, c);

        Assert.Equal(a.Value + b.Value + c.Value, d.Value);
        Assert.Equal(3000, d.Nobs);
    }

    [Fact]
    public void ResetSum()
    {
        Sum<int> a = new(), b = new(), c = new();
        var rng = new Random();
        
        for (var i = 0; i < 1000; i++)
        {
            a.Fit(rng.Next());
            b.Fit(rng.Next());
            c.Fit(rng.Next());
        }

        var d = Sum<int>.Merge(a, b, c);
        Assert.Equal(3000, d.Nobs);

        Assert.Equal(1000, a.Nobs);
        Assert.Equal(1000, b.Nobs);
        Assert.Equal(1000, c.Nobs);
        
        a.Merge(b);
        
        Assert.Equal(2000, a.Nobs);
        Assert.Equal(1000, b.Nobs);
        
        b.Merge(c);
        a.Merge(c);
        
        Assert.Equal(3000, a.Nobs);
        Assert.Equal(2000, b.Nobs);
        Assert.Equal(1000, c.Nobs);
        
        a.Reset();
        
        Assert.Equal(0, a.Nobs);
        Assert.Equal(2000, b.Nobs);
        Assert.Equal(1000, c.Nobs);
        Assert.Equal(3000, d.Nobs);
        
        b.Reset();
        
        Assert.Equal(0, a.Nobs);
        Assert.Equal(0, b.Nobs);
        Assert.Equal(1000, c.Nobs);
        Assert.Equal(3000, d.Nobs);
        
        c.Reset();
        
        Assert.Equal(0, a.Nobs);
        Assert.Equal(0, b.Nobs);
        Assert.Equal(0, c.Nobs);
        Assert.Equal(3000, d.Nobs);
        
        d.Reset();
        
        Assert.Equal(0, a.Nobs);
        Assert.Equal(0, b.Nobs);
        Assert.Equal(0, c.Nobs);
        Assert.Equal(0, d.Nobs);
        
        Assert.Equal(0, a.Value);
        Assert.Equal(0, b.Value);
        Assert.Equal(0, c.Value);
        Assert.Equal(0, d.Value);
    }
}