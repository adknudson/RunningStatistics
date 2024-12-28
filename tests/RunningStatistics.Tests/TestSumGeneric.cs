using System;
using Xunit;

namespace RunningStatistics.Tests;

public class TestSumGeneric
{
    [Fact]
    public void SumWithNoNumbers()
    {
        var sum = new Sum<long>();
        Assert.Equal(0L, sum.Value);
    }

    [Fact]
    public void SumWithSingleNumber()
    {
        var sum = new Sum<long>();
        sum.Fit(42L);
        Assert.Equal(42L, sum.Value);
    }

    [Fact]
    public void SumOfPositiveNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(20L);
        sum.Fit(30L);
        Assert.Equal(60L, sum.Value);
    }

    [Fact]
    public void SumOfNegativeNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(-10L);
        sum.Fit(-20L);
        sum.Fit(-30L);
        Assert.Equal(-60L, sum.Value);
    }

    [Fact]
    public void SumOfMixedNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(-20L);
        sum.Fit(30L);
        Assert.Equal(20L, sum.Value);
    }

    [Fact]
    public void MeanWithNoNumbers()
    {
        var sum = new Sum<long>();
        Assert.Equal(double.NaN, sum.Mean());
    }

    [Fact]
    public void MeanWithSingleNumber()
    {
        var sum = new Sum<long>();
        sum.Fit(42L);
        Assert.Equal(42.0, sum.Mean());
    }

    [Fact]
    public void MeanOfPositiveNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(20L);
        sum.Fit(30L);
        Assert.Equal(20.0, sum.Mean());
    }

    [Fact]
    public void MeanOfNegativeNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(-10L);
        sum.Fit(-20L);
        sum.Fit(-30L);
        Assert.Equal(-20.0, sum.Mean());
    }

    [Fact]
    public void MeanOfMixedNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(-20L);
        sum.Fit(30L);
        Assert.Equal(6.67, sum.Mean(), 2);
    }
}