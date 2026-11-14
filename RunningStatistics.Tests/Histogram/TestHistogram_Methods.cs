using System;
using System.Linq;
using Xunit;

namespace RunningStatistics.Tests.Histogram;

public partial class TestHistogram
{
    // |-inf, 0|, |0, 10|, |10, 100|
    private readonly double[] _edges = [double.NegativeInfinity, 0, 10, 100];
    
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var hist = new RunningStatistics.Histogram([0, 0.5, 1]);
        hist.Fit(0.5, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => hist.Fit(0.5, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, hist.Nobs);
    }
    
    // Commit 17: edge-validation guards in the constructor. Scenarios that silently
    // constructed today (red): any NaN edge (any position), unsorted/non-monotonic edges
    // (the ctor used to OrderBy-sort them), and (implicitly) duplicate edges. The ctor
    // now requires finite-or-infinite (no NaN), strictly monotonically increasing edges.
    [Fact]
    public void Ctor_RejectsNaNEdges()
    {
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([0.0, double.NaN, 5.0]));
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([double.NaN, 1.0, 2.0]));
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([0.0, 1.0, double.NaN]));
    }
    
    [Fact]
    public void Ctor_RejectsDuplicateEdges()
    {
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([0.0, 1.0, 1.0]));
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([0.0, 0.0, 1.0]));
    }
    
    [Fact]
    public void Ctor_RejectsUnsortedEdges()
    {
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([0.0, 1.0, 0.5]));
        Assert.Throws<ArgumentException>(
            () => new RunningStatistics.Histogram([2.0, 1.0, 0.0]));
    }
    
    [Fact]
    public void Ctor_RejectsTooFewEdges()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RunningStatistics.Histogram([0.5]));
    }
    
    [Fact]
    public void Ctor_AcceptsInfiniteEdges()
    {
        RunningStatistics.Histogram h = new([double.NegativeInfinity, 0, 10, 100]);
        Assert.Equal([double.NegativeInfinity, 0, 10, 100], h.Edges);
        Assert.Equal(0, h.Nobs);
        
        RunningStatistics.Histogram upperOpen = new([0.0, 10.0, double.PositiveInfinity]);
        Assert.Equal([0.0, 10.0, double.PositiveInfinity], upperOpen.Edges);
        Assert.Equal(0, upperOpen.Nobs);
    }
    
    [Fact]
    public void Ctor_AcceptsValidEdgesUnchanged()
    {
        RunningStatistics.Histogram hist = new([double.NegativeInfinity, 0, 10, 100],
            leftClosed: false,
            endsClosed: false);
        Assert.Equal([double.NegativeInfinity, 0, 10, 100], hist.Edges);
        Assert.False(hist.LeftClosed);
        Assert.False(hist.EndsClosed);
        
        var numBins = 0;
        foreach (var bin in hist)
        {
            numBins++;
        }
        Assert.Equal(3, numBins);
    }
    
    [Fact]
    public void Fit_RejectsNaNObservations()
    {
        RunningStatistics.Histogram h = new(_edges);
        h.Fit(5);
        Assert.Throws<ArgumentException>(() => h.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => h.Fit(double.NaN, 3));
        Assert.Equal(1, h.Nobs);
    }
    
    [Fact]
    public void Merge_MatchingEdgesSumsBinsDirectly()
    {
        RunningStatistics.Histogram a = new(_edges);
        RunningStatistics.Histogram b = new(_edges);
        a.Fit(-1);
        a.Fit(5);
        b.Fit(10);
        b.Fit(1000);
        
        a.Merge(b);
        
        int[] expectedCounts = [1, 1, 1];
        var i = 0;
        foreach (var bin in a)
        {
            Assert.Equal(expectedCounts[i], bin.Nobs);
            i++;
        }
        Assert.Equal(3, i);
        Assert.Equal(4, a.Nobs);
        Assert.Equal(0, a.OutOfBoundsCounts.Lower);
        Assert.Equal(1, a.OutOfBoundsCounts.Upper);
    }
    
    [Fact]
    public void FitValueOutOfBoundsIncreasesOutOfBoundsCount()
    {
        RunningStatistics.Histogram h = new(_edges, false, false);
        h.Fit(200);
        Assert.Equal(1, h.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void ResetClearsAllCounts()
    {
        RunningStatistics.Histogram h = new(_edges, false, false);
        h.Fit(5);
        h.Fit(15);
        h.Reset();
        Assert.Equal(0, h.Nobs);
        foreach (var bin in h)
        {
            Assert.Equal(0, bin.Nobs);
        }
    }
    
    [Fact]
    public void EmptyHistIsZero()
    {
        // (-inf, 0], (0, 10], (10, 100]
        RunningStatistics.Histogram h = new(_edges, false, false);

        Assert.Equal(0, h.Nobs);

        foreach (var bin in h)
        {
            Assert.Equal(0, bin.Nobs);
        }
    }

    [Fact]
    public void MergePartsEqualsMergeAll()
    {
        const int n = 2000;
        var rng = new Random();
        double[] smallEdges = [0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0];

        RunningStatistics.Histogram a = new(smallEdges), b = new(smallEdges), c = new(smallEdges);

        double v;
        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            a.Fit(v);
            c.Fit(v);
        }

        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            b.Fit(v);
            c.Fit(v);
        }

        a.Merge(b);

        foreach (var (binA, binC) in a.Zip(c))
        {
            Assert.Equal(binA.Nobs, binC.Nobs);
        }
    }

    [Fact]
    public void LeftClosed()
    {
        // [-inf, 0), [0, 10), [10, 100]
        RunningStatistics.Histogram h = new(_edges);

        h.Fit(-1);
        h.Fit(1);
        h.Fit(20);
        h.Fit(1000);

        // One value out of bounds on the upper end
        Assert.Equal(0, h.OutOfBoundsCounts.Lower);
        Assert.Equal(1, h.OutOfBoundsCounts.Upper);

        foreach (var bin in h)
        {
            Assert.Equal(1, bin.Nobs);
        }
    }

    [Fact]
    public void LeftOpen()
    {
        // [-inf, 0), [0, 10), [10, 100)
        RunningStatistics.Histogram h = new(_edges, true, false);

        h.Fit(-1);
        h.Fit(1);
        h.Fit(20);
        h.Fit(100);

        // One value out of bounds on the upper end
        Assert.Equal(0, h.OutOfBoundsCounts.Lower);
        Assert.Equal(1, h.OutOfBoundsCounts.Upper);

        foreach (var bin in h)
        {
            Assert.Equal(1, bin.Nobs);
        }
    }

    [Fact]
    public void RightClosed()
    {
        // [-inf, 0], (0, 10], (10, 100]
        RunningStatistics.Histogram h = new(_edges, false);

        h.Fit(double.NegativeInfinity);
        h.Fit(10);
        h.Fit(100);
        h.Fit(101);

        // One value out of bounds on the upper end
        Assert.Equal(0, h.OutOfBoundsCounts.Lower);
        Assert.Equal(1, h.OutOfBoundsCounts.Upper);

        foreach (var bin in h)
        {
            Assert.Equal(1, bin.Nobs);
        }
    }

    [Fact]
    public void RightOpen()
    {
        // (-inf, 0], (0, 10], (10, 100]
        RunningStatistics.Histogram h = new(_edges, false, false);

        h.Fit(double.NegativeInfinity);
        h.Fit(0.0);
        h.Fit(10);
        h.Fit(100);
        h.Fit(double.PositiveInfinity);

        // Two values out of bounds on both ends (one each)
        Assert.Equal(1, h.OutOfBoundsCounts.Lower);
        Assert.Equal(1, h.OutOfBoundsCounts.Upper);

        foreach (var bin in h)
        {
            Assert.Equal(1, bin.Nobs);
        }
    }

    [Fact]
    public void StaticMergeDoesNotAffectOriginals()
    {
        // (0, 0.1], (0.1, 0.5], (0.5, 0.9]
        RunningStatistics.Histogram a = new([0, 0.1, 0.5, 0.9], leftClosed: false, endsClosed: false);
        RunningStatistics.Histogram b = new([0, 0.1, 0.5, 0.9], leftClosed: false, endsClosed: false);
            
        var rng = new Random();
            
        for (var i = 0; i < 1000; i++)
        {
            a.Fit(rng.NextDouble());
            b.Fit(rng.NextDouble());
        }

        var c = RunningStatistics.Histogram.Merge(a, b);
        Assert.Equal(a.Nobs + b.Nobs, c.Nobs);
        Assert.Equal(a.OutOfBoundsCounts.Upper + b.OutOfBoundsCounts.Upper, c.OutOfBoundsCounts.Upper);

        c.Fit(0.99, 10);
        Assert.Equal(a.OutOfBoundsCounts.Upper + b.OutOfBoundsCounts.Upper + 10, c.OutOfBoundsCounts.Upper);
    }
}