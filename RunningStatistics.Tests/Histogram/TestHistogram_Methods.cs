using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RunningStatistics.Tests.Histogram;

public partial class TestHistogram
{
    // The shared edge list: negative infinity, 0, 10 and 100. Consecutive edges form the
    // three bins this histogram splits the number line into.
    private readonly double[] _edges = [double.NegativeInfinity, 0, 10, 100];
    
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var hist = new RunningStatistics.Histogram([0, 0.5, 1]);
        hist.Fit(0.5, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => hist.Fit(0.5, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, hist.Nobs);
    }
    
    // The constructor's checks on the edge list. These scenarios constructed quietly
    // under the old lenient behavior and are now rejected: a NaN edge in any position,
    // edges out of order, which the constructor used to sort for the caller, and repeated
    // edges. The constructor now requires every edge to be a real number or an infinity,
    // and the edges to strictly increase, so each of these scenarios must throw.
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
        // The bins run from negative infinity to 0 including 0, 0 to 10 including 10, and
        // 10 to 100 including 100, with the left ends open.
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
        // The bins run from negative infinity to 0 including 0, 0 to 10 including 10, and
        // 10 to 100 including 100, with the left ends open.
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
        // A value of -1 sits in the bin at or below 0, 5 sits in the bin above 0 up to 10,
        // and 1000 is past the last edge so it counts as out of bounds.
        Assert.Equal([1, 1, 0], BinCounts(clone));
        Assert.Equal(0, clone.OutOfBoundsCounts.Lower);
        Assert.Equal(1, clone.OutOfBoundsCounts.Upper);
        Assert.Equal(3, clone.Nobs);
    }

    // The accumulation pattern user code writes: clone an empty copy carrying the
    // configuration, then merge each chunk into it. The clone keeps the edges, so the
    // merge must take the same-bins branch that adds bin counts directly; the fallback
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

        // Each bin now holds the sum of the two chunks' counts, the out-of-bounds counters
        // sum the same way, and nothing was dropped.
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

    // Reset must clear counters while leaving the bin configuration alone: edges, closure flags
    // and the bin list survive, OOB counters clear (not just the bins), and refitting the
    // cleared histogram reproduces the exact bin membership it had before the reset.
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

    // Decision note: the fallback for mismatched edges in the merge is pinned exactly as
    // the library behaves today. When the two histograms' edge lists differ, the merge
    // does not throw. It replays each source bin's count at that bin's midpoint, an
    // approximation of where the observations actually were, and a midpoint outside the
    // receiver falls into its out-of-bounds counters instead of a bin. Making this a
    // throw, or redistributing more finely, would change released behavior and would
    // need the staged correctness and benchmark gate from the house rules before adopting
    // it. For the record, the two statistics differ here: merging empirical cumulative
    // distributions rejects a batch-size mismatch outright, while a histogram merge
    // approximates. The same-edges branch, where counts are added directly, is already
    // pinned by Merge_MatchingEdgesSumsBinsDirectly and CloneEmpty_AccumulateMergeSumsBinsDirectly,
    // so it is not repeated below.
    //
    // The scenario pinned here: the receiver covers 0 up to but not including 10, and 10
    // to 20 including 10, and holds one observation, 5, in its first bin. The source uses
    // four bins of width 5, each closed on the left except the last which closes its right
    // edge, and holds one observation in each, fitted at 1, 6, 11 and 16, with midpoints
    // 2.5, 7.5, 12.5 and 17.5. The replay sends 2.5 and 7.5 into the receiver's first bin
    // and 12.5 and 17.5 into its second, leaving bin counts 3 and 2, a total of one plus
    // four observations, and out-of-bounds counters still at zero.
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

    // The replayed midpoints go through the same unchecked fitting a normal fit would
    // use, so a midpoint outside the receiver lands in its out-of-bounds counters just as
    // an ordinary out-of-range observation would.
    //
    // Scenario a: the receiver covers 10 to 12 and holds an observation at 11. The source
    // covers 0 to 100 and fits 50 with a count of four. The only source midpoint, 50, sits
    // past the receiver's top edge, so four observations go to the upper out-of-bounds
    // counter, the total becomes five, and the bins keep their one count unchanged.
    //
    // Scenario b: the source has edges at negative infinity, 0, 10 and positive infinity,
    // and its bins are replayed into a receiver covering 0 to 10 and 10 to 20. A bin with
    // an infinite bound reports that infinite bound as its midpoint, so the outer bins
    // replay at negative infinity and positive infinity, going to the lower and upper
    // out-of-bounds counters, and only the middle bin's midpoint, 5, lands in a bin.
    // Fitting only rejects NaN, so the infinities are accepted rather than thrown.
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

        // The middle bin's midpoint, 5, replays into the first bin; negative infinity goes
        // to the lower counter and positive infinity to the upper one.
        Assert.Equal([2, 0], BinCounts(receiver));
        Assert.Equal(4, receiver.Nobs);
        Assert.Equal(1, receiver.OutOfBoundsCounts.Lower);
        Assert.Equal(1, receiver.OutOfBoundsCounts.Upper);
    }

    // Matching bins compares both the edge values and the closure flags, so two histograms
    // with identical edges but different closure settings take the replay branch rather
    // than adding counts. The receiver covers 0 up to but not including 10, and 10 to 20
    // including 10, with the default flags; the source uses the same edges closed on the
    // right instead. Its two fits of 10 land in the source's first bin because its left
    // end is open. Adding counts directly would have placed them in the receiver's second
    // bin; the replay instead puts both observations at the first bin's midpoint, 5,
    // landing in the receiver's first bin. The resulting counts, two and zero, are only
    // reachable through that replay.
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
        // The bins are: negative infinity to 0 excluding 0, 0 to 10 excluding 10, and 10
        // to 100 including 100.
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
        // The bins are: negative infinity to 0 excluding 0, 0 to 10 excluding 10, and 10
        // to 100 excluding 100.
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
        // The bins are: negative infinity to 0 including 0, above 0 up to 10 including 10,
        // and above 10 up to 100 including 100.
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
        // The bins run from negative infinity to 0 including 0, 0 to 10 including 10, and
        // 10 to 100 including 100, with the left ends open.
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
        // The bins are: above 0 up to and including 0.1, above 0.1 up to and including 0.5,
        // and above 0.5 up to and including 0.9.
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
    // The promise about which bin receives an observation sitting exactly on an edge,
    // for every combination of the two closure flags, pinned exactly as the fitting and
    // the bin-containment check behave today. Each scenario reads back the observation
    // count of every bin in index order, so the exact bin that gets each edge
    // observation is pinned. Every scenario also checks the conservation identity: the
    // total count equals the sum of the bins plus the two out-of-bounds counters, so
    // no bin double-counts an observation and no observation counted in the total is
    // counted nowhere.
    //
    // How the flags decide membership, for edges running from the first to the last:
    // - An observation on an interior edge goes to the bin above it when the bins are
    //   closed on the left, and to the bin below it when they are closed on the right.
    // - The first edge belongs to the first bin when the left end is closed. When the
    //   left end is open, it belongs to the first bin only if the ends are closed;
    //   otherwise it goes to the lower out-of-bounds counter.
    // - The last edge belongs to the last bin when the right end is closed. When the
    //   left end is closed instead, it belongs to the last bin only if the ends are
    //   closed; otherwise it goes to the upper out-of-bounds counter.
    // - A value strictly below the first edge or strictly above the last edge goes to
    //   the matching out-of-bounds counter regardless of the flags; it is never thrown
    //   away, and it counts in the total but not in any bin.
    // - A value of negative infinity sitting at a first edge of negative infinity is in
    //   the first bin exactly when that bin's left end is closed: the closed-left flag
    //   forces that, and the open-left flag takes it from the ends flag. A value of
    //   positive infinity at a last edge of positive infinity is in the last bin exactly
    //   when that bin's right end is closed: the open-left flag always closes it, the
    //   closed-left flag closes it only when the ends are closed.
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
        // With the left end closed, an interior bin covers its lower edge, so an
        // observation sitting exactly on an interior edge, such as 0.25, 0.5 or 0.75,
        // belongs to the bin above it, at indices 1, 2 and 3. The ends flag never moves
        // an interior edge, so both of its values give the same counts.
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
        // With the right end closed, an interior bin covers its upper edge, so an
        // observation sitting exactly on an interior edge belongs to the bin below it,
        // one bin lower than in the closed-left case. The ends flag changes nothing here.
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
        // An observation exactly on the first edge, 0.0, with the left end closed: the
        // first bin always closes its left end when the flag asks for a closed left, so
        // the observation is in the first bin for either value of the ends flag. It is
        // counted once in the bins and once in the total.
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
        // With the left end open, the first bin closes its left end only when the ends
        // flag asks for it. With the ends open too, an observation on the first edge
        // sits at an open end: it is not silently dropped and not thrown. A dedicated
        // check in the fitting notices a value equal to the first bin's lower edge and
        // routes it to the lower out-of-bounds counter, so the bins count nothing, the
        // counter holds one, and the total holds one.
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
        // With the left end closed, the last bin covers its upper edge only when the ends
        // flag asks for it. An observation exactly on the last edge, 1.0, meets the check
        // for a value equal to the last bin's upper edge: with the ends closed it goes to
        // the last bin, and with the ends open it goes to the upper out-of-bounds counter
        // instead. The bins stay at zero, the counter holds one and the total holds one,
        // so the observation is counted, not lost.
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
        // With the right end closed, the last bin always closes its right end, so an
        // observation exactly on the last edge is in the last bin for either value of
        // the ends flag. With the left end open, the ends flag acts on the left side
        // instead, the mirror of the first-edge case above.
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
        // Observations strictly outside the outer edges, with finite ends, never throw:
        // the checks for a value below the first bin's lower edge and above the last
        // bin's upper edge run before any closure-flag logic, so the flags make no
        // difference. The bins ignore them while the counters hold two below and three
        // above, the total is five, and the total equals the bins plus the counters
        // exactly: no bin over-counts, and every counted observation lives somewhere.
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
        // equals the first bin's lower edge, so it belongs to the first bin exactly when
        // that bin's left end is closed, which the closed-left flag forces and the open-
        // left flag takes from the ends flag. Negative infinity with both flags open goes
        // to the lower out-of-bounds counter. Positive infinity sits strictly above the
        // last edge of 100, so it takes the upper counter regardless of the flags. The
        // conservation identity holds in every combination.
        for (var leftClosed = 0; leftClosed < 2; leftClosed++)
        {
            for (var endsClosed = 0; endsClosed < 2; endsClosed++)
            {
                RunningStatistics.Histogram h = new([double.NegativeInfinity, 0.0, 10.0, 100.0], leftClosed == 0, endsClosed == 0);
                h.Fit(double.NegativeInfinity);
                h.Fit(double.PositiveInfinity);
                if (leftClosed == 0 || endsClosed == 0)
                {
                    // Where the first bin closes at negative infinity, by the closed-left
                    // flag or by the open-left flag paired with closed ends, the
                    // observation of negative infinity belongs to the first bin; four
                    // edges make three bins.
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

        // Edges at 0, 10 and positive infinity: an observation of positive infinity equals
        // the last bin's upper edge, so it belongs to the last bin exactly when that bin's
        // right end is closed, which the open-left flag forces and the closed-left flag
        // takes from the ends flag. With the closed left and open ends, positive infinity
        // goes to the upper out-of-bounds counter instead of a bin.
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

    // Why this matters: the self-merge facts in the abstract test pin the total count at
    // the scale of 2 to the 62nd power, but nothing pinned the counters inside the bins
    // at that scale. A weighted fit raises a bin's count in one step, an unchecked
    // addition, while the histogram's own total is checked, so one histogram can hold
    // per-bin counts near 2 to the 61st power while its total stays representable. The
    // bin counters and their text form must then neither throw nor lose digits.
    //
    // Pinned here: edges 0, 1, 2 and 3, with 0.5 and 1.5 each fitted at a count of 2 to
    // the 61st power. The total comes to 2 to the 62nd power, exactly 4,611,686,018,427,387,904;
    // the two hit bins read back exactly 2 to the 61st power, 2,305,843,009,213,693,952
    // apiece; the text form succeeds, gives the same string on repeated calls, and the
    // long digit runs survive formatting. Because the text form groups thousands with the
    // locale's separator, the digit checks below compare the text with every non-digit
    // dropped, using the helper the test shares.
    [Fact]
    public void ToString_HugeBinCountsFormatsStably()
    {
        RunningStatistics.Histogram hist = new([0.0, 1.0, 2.0, 3.0]);
        hist.Fit(0.5, 1L << 61);
        hist.Fit(1.5, 1L << 61);

        // The total is the checked sum of the two weighted fits: 2 to the 61st power twice
        // makes 2 to the 62nd power, comfortably inside the whole-number range.
        Assert.Equal(1L << 62, hist.Nobs);

        // The text form renders the huge counters successfully and gives the same text on
        // every call, with no lazily recomputed rendering drifting between calls.
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

        // The conservation identity holds at this scale too: the total equals the bins plus
        // the two untouched out-of-bounds counters, which still read zero, so nothing
        // leaked out of the bins.
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