using System;
using System.Collections.Generic;
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

    // -----------------------------------------------------------------------
    // Commit 18: PIN — observation/bin MEMBERSHIP at edges for every
    // (leftClosed, endsClosed) combination. Pins exactly what UncheckedFit +
    // HistogramBin.Contains do today. BinCounts(h) = the bin Nobs by bin index,
    // so the exact bin getting each edge observation is pinned. The conservation
    // identity Nobs == SumBins(h) + OutOfBounds.Lower + OutOfBounds.Upper is
    // pinned in every scenario: no bin double-counts an observation and no
    // observation counted in Nobs is counted nowhere.
    //
    // Membership table (edges e0<e1<...<e4, bins B0=[e0,e1)-style ... B3):
    //   interior edge ek:  leftClosed=true  -> Bk    (upper bin, bins [a,b))
    //                      leftClosed=false -> B(k-1) (lower bin, bins (a,b])
    //   first edge e0:     leftClosed=true -> B0 always
    //                      leftClosed=false: endsClosed=true -> B0; false -> OOB.Lower
    //   last edge e4:      leftClosed=false -> B3 always
    //                      leftClosed=true: endsClosed=true -> B3; false -> OOB.Upper
    //   v < e0 / v > e4:   OOB.Lower / OOB.Upper flag-independently (never thrown;
    //                      counted in Nobs, not in bins)
    //   -Infinity at a -Infinity lower edge: in B0 iff first bin closed at Left
    //     (leftClosed=true, or leftClosed=false AND endsClosed=true)
    //   +Infinity at a +Infinity upper edge: in last bin iff last bin closed at Right
    //     (leftClosed=false always; leftClosed=true only when endsClosed=true)
    // -----------------------------------------------------------------------

    private static int[] BinCounts(RunningStatistics.Histogram histogram)
    {
        var counts = new List<int>();
        foreach (var bin in histogram)
        {
            counts.Add((int)bin.Nobs);
        }
        return counts.ToArray();
    }

    private static long SumBins(RunningStatistics.Histogram histogram)
    {
        var sum = 0L;
        foreach (var bin in histogram)
        {
            sum += bin.Nobs;
        }
        return sum;
    }

    [Fact]
    public void Boundary_InteriorEdgeLeftClosedToUpperBin()
    {
        // leftClosed=true: interior bins are [a, b), so an observation exactly at an
        // interior edge ek (0.25, 0.5, 0.75) is the LOWER (closed) bound of the bin
        // ABOVE -> it lands in Bk (indices 1, 2, 3). endsClosed never moves interior
        // edges: the same counts hold for endsClosed true and false.
        RunningStatistics.Histogram bothClosed = new([0.0, 0.25, 0.5, 0.75, 1.0], true, true);
        bothClosed.Fit(0.25);
        bothClosed.Fit(0.5);
        bothClosed.Fit(0.75);
        Assert.Equal([0, 1, 1, 1], BinCounts(bothClosed));
        Assert.Equal(3, bothClosed.Nobs);
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Lower);
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram endsOpen = new([0.0, 0.25, 0.5, 0.75, 1.0], true, false);
        endsOpen.Fit(0.25);
        endsOpen.Fit(0.5);
        endsOpen.Fit(0.75);
        Assert.Equal([0, 1, 1, 1], BinCounts(endsOpen));
        Assert.Equal(3, endsOpen.Nobs);
        Assert.Equal(0, endsOpen.OutOfBoundsCounts.Lower);
        Assert.Equal(0, endsOpen.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Boundary_InteriorEdgeLeftOpenToLowerBin()
    {
        // leftClosed=false: interior bins are (a, b], so an observation at an interior
        // edge ek is the UPPER (closed) bound of the bin BELOW -> B(k-1) (indices 0, 1, 2).
        // Mirror of the leftClosed=true case shifted one bin down, for both endsClosed.
        RunningStatistics.Histogram bothClosed = new([0.0, 0.25, 0.5, 0.75, 1.0], false, true);
        bothClosed.Fit(0.25);
        bothClosed.Fit(0.5);
        bothClosed.Fit(0.75);
        Assert.Equal([1, 1, 1, 0], BinCounts(bothClosed));
        Assert.Equal(3, bothClosed.Nobs);
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Lower);
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram endsOpen = new([0.0, 0.25, 0.5, 0.75, 1.0], false, false);
        endsOpen.Fit(0.25);
        endsOpen.Fit(0.5);
        endsOpen.Fit(0.75);
        Assert.Equal([1, 1, 1, 0], BinCounts(endsOpen));
        Assert.Equal(3, endsOpen.Nobs);
        Assert.Equal(0, endsOpen.OutOfBoundsCounts.Lower);
        Assert.Equal(0, endsOpen.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Boundary_FirstEdgeLeftClosedAlwaysInFirstBin()
    {
        // Observation exactly at the FIRST edge (0.0) with leftClosed=true: the first
        // bin's ClosedLeft is hard-coded true (InitializeBins), so it is in B0 for
        // either endsClosed. Counted once in bins, once in Nobs (conservation pinned).
        RunningStatistics.Histogram bothClosed = new([0.0, 0.25, 0.5, 0.75, 1.0], true, true);
        bothClosed.Fit(0.0);
        Assert.Equal([1, 0, 0, 0], BinCounts(bothClosed));
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Lower);
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Upper);
        Assert.Equal(1, bothClosed.Nobs);
        Assert.Equal(1, SumBins(bothClosed) + bothClosed.OutOfBoundsCounts.Lower + bothClosed.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram endsOpen = new([0.0, 0.25, 0.5, 0.75, 1.0], true, false);
        endsOpen.Fit(0.0);
        Assert.Equal([1, 0, 0, 0], BinCounts(endsOpen));
        Assert.Equal(0, endsOpen.OutOfBoundsCounts.Lower);
        Assert.Equal(0, endsOpen.OutOfBoundsCounts.Upper);
        Assert.Equal(1, endsOpen.Nobs);
        Assert.Equal(1, SumBins(endsOpen) + endsOpen.OutOfBoundsCounts.Lower + endsOpen.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Boundary_FirstEdgeLeftOpenEndsClosedDecides()
    {
        // leftClosed=false: the first bin is [0.0, ...) iff endsClosed=true (InitializeBins
        // passes EndsClosed as its closedLeft). endsClosed=false makes the first edge an
        // OPEN end: the observation at e0 is NOT dropped silently and NOT thrown — the
        // value.Equals(firstBin.Lower) branch in UncheckedFit routes it to the lower
        // OutOfBounds counter: bins count 0, OOB.Lower == 1, Nobs == 1 (conservation).
        RunningStatistics.Histogram firstClosed = new([0.0, 0.25, 0.5, 0.75, 1.0], false, true);
        firstClosed.Fit(0.0);
        Assert.Equal([1, 0, 0, 0], BinCounts(firstClosed));
        Assert.Equal(0, firstClosed.OutOfBoundsCounts.Lower);
        Assert.Equal(0, firstClosed.OutOfBoundsCounts.Upper);
        Assert.Equal(1, firstClosed.Nobs);
        Assert.Equal(1, SumBins(firstClosed) + firstClosed.OutOfBoundsCounts.Lower + firstClosed.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram firstOpen = new([0.0, 0.25, 0.5, 0.75, 1.0], false, false);
        firstOpen.Fit(0.0);
        Assert.Equal([0, 0, 0, 0], BinCounts(firstOpen));
        Assert.Equal(1, firstOpen.OutOfBoundsCounts.Lower);
        Assert.Equal(0, firstOpen.OutOfBoundsCounts.Upper);
        Assert.Equal(1, firstOpen.Nobs);
        Assert.Equal(1, SumBins(firstOpen) + firstOpen.OutOfBoundsCounts.Lower + firstOpen.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Boundary_LastEdgeLeftClosedEndsClosedDecides()
    {
        // leftClosed=true: the last bin is [0.75, 1.0] iff endsClosed=true. An observation
        // exactly at the LAST edge (1.0) hits the value.Equals(lastBin.Upper) branch:
        // endsClosed=true -> B3; endsClosed=false -> upper OutOfBounds counter (bins 0,
        // OOB.Upper == 1, Nobs == 1 — counted, not lost, conservation pinned).
        RunningStatistics.Histogram lastClosed = new([0.0, 0.25, 0.5, 0.75, 1.0], true, true);
        lastClosed.Fit(1.0);
        Assert.Equal([0, 0, 0, 1], BinCounts(lastClosed));
        Assert.Equal(0, lastClosed.OutOfBoundsCounts.Lower);
        Assert.Equal(0, lastClosed.OutOfBoundsCounts.Upper);
        Assert.Equal(1, lastClosed.Nobs);
        Assert.Equal(1, SumBins(lastClosed) + lastClosed.OutOfBoundsCounts.Lower + lastClosed.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram lastOpen = new([0.0, 0.25, 0.5, 0.75, 1.0], true, false);
        lastOpen.Fit(1.0);
        Assert.Equal([0, 0, 0, 0], BinCounts(lastOpen));
        Assert.Equal(0, lastOpen.OutOfBoundsCounts.Lower);
        Assert.Equal(1, lastOpen.OutOfBoundsCounts.Upper);
        Assert.Equal(1, lastOpen.Nobs);
        Assert.Equal(1, SumBins(lastOpen) + lastOpen.OutOfBoundsCounts.Lower + lastOpen.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Boundary_LastEdgeLeftOpenAlwaysInLastBin()
    {
        // leftClosed=false: the last bin's ClosedRight is hard-coded true, so an
        // observation at the last edge (1.0) is in B3 for either endsClosed — endsClosed
        // acts on the LEFT end when leftClosed=false (mirror of FirstEdgeLeftClosed...).
        RunningStatistics.Histogram bothClosed = new([0.0, 0.25, 0.5, 0.75, 1.0], false, true);
        bothClosed.Fit(1.0);
        Assert.Equal([0, 0, 0, 1], BinCounts(bothClosed));
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Lower);
        Assert.Equal(0, bothClosed.OutOfBoundsCounts.Upper);
        Assert.Equal(1, bothClosed.Nobs);
        Assert.Equal(1, SumBins(bothClosed) + bothClosed.OutOfBoundsCounts.Lower + bothClosed.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram firstOpen = new([0.0, 0.25, 0.5, 0.75, 1.0], false, false);
        firstOpen.Fit(1.0);
        Assert.Equal([0, 0, 0, 1], BinCounts(firstOpen));
        Assert.Equal(0, firstOpen.OutOfBoundsCounts.Lower);
        Assert.Equal(0, firstOpen.OutOfBoundsCounts.Upper);
        Assert.Equal(1, firstOpen.Nobs);
        Assert.Equal(1, SumBins(firstOpen) + firstOpen.OutOfBoundsCounts.Lower + firstOpen.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Boundary_OutsideFiniteEdgesCountedInOobAndNobs()
    {
        // Observations strictly OUTSIDE [first edge, last edge] with finite ends never
        // throw: the value < firstBin.Lower / value > lastBin.Upper branches of UncheckedFit
        // run BEFORE any leftClosed/endsClosed logic, so the outcome is flag-independent.
        // They are ignored by the bins but counted: OOB.Lower == 2, OOB.Upper == 3, Nobs == 5,
        // and Nobs == SumBins + OOB exactly (bin sum does not exceed Nobs; every Nobs-counted
        // observation is somewhere).
        double[] edges = [0.0, 0.25, 0.5, 0.75, 1.0];
        for (var leftClosed = 0; leftClosed < 2; leftClosed++)
        {
            for (var endsClosed = 0; endsClosed < 2; endsClosed++)
            {
                RunningStatistics.Histogram h = new(edges, leftClosed == 0, endsClosed == 0);
                h.Fit(-1.0, 2);
                h.Fit(1.5, 3);
                Assert.Equal([0, 0, 0, 0], BinCounts(h));
                Assert.Equal(2, h.OutOfBoundsCounts.Lower);
                Assert.Equal(3, h.OutOfBoundsCounts.Upper);
                Assert.Equal(5, h.Nobs);
                Assert.Equal(5, SumBins(h) + h.OutOfBoundsCounts.Lower + h.OutOfBoundsCounts.Upper);
            }
        }
    }

    [Fact]
    public void Boundary_InfiniteEndsObservationMembership()
    {
        // Edges [-Infinity, 0, 10, 100]: an observation at -Infinity equals firstBin.Lower,
        // so it is in B0 iff the first bin is closed at its Left: leftClosed=true fixes that
        // true, and leftClosed=false uses EndsClosed. -Infinity with leftClosed=false AND
        // endsClosed=false goes to OOB.Lower. +Infinity is strictly > 100 == lastBin.Upper
        // -> OOB.Upper flag-independently. Conservation pinned in every combo.
        for (var leftClosed = 0; leftClosed < 2; leftClosed++)
        {
            for (var endsClosed = 0; endsClosed < 2; endsClosed++)
            {
                RunningStatistics.Histogram h = new([double.NegativeInfinity, 0.0, 10.0, 100.0], leftClosed == 0, endsClosed == 0);
                h.Fit(double.NegativeInfinity);
                h.Fit(double.PositiveInfinity);
                if (leftClosed == 0 || endsClosed == 0)
                {
                    // first bin closed at -Infinity (leftClosed=true, or leftClosed=false
                    // with endsClosed=true) -> -Inf observation in B0 (4 edges -> 3 bins)
                    Assert.Equal([1, 0, 0], BinCounts(h));
                    Assert.Equal(0, h.OutOfBoundsCounts.Lower);
                }
                else
                {
                    // leftClosed=false, endsClosed=false: first bin (-Infinity, 0] is open
                    // at its Lower -> the -Inf observation goes to the lower OOB counter
                    Assert.Equal([0, 0, 0], BinCounts(h));
                    Assert.Equal(1, h.OutOfBoundsCounts.Lower);
                }
                Assert.Equal(1, h.OutOfBoundsCounts.Upper);
                Assert.Equal(2, h.Nobs);
                Assert.Equal(2, SumBins(h) + h.OutOfBoundsCounts.Lower + h.OutOfBoundsCounts.Upper);
            }
        }

        // Edges [0, 10, +Infinity]: an observation at +Infinity equals lastBin.Upper; it is
        // in the last bin iff ClosedRight there: leftClosed=false hard-codes it true, but
        // leftClosed=true uses endsClosed — leftClosed=true, endsClosed=false leaves +Infinity
        // in OOB.Upper (the value.Equals(lastBin.Upper) branch), NOT in a bin.
        for (var leftClosed = 0; leftClosed < 2; leftClosed++)
        {
            for (var endsClosed = 0; endsClosed < 2; endsClosed++)
            {
                RunningStatistics.Histogram h = new([0.0, 10.0, double.PositiveInfinity], leftClosed == 0, endsClosed == 0);
                h.Fit(double.PositiveInfinity);
                if (leftClosed == 0 && endsClosed == 1)
                {
                    // leftClosed=true, endsClosed=false: last bin [10, +Infinity) open at
                    // Upper -> +Inf observation in OOB.Upper
                    Assert.Equal([0, 0], BinCounts(h));
                    Assert.Equal(1, h.OutOfBoundsCounts.Upper);
                }
                else
                {
                    Assert.Equal([0, 1], BinCounts(h));
                    Assert.Equal(0, h.OutOfBoundsCounts.Upper);
                }
                Assert.Equal(0, h.OutOfBoundsCounts.Lower);
                Assert.Equal(1, h.Nobs);
                Assert.Equal(1, SumBins(h) + h.OutOfBoundsCounts.Lower + h.OutOfBoundsCounts.Upper);
            }
        }
    }

    // Commit 24: null iterable Fit contract — Histogram has no iterable Fit override;
    // the call resolves to the base RunningStatisticBase.Fit(IEnumerable<double>).
    [Fact]
    public void Fit_RejectsNullIterable()
    {
        RunningStatistics.Histogram h = new([0.0, 1.0, 2.0]);
        Assert.Throws<ArgumentNullException>(() => h.Fit((IEnumerable<double>?)null));
        Assert.Equal(0, h.Nobs);
    }
}