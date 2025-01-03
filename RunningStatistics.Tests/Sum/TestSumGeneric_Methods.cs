using Xunit;

namespace RunningStatistics.Tests.Sum;

public partial class TestSumGeneric
{
    [Fact]
    public void SumWith_NoNumbers()
    {
        var sum = new Sum<long>();
        Assert.Equal(0L, sum.Value);
    }

    [Fact]
    public void SumWith_SingleNumber()
    {
        var sum = new Sum<long>();
        sum.Fit(42L);
        Assert.Equal(42L, sum.Value);
    }

    [Fact]
    public void SumOf_PositiveNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(20L);
        sum.Fit(30L);
        Assert.Equal(60L, sum.Value);
    }

    [Fact]
    public void SumOf_NegativeNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(-10L);
        sum.Fit(-20L);
        sum.Fit(-30L);
        Assert.Equal(-60L, sum.Value);
    }

    [Fact]
    public void SumOf_MixedNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(-20L);
        sum.Fit(30L);
        Assert.Equal(20L, sum.Value);
    }

    [Fact]
    public void MeanWith_NoNumbers()
    {
        var sum = new Sum<long>();
        Assert.Equal(double.NaN, sum.Mean());
    }

    [Fact]
    public void MeanWith_SingleNumber()
    {
        var sum = new Sum<long>();
        sum.Fit(42L);
        Assert.Equal(42.0, sum.Mean());
    }

    [Fact]
    public void MeanOf_PositiveNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(20L);
        sum.Fit(30L);
        Assert.Equal(20.0, sum.Mean());
    }

    [Fact]
    public void MeanOf_NegativeNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(-10L);
        sum.Fit(-20L);
        sum.Fit(-30L);
        Assert.Equal(-20.0, sum.Mean());
    }

    [Fact]
    public void MeanOf_MixedNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L);
        sum.Fit(-20L);
        sum.Fit(30L);
        Assert.Equal(6.67, sum.Mean(), 2);
    }
}