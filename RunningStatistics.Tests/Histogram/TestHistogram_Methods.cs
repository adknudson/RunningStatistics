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
    
    // Edge-validation guards in the constructor. Scenarios that silently
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
    public void Fit_OutOfBoundsValueIncreasesOobCount()
    {
        RunningStatistics.Histogram h = new(_edges, false, false);
        h.Fit(200);
        Assert.Equal(1, h.OutOfBoundsCounts.Upper);
    }

    [Fact]
    public void Reset_ClearsAllCounts()
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
    public void FreshHistogram_NobsAndBinsAreZero()
    {
        // (-inf, 0], (0, 10], (10, 100]
        RunningStatistics.Histogram h = new(_edges, false, false);

        Assert.Equal(0, h.Nobs);

        foreach (var bin in h)
        {
            Assert.Equal(0, bin.Nobs);
        }
    }

    // CloneEmpty's interface contract for parameterized statistics: the clone keeps the same
    // internal parameters but zero observations. For Histogram the parameters are the constructor's
    // edges and closure flags — a clone that lost them would bin later observations differently
    // (and would take the wrong branch of Merge).
    [Fact]
    public void CloneEmpty_PreservesEdgesAndClosureFlags()
    {
        // (-inf, 0], (0, 10], (10, 100]
        RunningStatistics.Histogram hist = new(_edges, leftClosed: false, endsClosed: false);
        hist.Fit(-1);
        hist.Fit(5);
        hist.Fit(1000);

        var clone = hist.CloneEmpty();

        // configuration survives, observations do not
        Assert.Equal(_edges, clone.Edges);
        Assert.False(clone.LeftClosed);
        Assert.False(clone.EndsClosed);
        Assert.Equal(0, clone.Nobs);

        // the clone bins identically: same values -> same bins, same OOB side
        clone.Fit(-1);
        clone.Fit(5);
        clone.Fit(1000);
        // -1 lands in the (-inf,0] bin, 5 in the (0,10] bin; 1000 is above the top edge: OOB
        Assert.Equal([1, 1, 0], BinCounts(clone));
        Assert.Equal(0, clone.OutOfBoundsCounts.Lower);
        Assert.Equal(1, clone.OutOfBoundsCounts.Upper);
        Assert.Equal(3, clone.Nobs);
    }

    // The accumulate pattern from user code (total = stat.CloneEmpty(); total.Merge(chunk)):
    // the configuration-carrying clone must land on Merge's same-bins branch and sum bin counts
    // directly — the midpoint fallback would not produce these counts.
    [Fact]
    public void CloneEmpty_AccumulateMergeSumsBinsDirectly()
    {
        RunningStatistics.Histogram chunk1 = new(_edges, leftClosed: false, endsClosed: false);
        chunk1.Fit(-1, 2);
        chunk1.Fit(5);
        chunk1.Fit(1000);

        RunningStatistics.Histogram chunk2 = new(_edges, leftClosed: false, endsClosed: false);
        chunk2.Fit(-5);
        chunk2.Fit(15, 3);

        var total = chunk1.CloneEmpty();
        Assert.Equal(_edges, total.Edges);
        Assert.False(total.LeftClosed);
        Assert.False(total.EndsClosed);
        Assert.Equal(0, total.Nobs);

        total.Merge(chunk1);
        total.Merge(chunk2);

        // bins summed elementwise, OOB counters summed, nothing lost
        Assert.Equal([3, 1, 3], BinCounts(total));
        Assert.Equal(0, total.OutOfBoundsCounts.Lower);
        Assert.Equal(1, total.OutOfBoundsCounts.Upper);
        Assert.Equal(8, total.Nobs);

        // equivalence with a single histogram fitted all data at once
        RunningStatistics.Histogram all = new(_edges, leftClosed: false, endsClosed: false);
        all.Fit(-1, 2);
        all.Fit(5);
        all.Fit(1000);
        all.Fit(-5);
        all.Fit(15, 3);
        Assert.Equal(BinCounts(total), BinCounts(all));
        Assert.Equal(total.Nobs, all.Nobs);
        Assert.Equal(total.OutOfBoundsCounts.Lower, all.OutOfBoundsCounts.Lower);
        Assert.Equal(total.OutOfBoundsCounts.Upper, all.OutOfBoundsCounts.Upper);
    }

    // Reset must clear counters while leaving the bin configuration alone: edges, closure flags
    // and the bin list survive, OOB counters clear (not just the bins), and refitting the
    // cleared histogram reproduces the exact bin membership it had before the reset.
    [Fact]
    public void Reset_PreservesEdgesRefitsSameBins()
    {
        // [0, 10), [10, 100]
        RunningStatistics.Histogram h = new([0.0, 10.0, 100.0]);
        h.Fit(-7, 2);
        h.Fit(5);
        h.Fit(50);
        h.Fit(1000, 4);

        // pre-reset snapshot
        Assert.Equal([1, 1], BinCounts(h));
        Assert.Equal(2, h.OutOfBoundsCounts.Lower);
        Assert.Equal(4, h.OutOfBoundsCounts.Upper);
        Assert.Equal(8, h.Nobs);

        h.Reset();
        Assert.Equal(0, h.Nobs);
        Assert.Equal([0, 0], BinCounts(h));
        Assert.Equal(0, h.OutOfBoundsCounts.Lower);
        Assert.Equal(0, h.OutOfBoundsCounts.Upper);

        // the bins are still there, still configured, and bin identically after reuse
        h.Fit(-7, 2);
        h.Fit(5);
        h.Fit(50);
        h.Fit(1000, 4);
        Assert.Equal([0.0, 10.0, 100.0], h.Edges);
        Assert.True(h.LeftClosed);
        Assert.True(h.EndsClosed);
        Assert.Equal([1, 1], BinCounts(h));
        Assert.Equal(2, h.OutOfBoundsCounts.Lower);
        Assert.Equal(4, h.OutOfBoundsCounts.Upper);
        Assert.Equal(8, h.Nobs);
    }

    // DECISION NOTE — the mismatched-edges fallback of Histogram.Merge is PINNED AS-IS.
    // When BinsAreMatching is false (Bins.Equals compares bounds AND closure flags, counts
    // ignored), Merge does NOT throw: it replays each source bin's count at that bin's
    // Midpoint — an approximation of where the observations actually were — and a midpoint
    // outside the receiver falls into its OutOfBounds counters instead of a bin. Turning
    // this into a throw (or a finer redistribution) would change released behavior and
    // would need the staged correctness + benchmark gate from AGENTS.md. Cross-statistic
    // contrast, for the record: EmpiricalCdf.Merge THROWS on a NumBins mismatch while
    // Histogram.Merge approximates. The contrast for SAME edges AND SAME closure flags
    // (the bin-summing branch) is already pinned by Merge_MatchingEdgesSumsBinsDirectly
    // and CloneEmpty_AccumulateMergeSumsBinsDirectly — not duplicated below.
    // Mismatched edge sets: no throw, no complaint — each source bin's count is refit at
    // that bin's midpoint into the receiver. Receiver [0,10), [10,20] holds fit 5 (bin 0);
    // source [0,5), [5,10), [10,15), [15,20] has one observation per bin (fits 1, 6, 11, 16),
    // midpoints 2.5, 7.5, 12.5, 17.5. Refit: 2.5 and 7.5 -> receiver bin 0 [0,10); 12.5 and
    // 17.5 -> receiver bin 1 [10,20]. Final bins [3, 2], Nobs 1+4, OOB untouched (0, 0).
    [Fact]
    public void Merge_MismatchedEdgesRefitsBinMidpoints()
    {
        RunningStatistics.Histogram receiver = new([0.0, 10.0, 20.0]);
        receiver.Fit(5);

        RunningStatistics.Histogram source = new([0.0, 5.0, 10.0, 15.0, 20.0]);
        source.Fit(1);
        source.Fit(6);
        source.Fit(11);
        source.Fit(16);

        receiver.Merge(source);

        Assert.Equal([3, 2], BinCounts(receiver));
        Assert.Equal(5, receiver.Nobs);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Upper);
    }

    // The midpoint refit reuses UncheckedFit, so a midpoint outside the receiver is routed
    // to the OutOfBounds counters exactly like a normal Fit would. (a) Receiver [10,12] with
    // fit 11; source [0,100] carries Fit(50, 4); the single source midpoint 50 > 12 -> upper
    // OOB +4, Nobs 1+4, bins unchanged [1]. (b) Source edges [-inf, 0, 10, +inf] (bins [-inf,0),
    // [0,10), [10,+inf]) refit into receiver [0,10), [10,20]: HistogramBin.Midpoint returns the
    // infinite bound when one bound is infinite, so the infinite-edge bins refit at -Infinity
    // (-> lower OOB) and +Infinity (-> upper OOB); only the middle bin's midpoint 5 lands in a
    // bin. Fit guards NotNaN only, so the infinities are accepted, not thrown.
    [Fact]
    public void Merge_MismatchedEdgesMidpointsFallOutOfBounds()
    {
        RunningStatistics.Histogram narrow = new([10.0, 12.0]);
        narrow.Fit(11);

        RunningStatistics.Histogram wide = new([0.0, 100.0]);
        wide.Fit(50, 4);

        narrow.Merge(wide);

        Assert.Equal([1], BinCounts(narrow));
        Assert.Equal(5, narrow.Nobs);
        Assert.Equal(0, narrow.OutOfBoundsCounts.Lower);
        Assert.Equal(4, narrow.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram receiver = new([0.0, 10.0, 20.0]);
        receiver.Fit(5);

        RunningStatistics.Histogram infinite = new([double.NegativeInfinity, 0.0, 10.0, double.PositiveInfinity]);
        infinite.Fit(-1);
        infinite.Fit(5);
        infinite.Fit(15);

        receiver.Merge(infinite);

        // midpoint 5 refits into bin 0; -Infinity -> lower OOB, +Infinity -> upper OOB
        Assert.Equal([2, 0], BinCounts(receiver));
        Assert.Equal(4, receiver.Nobs);
        Assert.Equal(1, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(1, receiver.OutOfBoundsCounts.Upper);
    }

    // BinsAreMatching compares bounds AND closure flags, so identical edges with different
    // (leftClosed, endsClosed) take the refit branch, not the bin-summing branch. Receiver
    // [0,10), [10,20] (defaults) vs source (0,10], (10,20]: the source's fits are both at
    // 10.0, which with leftClosed=false lands in the source's FIRST bin (0,10]. The direct
    // sum would have put those 2 into receiver bin 1 (10,20]-positioned); the refit branch
    // instead replays 2 observations at the first bin's midpoint 5 -> receiver bin 0. Bins
    // [2, 0] is only reachable via the midpoint replay.
    [Fact]
    public void Merge_SameEdgesDifferentClosureFlagsRefitsMidpoints()
    {
        RunningStatistics.Histogram receiver = new([0.0, 10.0, 20.0], leftClosed: true, endsClosed: true);

        RunningStatistics.Histogram source = new([0.0, 10.0, 20.0], leftClosed: false, endsClosed: false);
        source.Fit(10.0, 2);

        receiver.Merge(source);

        Assert.Equal([2, 0], BinCounts(receiver));
        Assert.Equal(2, receiver.Nobs);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Upper);
    }

    // Merging two stats fitted with the first and second halves of the same data must match
    // a stat fitted with all of the data at once.
    [Fact]
    public void Merge_PartsEqualsAll()
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

    // Which bins values land in, for the bin-closure configuration given by the constructor
    // arguments (see the edge notation in the body).
    [Fact]
    public void Fit_LeftClosedBins()
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

    // Which bins values land in, for the bin-closure configuration given by the constructor
    // arguments (see the edge notation in the body).
    [Fact]
    public void Fit_LeftOpenBins()
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

    // Which bins values land in, for the bin-closure configuration given by the constructor
    // arguments (see the edge notation in the body).
    [Fact]
    public void Fit_RightClosedBins()
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

    // Which bins values land in, for the bin-closure configuration given by the constructor
    // arguments (see the edge notation in the body).
    [Fact]
    public void Fit_RightOpenBins()
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

    // The static Type.Merge(a, b) returns a new stat; a and b keep their own state.
    [Fact]
    public void StaticMerge_DoesNotAffectOriginals()
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
    // PIN — observation/bin MEMBERSHIP at edges for every
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

    // Why this matters: the merge-doubling facts (AbstractRunningStatsTest.MergeSelf_NobsOverflowThrows)
    // pin Nobs at 2^62 scale, but nothing pinned the PER-BIN counters there. Fit(value, count)
    // accumulates a bin in O(1) (HistogramBin.Increment: Nobs += count, unchecked) exactly like
    // Histogram's own checked Nobs += count, so one histogram can hold per-bin counts near 2^61
    // while Nobs stays representable: the bin counters and their formatting must neither throw nor
    // silently lose digits. Pinned here (edges [0,1), [1,2), [2,3]; fits 0.5 and 1.5 at 1L << 61
    // each): Nobs == 2^62 == 4611686018427387904, the two hit bins read back exactly 2^61 ==
    // 2305843009213693952, ToString succeeds, repeated calls give the SAME string, and the digit
    // runs survive formatting. Separator-tolerance: the base ToString and HistogramBin.ToString
    // both print {Nobs:N0}, which groups thousands with the locale separator, so the digit
    // assertions below run over DigitsOf(text), the same text with every non-digit dropped.
    [Fact]
    public void ToString_HugeBinCountsFormatsStably()
    {
        RunningStatistics.Histogram hist = new([0.0, 1.0, 2.0, 3.0]);
        hist.Fit(0.5, 1L << 61);
        hist.Fit(1.5, 1L << 61);

        // Nobs is the checked sum of the two weighted fits: 2^61 + 2^61 == 2^62, no overflow
        Assert.Equal(1L << 62, hist.Nobs);

        // formatting succeeds and is stable across repeated calls (no lazily recomputed/
        // drifting rendering of the huge counters)
        var first = hist.ToString();
        var again = hist.ToString();
        Assert.Equal(first, again);

        // the whole Nobs digit run appears in the rendered string
        Assert.True(DigitsOf(first).Contains("4611686018427387904"), first);

        // the bins themselves: the two hit bins read the huge counts exactly, the third stays
        // zero, and each bin's own ToString carries the huge count's digits
        var binIndex = 0;
        foreach (var bin in hist)
        {
            var binText = bin.ToString();
            if (binIndex < 2)
            {
                Assert.Equal(1L << 61, bin.Nobs);
                Assert.True(DigitsOf(binText).Contains("2305843009213693952"), binText);
            }
            else
            {
                Assert.Equal(0L, bin.Nobs);
            }

            binIndex++;
        }

        Assert.Equal(3, binIndex);

        // conservation at the huge scale: Nobs == sum of bins + OOB, and the untouched
        // out-of-bounds counters still read zero (nothing leaked out of the bins)
        Assert.Equal(0, hist.OutOfBoundsCounts.Lower);
        Assert.Equal(0, hist.OutOfBoundsCounts.Upper);
        Assert.Equal(hist.Nobs, SumBins(hist));
    }

    private static string DigitsOf(string text)
    {
        var pieces = new List<string>();
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsDigit(text[i])) pieces.Add(text.Substring(i, 1));
        }

        return string.Join("", pieces);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}