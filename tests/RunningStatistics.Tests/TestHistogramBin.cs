using System;
using Xunit;

namespace RunningStatistics.Tests;

public class TestHistogramBin
{
    [Fact]
    public void ConstructorThrowsExceptionForInvalidBounds()
    {
        Assert.Throws<ArgumentException>(() => new HistogramBin(10, 0, true, false));
    }
    
    [Fact]
    public void ConstructorThrowsExceptionForBothBoundsInfinite()
    {
        Assert.Throws<ArgumentException>(() =>
            new HistogramBin(double.NegativeInfinity, double.PositiveInfinity, true, false).Midpoint);
    }

    [Fact]
    public void ConstructorInitializesPropertiesCorrectly()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.Equal(0, bin.Lower);
        Assert.Equal(10, bin.Upper);
        Assert.Equal(0, bin.Nobs);
        Assert.True(bin.ClosedLeft);
        Assert.False(bin.ClosedRight);
        Assert.Equal("[0.00, 10.00)", bin.BinName);
    }
    
    [Fact]
    public void ContainsValueReturnsTrueForValueWithinBounds()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.True(bin.Contains(5));
    }

    [Fact]
    public void ContainsValueReturnsFalseForValueOutOfBounds()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.False(bin.Contains(15));
    }

    [Fact]
    public void ContainsValueReturnsTrueForLowerBoundWhenClosedLeft()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.True(bin.Contains(0));
    }

    [Fact]
    public void ContainsValueReturnsFalseForLowerBoundWhenOpenLeft()
    {
        var bin = new HistogramBin(0, 10, false, false);
        Assert.False(bin.Contains(0));
    }

    [Fact]
    public void ContainsValueReturnsTrueForUpperBoundWhenClosedRight()
    {
        var bin = new HistogramBin(0, 10, false, true);
        Assert.True(bin.Contains(10));
    }

    [Fact]
    public void ContainsValueReturnsFalseForUpperBoundWhenOpenRight()
    {
        var bin = new HistogramBin(0, 10, false, false);
        Assert.False(bin.Contains(10));
    }
    
    [Fact]
    public void IncrementThrowsExceptionForNegativeCount()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.Throws<ArgumentOutOfRangeException>(() => bin.Increment(-1));
    }
    
    [Fact]
    public void MidpointCalculatesCorrectly()
    {
        var bin = new HistogramBin(0, 10, true, false);
        Assert.Equal(5, bin.Midpoint);
    }
    
    [Fact]
    public void MidpointCalculatesCorrectlyForInfiniteLowerBound()
    {
        var bin = new HistogramBin(double.NegativeInfinity, 10, true, false);
        Assert.Equal(double.NegativeInfinity, bin.Midpoint);
    }
    
    [Fact]
    public void MidpointCalculatesCorrectlyForInfiniteUpperBound()
    {
        var bin = new HistogramBin(0, double.PositiveInfinity, true, false);
        Assert.Equal(double.PositiveInfinity, bin.Midpoint);
    }
    
    [Fact]
    public void MidpointCalculatesCorrectlyForLargeNumbers()
    {
        var bin = new HistogramBin(double.MaxValue / 2, double.MaxValue, true, false);
        Assert.Equal(double.MaxValue * 0.75, bin.Midpoint);
    }

    [Fact]
    public void MidpointCalculatesCorrectlyForSmallNumbers()
    {
        var bin = new HistogramBin(double.MinValue, double.MinValue / 2, true, false);
        Assert.Equal(double.MinValue * 0.75, bin.Midpoint);
    }
    
    [Fact]
    public void MidpointCalculatesCorrectlyForValuesCloseToZero()
    {
        var bin = new HistogramBin(-1e-10, 1e-10, true, false);
        Assert.Equal(0, bin.Midpoint);
    }

    [Fact]
    public void MidpointCalculatesCorrectlyForPositiveValuesCloseToZero()
    {
        var bin = new HistogramBin(1e-10, 2e-10, true, false);
        Assert.Equal(1.5e-10, bin.Midpoint);
    }

    [Fact]
    public void MidpointCalculatesCorrectlyForNegativeValuesCloseToZero()
    {
        var bin = new HistogramBin(-2e-10, -1e-10, true, false);
        Assert.Equal(-1.5e-10, bin.Midpoint);
    }
}