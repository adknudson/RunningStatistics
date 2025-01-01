using System;
using Xunit;

namespace RunningStatistics.Tests.Sum;

public partial class TestSumGenericMyNum
{
    [Fact]
    public void SumWithNoNumbers()
    {
        var sum = new Sum<MyNum>();
        Assert.Equal(new MyNum(0), sum.Value);
    }

    [Fact]
    public void SumWithSingleNumber()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(42));
        Assert.Equal(new MyNum(42), sum.Value);
    }

    [Fact]
    public void SumOfPositiveNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(20));
        sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(60), sum.Value);
    }

    [Fact]
    public void SumOfNegativeNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(-10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(-30));
        Assert.Equal(new MyNum(-60), sum.Value);
    }

    [Fact]
    public void SumOfMixedNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(20), sum.Value);
    }
    
    [Fact]
    public void MeanWithNoNumbers()
    {
        var sum = new Sum<MyNum>();
        Assert.Throws<DivideByZeroException>(() => sum.Mean());
    }

    [Fact]
    public void MeanWithSingleNumber()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(42));
        Assert.Equal(new MyNum(42), sum.Mean());
    }

    [Fact]
    public void MeanOfPositiveNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(20));
        sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(20), sum.Mean());
    }

    [Fact]
    public void MeanOfNegativeNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(-10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(-30));
        Assert.Equal(new MyNum(-20), sum.Mean());
    }

    [Fact]
    public void MeanOfMixedNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(30));
        Assert.Equal(6.67m, sum.Mean().Value, 2);
    }
}