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

    // These facts test the constructor's checks on the edge list. The
    // scenarios below built histograms the old lenient behavior allowed: a
    // NaN edge in any position, edges out of order, and repeated edges. The
    // constructor used to sort out-of-order edges for the caller. It now
    // requires every edge to be a real number or an infinity, and the edges
    // to strictly increase. Each scenario below must therefore throw.
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
    // internal parameters but zero observations. For Histogram the parameters are the
    // constructor's edges and closure flags. A clone that lost them would bin later
    // observations differently and would take the wrong branch of Merge.
    [Fact]
    public void CloneEmpty_PreservesEdgesAndClosureFlags()
    {
        // The bins run from negative infinity to 0, 0 to 10, and 10 to 100. The
        // left ends are open, so each bin excludes its lower edge and
        // includes its upper edge.
        RunningStatistics.Histogram hist = new(_edges, leftClosed: false, endsClosed: false);
        hist.Fit(-1);
        hist.Fit(5);
        hist.Fit(1000);

        var clone = hist.CloneEmpty();

        // A reset keeps the edge configuration while discarding the observations.
        Assert.Equal(_edges, clone.Edges);
        Assert.False(clone.LeftClosed);
        Assert.False(clone.EndsClosed);
        Assert.Equal(0, clone.Nobs);

        // The clone sorts values into the bins the same way: identical values go to the
        // same bins and out-of-range values land on the same side.
        clone.Fit(-1);
        clone.Fit(5);
        clone.Fit(1000);
        // A value of -1 sits in the bin at or below 0. A value of 5 sits in the
        // bin above 0 up to 10. A value of 1000 is past the last edge, so it
        // counts as out of bounds.
        Assert.Equal([1, 1, 0], BinCounts(clone));
        Assert.Equal(0, clone.OutOfBoundsCounts.Lower);
        Assert.Equal(1, clone.OutOfBoundsCounts.Upper);
        Assert.Equal(3, clone.Nobs);
    }

    // The accumulation pattern user code writes: clone an empty copy carrying the
    // configuration, then merge each chunk into it. The clone keeps the edges, so the
    // merge must take the same-bins branch that adds bin counts directly. The fallback
    // that replays bins at their midpoints would not produce these counts.
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

        // Each bin now holds the sum of the two chunks' counts. The out-of-bounds
        // counters sum the same way, and the merge dropped nothing.
        Assert.Equal([3, 1, 3], BinCounts(total));
        Assert.Equal(0, total.OutOfBoundsCounts.Lower);
        Assert.Equal(1, total.OutOfBoundsCounts.Upper);
        Assert.Equal(8, total.Nobs);

        // The accumulated result must match one histogram fitted with all the data at once.
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

    // Reset must clear counters while leaving the bin configuration alone. The
    // edges, the closure flags and the bin list survive. Reset clears the
    // out-of-bounds counters as well, not just the bins. Refitting the cleared
    // histogram reproduces the exact bin membership it had before the reset.
    [Fact]
    public void Reset_PreservesEdgesRefitsSameBins()
    {
        // The bins here are 0 up to but not including 10, and 10 up to and including 100.
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

    // Policy note (issue #17): the merge now follows the EmpiricalCdf policy.
    // When the two histograms' bins differ, in their edges or their closure
    // flags, the merge throws instead of approximating the source at bin
    // midpoints. The shape check runs before any state change, so a rejected
    // merge leaves the receiver's bins, counters and total untouched. The
    // same-bins branch, where the code adds counts directly, is pinned by
    // Merge_MatchingEdgesSumsBinsDirectly and CloneEmpty_AccumulateMergeSumsBinsDirectly,
    // so it is not repeated below.
    //
    // The scenario pinned here: the receiver covers 0 up to but not including
    // 10, and 10 to 20 including 10, and holds one observation, 5. The source
    // uses four bins of width 5. Five edges against two, so the shapes differ
    // and the merge must throw with an ArgumentException. The receiver must
    // still read its one observation, its original counts and zero counters.
    [Fact]
    public void Merge_MismatchedEdgesThrows()
    {
        RunningStatistics.Histogram receiver = new([0.0, 10.0, 20.0]);
        receiver.Fit(5);

        RunningStatistics.Histogram source = new([0.0, 5.0, 10.0, 15.0, 20.0]);
        source.Fit(1);
        source.Fit(6);
        source.Fit(11);
        source.Fit(16);

        Assert.Throws<ArgumentException>(
            () => receiver.Merge(source));

        // A rejected merge must leave the receiver exactly as it was.
        Assert.Equal([1, 0], BinCounts(receiver));
        Assert.Equal(1, receiver.Nobs);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Upper);
    }

    // The shape check must reject equal bin counts whose edges differ, and it
    // must run before the out-of-bounds counters fold in. Scenario a: the
    // receiver covers 10 to 12 and holds an observation at 11. The source
    // covers 0 to 100 and holds four observations at 50. Both have one bin, so
    // only an edge comparison rejects the merge. The merge throws, and the
    // receiver's counter must still read zero, not four: the old code folded
    // the source's counters in before choosing the branch, and that leak is
    // what this fact now rules out.
    //
    // Scenario b: the source has edges at negative infinity, 0, 10 and
    // positive infinity, three bins against the receiver's two, so the merge
    // also throws, and the receiver keeps its single observation.
    [Fact]
    public void Merge_MismatchedEdgesSameBinCountThrowsReceiverUnchanged()
    {
        RunningStatistics.Histogram narrow = new([10.0, 12.0]);
        narrow.Fit(11);

        RunningStatistics.Histogram wide = new([0.0, 100.0]);
        wide.Fit(50, 4);

        Assert.Throws<ArgumentException>(
            () => narrow.Merge(wide));

        Assert.Equal([1], BinCounts(narrow));
        Assert.Equal(1, narrow.Nobs);
        Assert.Equal(0, narrow.OutOfBoundsCounts.Lower);
        Assert.Equal(0, narrow.OutOfBoundsCounts.Upper);

        RunningStatistics.Histogram receiver = new([0.0, 10.0, 20.0]);
        receiver.Fit(5);

        RunningStatistics.Histogram infinite = new([double.NegativeInfinity, 0.0, 10.0, double.PositiveInfinity]);
        infinite.Fit(-1);
        infinite.Fit(5);
        infinite.Fit(15);

        Assert.Throws<ArgumentException>(
            () => receiver.Merge(infinite));

        Assert.Equal([1, 0], BinCounts(receiver));
        Assert.Equal(1, receiver.Nobs);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Upper);
    }

    // Matching bins compares both the edge values and the closure flags. Two
    // histograms with identical edges but different closure settings
    // therefore take the throw, not the direct sum. The receiver covers 0 up
    // to but not including 10, and 10 to 20 including 10, with the default
    // flags. The source uses the same edges closed on the right instead. Its
    // two fits of 10 sit in the source's first bin because its left end is
    // open, where the receiver would put them in its second. Under the new
    // policy this merge is refused outright, so the receiver keeps its empty
    // bins and zero total.
    [Fact]
    public void Merge_SameEdgesDifferentClosureFlagsThrows()
    {
        RunningStatistics.Histogram receiver = new([0.0, 10.0, 20.0], leftClosed: true, endsClosed: true);

        RunningStatistics.Histogram source = new([0.0, 10.0, 20.0], leftClosed: false, endsClosed: false);
        source.Fit(10.0, 2);

        Assert.Throws<ArgumentException>(
            () => receiver.Merge(source));

        Assert.Equal([0, 0], BinCounts(receiver));
        Assert.Equal(0, receiver.Nobs);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(0, receiver.OutOfBoundsCounts.Upper);
    }

    // The fact merges the data as two stats, one for the first half and one for
    // the second half. Their merge must match a stat fitted with all of the
    // data at once.
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

    // The static Merge form returns a new statistic and leaves both arguments unchanged.
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
    // These facts pin which bin receives an observation sitting exactly on an edge.
    // They cover every combination of the two closure flags, and pin the behavior
    // exactly as the fitting and the bin-containment check behave today. Each
    // scenario reads back the observation count of every bin in index order, so
    // the exact bin that gets each edge observation is pinned. Every scenario
    // also checks the conservation identity. The total count must equal the sum
    // of the bins plus the two out-of-bounds counters. No bin double-counts an
    // observation, and no observation counted in the total is counted nowhere.
    //
    // The bullets below state how the flags decide membership. Edges run from the
    // first to the last.
    // - An observation on an interior edge goes to the bin above it when the bins
    //   are closed on the left. It goes to the bin below it when the bins are
    //   closed on the right.
    // - The first edge belongs to the first bin when the left end is closed. When
    //   the left end is open, it belongs to the first bin only if the ends are
    //   closed. Otherwise it goes to the lower out-of-bounds counter.
    // - The last edge belongs to the last bin when the right end is closed. When
    //   the left end is closed instead, it belongs to the last bin only if the
    //   ends are closed. Otherwise it goes to the upper out-of-bounds counter.
    // - A value strictly below the first edge or strictly above the last edge goes
    //   to the matching out-of-bounds counter regardless of the flags. The
    //   fitting keeps such a value in the total but not in any bin.
    // - A value of negative infinity at a first edge of negative infinity is in
    //   the first bin exactly when that bin's left end is closed. The closed-left
    //   flag forces that, and the open-left flag takes it from the ends flag.
    //   A value of positive infinity at a last edge of positive infinity is in
    //   the last bin exactly when that bin's right end is closed. The open-left
    //   flag always closes it, and the closed-left flag closes it only when the
    //   ends are closed.
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
        // With the left end closed, an interior bin covers its lower edge. An
        // observation sitting exactly on an interior edge belongs to the bin above
        // it. For example, observations at 0.25, 0.5 and 0.75 land in bins 1, 2
        // and 3. The ends flag never moves an interior edge, so both of its
        // values give the same counts.
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
        // With the right end closed, an interior bin covers its upper edge. An
        // observation sitting exactly on an interior edge belongs to the bin below
        // it, one bin lower than in the closed-left case. The ends flag changes
        // nothing here.
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
        // An observation sits exactly on the first edge, 0.0, with the left end
        // closed. The first bin always closes its left end when the flag asks for
        // a closed left. The observation is therefore in the first bin for either
        // value of the ends flag. It is counted once in the bins and once in the
        // total.
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
        // With the left end open, the first bin closes its left end only when the
        // ends flag asks for it. With the ends open too, an observation on the
        // first edge sits at an open end. The fitting neither drops it silently
        // nor throws it. A dedicated check in the fitting notices a value equal
        // to the first bin's lower edge. The check routes the observation to the
        // lower out-of-bounds counter. The bins count nothing, the counter holds
        // one, and the total holds one.
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
        // With the left end closed, the last bin covers its upper edge only when the
        // ends flag asks for it. An observation exactly on the last edge, 1.0, meets
        // the check for a value equal to the last bin's upper edge. With the ends
        // closed it goes to the last bin, and with the ends open it goes to the
        // upper out-of-bounds counter instead. The bins stay at zero, the counter
        // holds one and the total holds one, so the observation is counted, not
        // lost.
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
        // With the right end closed, the last bin always closes its right end. An
        // observation exactly on the last edge is in the last bin for either value
        // of the ends flag. With the left end open, the ends flag acts on the left
        // side instead. This case mirrors the first-edge case above.
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
        // Observations strictly outside the outer edges, with finite ends, never
        // throw. The checks for a value below the first bin's lower edge and above
        // the last bin's upper edge run before any closure-flag logic. The flags
        // therefore make no difference. The bins ignore them while the counters
        // hold two below and three above. The total is five. The total equals the
        // bins plus the counters exactly, so no bin over-counts and every counted
        // observation lives somewhere.
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
        // Edges at negative infinity, 0, 10 and 100: an observation of negative infinity
        // equals the first bin's lower edge. It belongs to the first bin exactly when
        // that bin's left end is closed. The closed-left flag forces that, and the
        // open-left flag takes it from the ends flag. Negative infinity with both
        // flags open goes to the lower out-of-bounds counter. Positive infinity sits
        // strictly above the last edge of 100, so it takes the upper counter
        // regardless of the flags. The conservation identity holds in every
        // combination.
        for (var leftClosed = 0; leftClosed < 2; leftClosed++)
        {
            for (var endsClosed = 0; endsClosed < 2; endsClosed++)
            {
                RunningStatistics.Histogram h = new([double.NegativeInfinity, 0.0, 10.0, 100.0], leftClosed == 0, endsClosed == 0);
                h.Fit(double.NegativeInfinity);
                h.Fit(double.PositiveInfinity);
                if (leftClosed == 0 || endsClosed == 0)
                {
                    // The first bin can close at negative infinity in two ways. The closed-left
                    // flag forces that, and the open-left flag paired with closed ends does
                    // too. In either case the observation of negative infinity belongs to the
                    // first bin. Four edges make three bins.
                    Assert.Equal([1, 0, 0], BinCounts(h));
                    Assert.Equal(0, h.OutOfBoundsCounts.Lower);
                }
                else
                {
                    // With both flags open, the first bin starts above negative infinity, so
                    // the observation of negative infinity falls to the lower out-of-bounds
                    // counter.
                    Assert.Equal([0, 0, 0], BinCounts(h));
                    Assert.Equal(1, h.OutOfBoundsCounts.Lower);
                }
                Assert.Equal(1, h.OutOfBoundsCounts.Upper);
                Assert.Equal(2, h.Nobs);
                Assert.Equal(2, SumBins(h) + h.OutOfBoundsCounts.Lower + h.OutOfBoundsCounts.Upper);
            }
        }

        // Edges at 0, 10 and positive infinity: an observation of positive infinity
        // equals the last bin's upper edge. It belongs to the last bin exactly when
        // that bin's right end is closed. The open-left flag forces that, and the
        // closed-left flag takes it from the ends flag. With the closed left and
        // open ends, positive infinity goes to the upper out-of-bounds counter
        // instead of a bin.
        for (var leftClosed = 0; leftClosed < 2; leftClosed++)
        {
            for (var endsClosed = 0; endsClosed < 2; endsClosed++)
            {
                RunningStatistics.Histogram h = new([0.0, 10.0, double.PositiveInfinity], leftClosed == 0, endsClosed == 0);
                h.Fit(double.PositiveInfinity);
                if (leftClosed == 0 && endsClosed == 1)
                {
                    // With the closed left and open ends, the last bin stops short of
                    // positive infinity, so the observation there goes to the upper
                    // out-of-bounds counter.
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

    // This matters because the self-merge facts in the abstract test pin the total
    // count at the scale of 2 to the 62nd power. Nothing pinned the counters inside
    // the bins at that scale. A weighted fit raises a bin's count in one step, an
    // unchecked addition, while the histogram's own total is checked. One
    // histogram can therefore hold per-bin counts near 2 to the 61st power while
    // its total stays representable. The bin counters and their text form must
    // then neither throw nor lose digits.
    //
    // The fact pins edges 0, 1, 2 and 3, with 0.5 and 1.5 each fitted at a count of
    // 2 to the 61st power. The total comes to 2 to the 62nd power, exactly
    // 4,611,686,018,427,387,904. The two hit bins read back exactly 2 to the 61st
    // power, 2,305,843,009,213,693,952, apiece. The text form succeeds, gives
    // the same string on repeated calls, and the long digit runs survive
    // formatting. The text form groups thousands with the locale's separator,
    // so the digit checks below compare the text with every non-digit dropped.
    // The test shares a helper for that comparison.
    [Fact]
    public void ToString_HugeBinCountsFormatsStably()
    {
        RunningStatistics.Histogram hist = new([0.0, 1.0, 2.0, 3.0]);
        hist.Fit(0.5, 1L << 61);
        hist.Fit(1.5, 1L << 61);

        // The total is the checked sum of the two weighted fits. Two times 2 to the
        // 61st power makes 2 to the 62nd power, comfortably inside the whole-number
        // range.
        Assert.Equal(1L << 62, hist.Nobs);

        // The text form renders the huge counters successfully and gives the same text on
        // every call, with no lazily recomputed rendering drifting between calls.
        var first = hist.ToString();
        var again = hist.ToString();
        Assert.Equal(first, again);

        // The whole Nobs digit run appears in the rendered string.
        Assert.True(DigitsOf(first).Contains("4611686018427387904"), first);

        // The test then checks the bins themselves. The two hit bins read the huge
        // counts exactly, the third stays zero, and each bin's own ToString
        // carries the digits of the huge count.
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

        // The conservation identity holds at this scale too. The total equals the bins
        // plus the two untouched out-of-bounds counters, which still read zero.
        // Nothing leaked out of the bins.
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

    // Edge accuracy at the scales where one representable step is the whole story.
    // The three edges are consecutive doubles, so bin boundaries meet at unit-in-the-
    // last-place distances. The scales reach 1e18, past 2^53, where that step is 128
    // wide. Each observation must land on the closed side of the edge that owns it,
    // and the counts must be conserved: Nobs equals the bins plus both out-of-bounds
    // counters. The audit run of 2026-11-19 found no disagreement and staged no
    // candidate. A red fact means the routing drifted; fix the routing, do not move
    // the edges closer together.
    [Fact]
    public void Gate_HistogramAdjacentEdgesSplitAtEveryScale()
    {
        foreach (var scale in new[] { 1.0, 1e15, 1e18 })
        {
            var e0 = scale;
            var e1 = NextAfter(e0, 1);
            var e2 = NextAfter(e1, 1);
            foreach (var leftClosed in new[] { true, false })
            {
                foreach (var endsClosed in new[] { true, false })
                {
                    var hist = new RunningStatistics.Histogram([e0, e1, e2], leftClosed, endsClosed);
                    hist.Fit(e0, 1);
                    hist.Fit(e1, 2);
                    hist.Fit(e2, 3);

                    // The first bin covers its lower edge when LeftClosed picks the left
                    // side, and when right-closed bins let the ends switch instead.
                    var firstSideClosed = leftClosed || endsClosed;
                    // The last bin covers its upper edge when LeftClosed picks the right
                    // side, which it always does, except that right-closed bins already
                    // close it from the inside.
                    var lastSideClosed = !leftClosed || endsClosed;
                    // The interior edge goes to the bin above with a closed left, and
                    // to the bin below with a closed right.
                    var interiorToFirst = leftClosed ? 0 : 2;
                    var interiorToLast = leftClosed ? 2 : 0;
                    var expectedFirst = (firstSideClosed ? 1 : 0) + interiorToFirst;
                    var expectedLast = interiorToLast + (lastSideClosed ? 3 : 0);

                    Assert.Equal([expectedFirst, expectedLast], BinCounts(hist));
                    Assert.Equal(6, hist.Nobs);
                    Assert.Equal(firstSideClosed ? 0 : 1, hist.OutOfBoundsCounts.Lower);
                    Assert.Equal(lastSideClosed ? 0 : 3, hist.OutOfBoundsCounts.Upper);
                    Assert.Equal(6, SumBins(hist) + hist.OutOfBoundsCounts.Lower + hist.OutOfBoundsCounts.Upper);
                }
            }
        }
    }

    // The weighted overload must route every value to the same bin the per-
    // observation loop picks, and its counts must be the exact multiple of the
    // weights. The dataset puts values exactly on every edge, just inside and just
    // outside the outer edges, and at interior points, over all four closure shapes.
    // The audit run of 2026-11-19 found no drift. A red fact means the weighted
    // route no longer matches the loop; fix the route, do not resample the data.
    [Fact]
    public void Gate_HistogramWeightedFitEqualsLoopAcrossEdges()
    {
        var values = new[] { 0.0, 0.25, 0.5, 0.75, 1.0, 0.3, 0.6, -0.25, 1.25 };
        foreach (var leftClosed in new[] { true, false })
        {
            foreach (var endsClosed in new[] { true, false })
            {
                var loop = new RunningStatistics.Histogram([0.0, 0.25, 0.5, 0.75, 1.0], leftClosed, endsClosed);
                var batch = new RunningStatistics.Histogram([0.0, 0.25, 0.5, 0.75, 1.0], leftClosed, endsClosed);
                var weighted = new RunningStatistics.Histogram([0.0, 0.25, 0.5, 0.75, 1.0], leftClosed, endsClosed);
                for (var i = 0; i < values.Length; i++)
                {
                    loop.Fit(values[i]);
                    batch.Fit(values[i], 1);
                    weighted.Fit(values[i], 5);
                }

                Assert.Equal(BinCounts(loop), BinCounts(batch));
                Assert.Equal(loop.Nobs, batch.Nobs);
                Assert.Equal(loop.OutOfBoundsCounts.Lower, batch.OutOfBoundsCounts.Lower);
                Assert.Equal(loop.OutOfBoundsCounts.Upper, batch.OutOfBoundsCounts.Upper);

                var loopCounts = BinCounts(loop);
                var quintupled = new int[loopCounts.Length];
                for (var k = 0; k < loopCounts.Length; k++) quintupled[k] = 5 * loopCounts[k];
                Assert.Equal(quintupled, BinCounts(weighted));
                Assert.Equal(5L * loop.Nobs, weighted.Nobs);
                Assert.Equal(weighted.Nobs,
                             SumBins(weighted) + weighted.OutOfBoundsCounts.Lower + weighted.OutOfBoundsCounts.Upper);
            }
        }
    }

    // Merge policy across shapes that do not match (issue #17). A mismatched
    // merge now throws instead of refitting the source's bin counts at their
    // midpoints. The rejection must leave the receiver alone: its bins, its
    // out-of-bounds counters and its total must all still read the pre-merge
    // values, and the count identity Nobs = bins + counters must still hold
    // after the rejection. The audit run of 2026-11-19 found one leak: the
    // old code folded the source's out-of-bounds counters into the receiver
    // and its total before choosing the branch, so a rejected merge counted
    // observations the receiver's bins never held. Histogram.Merge now checks
    // the shape first, and this gate pins the refusal and the identity.
    // A same-shape merge must still add bins directly, with exact per-bin
    // sums and exact out-of-bounds sums.
    [Fact]
    public void Gate_HistogramMergeRejectsMismatchedShapesWithoutLeak()
    {
        var dataA = new[] { 0.0, 0.5, 0.5, 1.0, 1.5, 2.0 };
        var dataB = new[] { 0.4, 1.2, 1.6, 2.4, 2.5 };

        var a = new RunningStatistics.Histogram([0.0, 1.0, 2.0], true, true);
        for (var i = 0; i < dataA.Length; i++) a.Fit(dataA[i]);

        var b = new RunningStatistics.Histogram([0.5, 1.5, 2.5], false, false);
        for (var i = 0; i < dataB.Length; i++) b.Fit(dataB[i]);

        // Two bins against two, but the edges differ, so the merge must throw.
        var total = a.Clone();
        Assert.Throws<ArgumentException>(
            () => total.Merge(b));

        // The rejected merge must not have touched the clone: same counts,
        // same counters, same total, and the identity still holds.
        Assert.Equal(BinCounts(a), BinCounts(total));
        Assert.Equal(a.OutOfBoundsCounts.Lower, total.OutOfBoundsCounts.Lower);
        Assert.Equal(a.OutOfBoundsCounts.Upper, total.OutOfBoundsCounts.Upper);
        Assert.Equal(a.Nobs, total.Nobs);
        Assert.Equal(total.Nobs,
                     SumBins(total) + total.OutOfBoundsCounts.Lower + total.OutOfBoundsCounts.Upper);

        // The same-shape merge adds bins directly, so every count must be exact.
        var dataC = new[] { -3.0, 0.5, 1.5, 2.0, 9.0 };
        var c = new RunningStatistics.Histogram([0.0, 1.0, 2.0], true, true);
        for (var i = 0; i < dataC.Length; i++) c.Fit(dataC[i]);

        var sums = a.Clone();
        sums.Merge(c);
        var aCounts = BinCounts(a);
        var cCounts = BinCounts(c);
        var sumCounts = BinCounts(sums);
        for (var k = 0; k < aCounts.Length; k++)
        {
            Assert.Equal(aCounts[k] + cCounts[k], sumCounts[k]);
        }

        Assert.Equal(a.OutOfBoundsCounts.Lower + c.OutOfBoundsCounts.Lower, sums.OutOfBoundsCounts.Lower);
        Assert.Equal(a.OutOfBoundsCounts.Upper + c.OutOfBoundsCounts.Upper, sums.OutOfBoundsCounts.Upper);
    }

    // The double steps away from a positive finite value along the doubles. The step
    // has to come in the representable spacing at that value, not in whole numbers:
    // past 2^53 the spacing is already 2 or more, so adding one to the value would
    // land back on the value itself. Reading the bit pattern as a whole number gives
    // the same order the real line gives, so one step there is one step here.
    private static double NextAfter(double value, int steps)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        return BitConverter.Int64BitsToDouble(bits + steps);
    }

    // The doubles touching a finite value from either side. A search that compared
    // the wrong side of an edge first disagrees with the linear route one step away
    // from that edge, so the lookup gate probes these neighbours.
    private static double UlpAbove(double value)
    {
        if (double.IsInfinity(value)) return value;
        var bits = BitConverter.DoubleToInt64Bits(value);
        return BitConverter.Int64BitsToDouble(bits < 0 ? bits - 1 : bits + 1);
    }

    private static double UlpBelow(double value)
    {
        if (double.IsInfinity(value)) return value;
        var bits = BitConverter.DoubleToInt64Bits(value);
        if (bits == 0) bits = long.MinValue;
        return BitConverter.Int64BitsToDouble(bits < 0 ? bits + 1 : bits - 1);
    }

    // bins + 1 evenly spaced edges between lower and upper.
    private static List<double> EvenEdges(int bins, double lower, double upper)
    {
        var edges = new List<double>(bins + 1);
        for (var i = 0; i <= bins; i++)
        {
            edges.Add(lower + (upper - lower) * i / bins);
        }

        return edges;
    }

    // The outer edges replaced by infinities. The route must stay total here:
    // only a bin with both bounds infinite is illegal, and the constructor
    // rejects that one earlier.
    private static List<double> InfiniteOuterEdges(List<double> edges)
    {
        var swapped = new List<double>(edges);
        swapped[0] = double.NegativeInfinity;
        swapped[^1] = double.PositiveInfinity;
        return swapped;
    }

    // The oracle for the lookup: the linear route, restated here. The bins are
    // built with the constructor's rules, then the value is looked up with a scan
    // over Histogram.Bin.Contains, which is what the old Histogram.UncheckedFit did.
    // A result of -1 means an out-of-bounds counter, not a bin. Keeping the oracle
    // in the test is what makes the lookup gate meaningful after the swap: the
    // production class no longer holds a scan of its own to compare against.
    private static int ScanBinIndex(List<double> edges, bool leftClosed, bool endsClosed, double value)
    {
        var bins = new List<RunningStatistics.Histogram.Bin>();
        if (leftClosed)
        {
            for (var i = 1; i < edges.Count - 1; i++)
            {
                bins.Add(new RunningStatistics.Histogram.Bin(edges[i - 1], edges[i], true, false));
            }

            bins.Add(new RunningStatistics.Histogram.Bin(edges[^2], edges[^1], true, endsClosed));
        }
        else
        {
            bins.Add(new RunningStatistics.Histogram.Bin(edges[0], edges[1], endsClosed, true));
            for (var i = 2; i < edges.Count; i++)
            {
                bins.Add(new RunningStatistics.Histogram.Bin(edges[i - 1], edges[i], false, true));
            }
        }

        if (value < bins[0].Lower) return -1;
        if (value > bins[^1].Upper) return -1;
        if (value.Equals(bins[^1].Upper)) return bins[^1].ClosedRight ? bins.Count - 1 : -1;
        if (value.Equals(bins[0].Lower)) return bins[0].ClosedLeft ? 0 : -1;

        for (var i = 0; i < bins.Count; i++)
        {
            if (bins[i].Contains(value)) return i;
        }

        throw new InvalidOperationException("The scan found no bin for an interior value");
    }

    // The lookup route must answer the linear route everywhere. Three bin counts
    // are checked, 5, 50 and 500, and both outer-edge shapes: finite edges and
    // infinite outer edges. All four closure-flag combinations are checked. The
    // probe values are each edge, the doubles touching that edge on either side,
    // a plain step outside each outer edge, the infinities when the outer edges
    // are infinite, and seeded interior values at three scales. Each probe runs
    // through a fresh histogram, so a wrong route shows up as a different bin
    // holding the single observation. The seeded stream then runs whole, and the
    // counts must match the scan's answer bin for bin. A red fact means the search
    // routed a value somewhere the scan does not; fix the route, do not thin the
    // probes or move the edges closer together.
    [Fact]
    public void Gate_HistogramBinLookupMatchesLinearScan()
    {
        foreach (var bins in new[] { 5, 50, 500 })
        {
            // Sample edges cheaply at the big bin counts, so the fact stays quick
            // while still reading interior edges far from the outer ones.
            var stride = bins > 50 ? 25 : 1;
            foreach (var outerInfinite in new[] { false, true })
            {
                var edges = EvenEdges(bins, -1000.0, 1000.0);
                if (outerInfinite) edges = InfiniteOuterEdges(edges);

                var probes = new List<double>();
                for (var i = 0; i < edges.Count; i += stride)
                {
                    probes.Add(edges[i]);
                    probes.Add(UlpAbove(edges[i]));
                    probes.Add(UlpBelow(edges[i]));
                }

                if (outerInfinite)
                {
                    // The infinities sit on an outer edge, so the flags decide.
                    // The largest finite doubles sit inside them, in the outer
                    // bin.
                    probes.Add(double.NegativeInfinity);
                    probes.Add(double.PositiveInfinity);
                    probes.Add(-1.7976931348623157e308);
                    probes.Add(1.7976931348623157e308);
                }
                else
                {
                    probes.Add(edges[0] - 1.0);
                    probes.Add(edges[^1] + 1.0);
                }

                foreach (var scale in new[] { 1.0, 1e6, 1e15 })
                {
                    var drawn = PropertyHelpers.UniformUnit(40, 202611250L + bins);
                    foreach (var v in drawn)
                    {
                        probes.Add(-1000.0 * scale + v * 2000.0 * scale);
                    }
                }

                foreach (var leftClosed in new[] { true, false })
                {
                    foreach (var endsClosed in new[] { true, false })
                    {
                        foreach (var value in probes)
                        {
                            RunningStatistics.Histogram h = new(edges, leftClosed, endsClosed);
                            h.Fit(value);

                            var expected = ScanBinIndex(edges, leftClosed, endsClosed, value);
                            var expectedCounts = new int[bins];
                            if (expected >= 0)
                            {
                                expectedCounts[expected] = 1;
                            }

                            Assert.True(expectedCounts.SequenceEqual(BinCounts(h)),
                                        $"the lookup disagrees with the linear scan at value {value} for {bins} bins with leftClosed={leftClosed}, endsClosed={endsClosed}");
                            Assert.Equal(1, h.Nobs);
                            Assert.Equal(expected >= 0 ? 0 : 1,
                                         h.OutOfBoundsCounts.Lower + h.OutOfBoundsCounts.Upper);
                        }

                        // The whole seeded stream, on data built once and shared.
                        var stream = PropertyHelpers.OffsetSpread(2_000, 0.0, 2000.0, 202611251L + bins);
                        RunningStatistics.Histogram streamed = new(edges, leftClosed, endsClosed);
                        foreach (var v in stream) streamed.Fit(v - 1000.0);

                        var streamCounts = new int[bins];
                        for (var i = 0; i < stream.Count; i++)
                        {
                            var index = ScanBinIndex(edges, leftClosed, endsClosed, stream[i] - 1000.0);
                            if (index >= 0) streamCounts[index]++;
                        }

                        Assert.True(streamCounts.SequenceEqual(BinCounts(streamed)),
                                    $"the seeded stream lands in different bins than the linear scan for {bins} bins with leftClosed={leftClosed}, endsClosed={endsClosed}");
                        Assert.Equal(2_000, streamed.Nobs);
                    }
                }
            }
        }
    }
}