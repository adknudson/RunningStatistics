using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Histogram;

public class TestHistogramBin
{
    [Fact]
    public void Constructor_ThrowsOnInvalidBounds()
    {
        Assert.Throws<ArgumentException>(() => new RunningStatistics.Histogram.Bin(10, 0, true, false));
    }
    
    [Fact]
    public void Constructor_ThrowsWhenBothBoundsInfinite()
    {
        Assert.Throws<ArgumentException>(() =>
            new RunningStatistics.Histogram.Bin(double.NegativeInfinity, double.PositiveInfinity, true, false).Midpoint);
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, true, false);
        Assert.Equal(0, bin.Lower);
        Assert.Equal(10, bin.Upper);
        Assert.Equal(0, bin.Nobs);
        Assert.True(bin.ClosedLeft);
        Assert.False(bin.ClosedRight);
    }
    
    [Fact]
    public void Contains_WithinBoundsTrue()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, true, false);
        Assert.True(bin.Contains(5));
    }

    [Fact]
    public void Contains_OutOfBoundsFalse()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, true, false);
        Assert.False(bin.Contains(15));
    }

    [Fact]
    public void Contains_ClosedLeftIncludesLowerBound()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, true, false);
        Assert.True(bin.Contains(0));
    }

    [Fact]
    public void Contains_OpenLeftExcludesLowerBound()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, false, false);
        Assert.False(bin.Contains(0));
    }

    [Fact]
    public void Contains_ClosedRightIncludesUpperBound()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, false, true);
        Assert.True(bin.Contains(10));
    }

    [Fact]
    public void Contains_OpenRightExcludesUpperBound()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, false, false);
        Assert.False(bin.Contains(10));
    }
    
    [Fact]
    public void Increment_ThrowsOnNegativeCount()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, true, false);
        Assert.Throws<ArgumentOutOfRangeException>(() => bin.Increment(-1));
    }
    
    [Fact]
    public void Midpoint_CalculatesCorrectly()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, 10, true, false);
        Assert.Equal(5, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_InfiniteLowerBound()
    {
        var bin = new RunningStatistics.Histogram.Bin(double.NegativeInfinity, 10, true, false);
        Assert.Equal(double.NegativeInfinity, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_InfiniteUpperBound()
    {
        var bin = new RunningStatistics.Histogram.Bin(0, double.PositiveInfinity, true, false);
        Assert.Equal(double.PositiveInfinity, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_LargeNumbers()
    {
        var bin = new RunningStatistics.Histogram.Bin(double.MaxValue / 2, double.MaxValue, true, false);
        Assert.Equal(double.MaxValue * 0.75, bin.Midpoint);
    }

    [Fact]
    public void Midpoint_SmallNumbers()
    {
        var bin = new RunningStatistics.Histogram.Bin(double.MinValue, double.MinValue / 2, true, false);
        Assert.Equal(double.MinValue * 0.75, bin.Midpoint);
    }
    
    [Fact]
    public void Midpoint_ValuesCloseToZero()
    {
        var bin = new RunningStatistics.Histogram.Bin(-1e-10, 1e-10, true, false);
        Assert.Equal(0, bin.Midpoint);
    }

    [Fact]
    public void Midpoint_PositiveValuesCloseToZero()
    {
        var bin = new RunningStatistics.Histogram.Bin(1e-10, 2e-10, true, false);
        Assert.Equal(1.5e-10, bin.Midpoint);
    }

    [Fact]
    public void Midpoint_NegativeValuesCloseToZero()
    {
        var bin = new RunningStatistics.Histogram.Bin(-2e-10, -1e-10, true, false);
        Assert.Equal(-1.5e-10, bin.Midpoint);
    }

    // Edge-accuracy audit: Contains must agree with the closed-interval model at
    // every boundary and at the doubles adjacent to each boundary. A probe one unit
    // in the last place inside or outside a side must sit on that side's answer. The
    // probes also cover infinite bounds, where the touching neighbor is the largest
    // finite double, and a NaN probe, which must take the documented unexpected-state
    // throw rather than answer silently. The audit run of 2026-11-19 found no
    // disagreement. A red fact means Contains drifted from the documented interval.
    [Fact]
    public void Gate_ContainsMatchesClosureModelAtBoundaryNeighbors()
    {
        var lowers = new[] { 0.0, 0.0, double.NegativeInfinity };
        var uppers = new[] { 10.0, double.PositiveInfinity, 10.0 };
        for (var shape = 0; shape < lowers.Length; shape++)
        {
            var lower = lowers[shape];
            var upper = uppers[shape];
            foreach (var closedLeft in new[] { true, false })
            {
                foreach (var closedRight in new[] { true, false })
                {
                    var bin = new RunningStatistics.Histogram.Bin(lower, upper, closedLeft, closedRight);

                    var probes = new List<double> { lower, upper, bin.Midpoint };
                    if (lower == double.NegativeInfinity)
                    {
                        // The double touching an infinite bound from inside is the largest
                        // finite one of that sign.
                        probes.Add(-LargestFiniteDouble);
                    }
                    else
                    {
                        probes.Add(lower - 1.0);
                        probes.Add(NextUlpBelow(lower));
                        probes.Add(NextUlpAbove(lower));
                    }

                    if (upper == double.PositiveInfinity)
                    {
                        probes.Add(LargestFiniteDouble);
                    }
                    else
                    {
                        probes.Add(upper + 1.0);
                        probes.Add(NextUlpBelow(upper));
                        probes.Add(NextUlpAbove(upper));
                    }

                    probes.Add(0.0);
                    probes.Add(5.0);
                    probes.Add(15.0);
                    probes.Add(1e300);
                    probes.Add(-1e300);

                    foreach (var value in probes)
                    {
                        Assert.True(bin.Contains(value) == ContainsByClosureModel(bin, value),
                                    $"Contains disagrees with the closed-interval model at {value} for bin {bin}");
                    }

                    Assert.Throws<InvalidOperationException>(() => bin.Contains(double.NaN));
                }
            }
        }
    }

    // The largest double that is not infinite; the double touching either
    // infinity from inside.
    private const double LargestFiniteDouble = 1.7976931348623157e308;

    // The doubles one step above and below a finite value. Reading the bit pattern
    // as a whole number orders the doubles like the real line orders them, except
    // below zero, where a negative pattern shrinks as the value grows. Zero needs
    // its own step because -0.0 sits at the bottom end of the negative patterns.
    private static double NextUlpAbove(double value)
    {
        if (value == double.PositiveInfinity || value == double.NegativeInfinity) return value;
        var bits = BitConverter.DoubleToInt64Bits(value);
        return BitConverter.Int64BitsToDouble(bits < 0 ? bits - 1 : bits + 1);
    }

    private static double NextUlpBelow(double value)
    {
        if (value == double.PositiveInfinity || value == double.NegativeInfinity) return value;
        var bits = BitConverter.DoubleToInt64Bits(value);
        if (bits == 0) bits = long.MinValue;
        return BitConverter.Int64BitsToDouble(bits < 0 ? bits + 1 : bits - 1);
    }

    private static bool ContainsByClosureModel(RunningStatistics.Histogram.Bin bin, double value)
    {
        var inFromLeft = value > bin.Lower || (value == bin.Lower && bin.ClosedLeft);
        var inFromRight = value < bin.Upper || (value == bin.Upper && bin.ClosedRight);
        return inFromLeft && inFromRight;
    }
}