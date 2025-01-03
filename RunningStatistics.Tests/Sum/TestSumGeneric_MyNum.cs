using System;
using Xunit;

namespace RunningStatistics.Tests.Sum;

public partial class TestSumGeneric
{
    [Fact]
    public void SumWithMyNum_NoNumbers()
    {
        var sum = new Sum<MyNum>();
        Assert.Equal(new MyNum(0), sum.Value);
    }

    [Fact]
    public void SumWithMyNum_SingleNumber()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(42));
        Assert.Equal(new MyNum(42), sum.Value);
    }

    [Fact]
    public void SumOfMyNum_PositiveNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(20));
        sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(60), sum.Value);
    }

    [Fact]
    public void SumOfMyNum_NegativeNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(-10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(-30));
        Assert.Equal(new MyNum(-60), sum.Value);
    }

    [Fact]
    public void SumOfMyNum_MixedNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(20), sum.Value);
    }
    
    [Fact]
    public void MeanWithMyNum_NoNumbers()
    {
        var sum = new Sum<MyNum>();
        Assert.Throws<DivideByZeroException>(() => sum.Mean());
    }

    [Fact]
    public void MeanWithMyNum_SingleNumber()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(42));
        Assert.Equal(new MyNum(42), sum.Mean());
    }

    [Fact]
    public void MeanOfMyNum_PositiveNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(20));
        sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(20), sum.Mean());
    }

    [Fact]
    public void MeanOfMyNum_NegativeNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(-10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(-30));
        Assert.Equal(new MyNum(-20), sum.Mean());
    }

    [Fact]
    public void MeanOfMyNum_MixedNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10));
        sum.Fit(new MyNum(-20));
        sum.Fit(new MyNum(30));
        Assert.Equal(6.67m, sum.Mean().Value, 2);
    }
}