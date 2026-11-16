using System;
using Xunit;

namespace RunningStatistics.Tests.Histogram;

public class TestHistogramBin
{
    [Fact]
    public void Constructor_ThrowsOnInvalidBounds()
    {
        Assert.Throws<ArgumentException>(() => new HistogramBin(10, 0, true, false));
    }
    
    [Fact]
    public void Constructor_ThrowsWhenBothBoundsInfinite()
    {
        Assert.Throws<ArgumentException>(() =>
            new HistogramBin(double.NegativeInfinity, double.PositiveInfinity, true, false).Midpoint);
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.Equal(0, bin.Lower);
        Assert.Equal(10, bin.Upper);
        Assert.Equal(0, bin.Nobs);
        Assert.True(bin.ClosedLeft);
        Assert.False(bin.ClosedRight);
    }
    
    [Fact]
    public void Contains_WithinBoundsTrue()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.True(bin.Contains(5));
    }

    [Fact]
    public void Contains_OutOfBoundsFalse()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.False(bin.Contains(15));
    }

    [Fact]
    public void Contains_ClosedLeftIncludesLowerBound()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.True(bin.Contains(0));
    }

    [Fact]
    public void Contains_OpenLeftExcludesLowerBound()
    {
        var bin = new HistogramBin(0, 10, false, false);
        Assert.False(bin.Contains(0));
    }

    [Fact]
    public void Contains_ClosedRightIncludesUpperBound()
    {
        var bin = new HistogramBin(0, 10, false, true);
        Assert.True(bin.Contains(10));
    }

    [Fact]
    public void Contains_OpenRightExcludesUpperBound()
    {
        var bin = new HistogramBin(0, 10, false, false);
        Assert.False(bin.Contains(10));
    }
    
    [Fact]
    public void Increment_ThrowsOnNegativeCount()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.Throws<ArgumentOutOfRangeException>(() => bin.Increment(-1));
    }
    
    [Fact]
    public void Midpoint_CalculatesCorrectly()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.Equal(5, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_InfiniteLowerBound()
    {
        var bin = new HistogramBin(double.NegativeInfinity, 10, true, false);
        Assert.Equal(double.NegativeInfinity, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_InfiniteUpperBound()
    {
        var bin = new HistogramBin(0, double.PositiveInfinity, true, false);
        Assert.Equal(double.PositiveInfinity, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_LargeNumbers()
    {
        var bin = new HistogramBin(double.MaxValue / 2, double.MaxValue, true, false);
        Assert.Equal(double.MaxValue * 0.75, bin.Midpoint);
    }

    [Fact]
    public void Midpoint_SmallNumbers()
    {
        var bin = new HistogramBin(double.MinValue, double.MinValue / 2, true, false);
        Assert.Equal(double.MinValue * 0.75, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_ValuesCloseToZero()
    {
        var bin = new HistogramBin(-1e-10, 1e-10, true, false);
        Assert.Equal(0, bin.Midpoint);
    }

    [Fact]
    public void Midpoint_PositiveValuesCloseToZero()
    {
        var bin = new HistogramBin(1e-10, 2e-10, true, false);
        Assert.Equal(1.5e-10, bin.Midpoint);
    }

    [Fact]
    public void Midpoint_NegativeValuesCloseToZero()
    {
        var bin = new HistogramBin(-2e-10, -1e-10, true, false);
        Assert.Equal(-1.5e-10, bin.Midpoint);
    }
}