﻿using Xunit;

namespace RunningStatistics.Tests;

public class TestSum
{
    [Fact]
    public void SumWithNoNumbers()
    {
        var sum = new Sum();
        Assert.Equal(0, sum.Value);
    }

    [Fact]
    public void SumWithSingleNumber()
    {
        var sum = new Sum();
        sum.Fit(42);
        Assert.Equal(42, sum.Value);
    }
    
    [Fact]
    public void SumOfPositiveNumbers()
    {
        var sum = new Sum();
        sum.Fit(10);
        sum.Fit(20);
        sum.Fit(30);
        Assert.Equal(60, sum.Value);
    }

    [Fact]
    public void SumOfNegativeNumbers()
    {
        var sum = new Sum();
        sum.Fit(-10);
        sum.Fit(-20);
        sum.Fit(-30);
        Assert.Equal(-60, sum.Value);
    }

    [Fact]
    public void SumOfMixedNumbers()
    {
        var sum = new Sum();
        sum.Fit(10);
        sum.Fit(-20);
        sum.Fit(30);
        Assert.Equal(20, sum.Value);
    }
    
    [Fact]
    public void MeanWithNoNumbers()
    {
        var sum = new Sum();
        Assert.Equal(double.NaN, sum.Mean());
    }

    [Fact]
    public void MeanWithSingleNumber()
    {
        var sum = new Sum();
        sum.Fit(42);
        Assert.Equal(42, sum.Mean());
    }

    [Fact]
    public void MeanOfPositiveNumbers()
    {
        var sum = new Sum();
        sum.Fit(10);
        sum.Fit(20);
        sum.Fit(30);
        Assert.Equal(20, sum.Mean());
    }

    [Fact]
    public void MeanOfNegativeNumbers()
    {
        var sum = new Sum();
        sum.Fit(-10);
        sum.Fit(-20);
        sum.Fit(-30);
        Assert.Equal(-20, sum.Mean());
    }

    [Fact]
    public void MeanOfMixedNumbers()
    {
        var sum = new Sum();
        sum.Fit(10);
        sum.Fit(-20);
        sum.Fit(30);
        Assert.Equal(6.67, sum.Mean(), 2);
    }
}