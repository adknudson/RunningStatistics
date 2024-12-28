using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests;

public class TestMean
{
    [Fact]
    public void Mean_ReturnsNaN_WhenNoNumbersAdded()
    {
        var mean = new Mean();
        Assert.Equal(double.NaN, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenSingleNumberAdded()
    {
        var mean = new Mean();
        mean.Fit(5);
        Assert.Equal(5, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenMultipleNumbersAdded()
    {
        var mean = new Mean();
        mean.Fit(new List<double> { 5, 10, 15 });
        Assert.Equal(10, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenNegativeNumbersAdded()
    {
        var mean = new Mean();
        mean.Fit(new List<double> { -5, -10, -15 });
        Assert.Equal(-10, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenMixedNumbersAdded()
    {
        var mean = new Mean();
        mean.Fit(new List<double> { -5, 10, 15 });
        Assert.Equal(6.67, mean.Value, 2);
    }

    [Fact]
    public void Mean_ResetsCorrectly()
    {
        var mean = new Mean();
        mean.Fit(new List<double> { 5, 10, 15 });
        mean.Reset();
        Assert.Equal(double.NaN, mean.Value);
    }

    [Fact]
    public void Mean_ClonesCorrectly()
    {
        var mean = new Mean();
        mean.Fit(new List<double> { 5, 10, 15 });
        var clone = mean.Clone();
        Assert.Equal(mean.Value, clone.Value);
        Assert.Equal(mean.Nobs, clone.Nobs);
    }

    [Fact]
    public void Mean_ClonesEmptyCorrectly()
    {
        var mean = new Mean();
        var emptyClone = mean.CloneEmpty();
        Assert.Equal(double.NaN, emptyClone.Value);
        Assert.Equal(0, emptyClone.Nobs);
    }

    [Fact]
    public void Mean_MergesCorrectly()
    {
        var mean1 = new Mean();
        mean1.Fit(new List<double> { 5, 10, 15 });
        var mean2 = new Mean();
        mean2.Fit(new List<double> { 20, 25, 30 });
        mean1.Merge(mean2);
        Assert.Equal(17.5, mean1.Value, 1);
    }
    
    [Fact]
    public void MergeEmptyDoesNotPropagateNaNs()
    {
        var a = new Mean();
        a.Fit(3.14159);
        a.Fit(2.71828);
        var m = a.Value;

        var b = new Mean();
        Assert.Equal(double.NaN, b.Value);
        
        a.Merge(b);
        Assert.Equal(a.Value, m);
    }

    [Fact]
    public void Merge_ThrowsOnNonFiniteObservation()
    {
        var a = new Mean();
        Assert.Throws<ArgumentException>(() => a.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => a.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => a.Fit(double.NegativeInfinity));
    }
}