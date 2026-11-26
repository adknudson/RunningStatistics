using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a histogram for running statistics. A set of edges defines the boundaries of the
/// bins. Each bin has left and right ends that are open or closed.
/// </summary>
public sealed class Histogram : RunningStatisticBase<double, Histogram>, IEnumerable<HistogramBin>
{
    private HistogramOutOfBounds _oob;
    private readonly List<double> _edges;
    private bool _binsAreInitialized;
    private long _nobs;


    /// <summary>
    /// Initializes a new instance of the <see cref="Histogram"/> class.
    /// </summary>
    /// <param name="edges">The edges of the histogram bins, in strictly monotonically increasing
    /// order. The constructor rejects NaN edge values. Infinite edge values are fine.</param>
    ///
    /// <param name="leftClosed">Shows if the bins have a closed left side. With true, the left
    /// side of each bin is the closed side. With false, the right side is the closed side.</param>
    ///
    /// <param name="endsClosed">Shows if the end bins have a closed side on the outside.</param>
    ///
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the constructor receives fewer
    /// than 2 edges.</exception>
    ///
    /// <exception cref="ArgumentException">Thrown when an edge value is NaN or when the edges are
    /// not strictly monotonically increasing (the constructor rejects duplicate or decreasing
    /// edges).</exception>
    public Histogram(IEnumerable<double> edges, bool leftClosed = true, bool endsClosed = true)
    {
        _edges = edges.ToList();

        if (_edges.Count < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(edges), _edges.Count, "At least 2 edges are required to define histogram bins.");
        }

        for (var i = 0; i < _edges.Count; i++)
        {
            if (double.IsNaN(_edges[i]))
            {
                throw new ArgumentException(
                    $"Edge values must not be NaN. Got NaN at index {i}.",
                    nameof(edges));
            }
        }

        for (var i = 1; i < _edges.Count; i++)
        {
            if (_edges[i - 1] >= _edges[i])
            {
                throw new ArgumentException(
                    "Edges must be strictly monotonically increasing. " +
                    $"Edge {_edges[i - 1]} at index {i - 1} is not less than edge {_edges[i]} at index {i}.",
                    nameof(edges));
            }
        }

        LeftClosed = leftClosed;
        EndsClosed = endsClosed;

        InitializeBins();
    }


    /// <summary>
    /// Gets the counts of values that are outside the bounds of the histogram bins.
    /// </summary>
    public (long Lower, long Upper) OutOfBoundsCounts => (_oob.Lower, _oob.Upper);

    /// <summary>
    /// The number of bins in the histogram.
    /// </summary>
    private int NumBins => Bins.Count;

    /// <summary>
    /// Gets the list of histogram bins.
    /// </summary>
    private List<HistogramBin> Bins { get; } = [];

    /// <summary>
    /// Get a list of the histogram edges.
    /// </summary>
    public IEnumerable<double> Edges => _edges;

    /// <summary>
    /// Are the bins closed on the left or the right?
    /// </summary>
    /// <example>
    /// <code>
    /// LeftClosed =  true: [a, b), [b, c)
    /// LeftClosed = false: (a, b], (b, c]
    /// </code>
    /// </example>
    public bool LeftClosed { get; }

    /// <summary>
    /// Is the bin on the end closed?
    /// </summary>
    /// <example>
    /// <code>
    /// EndsClosed= true: [a, b), [b, c), [c, d]
    /// EndsClosed=false: [a, b), [b, c), [c, d)
    /// </code>
    /// </example>
    public bool EndsClosed { get; }


    /// <summary>
    /// Initialize the bins according to the edges and end-bin behavior.
    /// </summary>
    private void InitializeBins()
    {
        if (_binsAreInitialized) return;

        if (LeftClosed)
        {
            // add all but last normally
            for (var i = 1; i < _edges.Count - 1; i++)
            {
                Bins.Add(new HistogramBin(_edges[i - 1], _edges[i], true, false));
            }

            var lastTwoEdges = Edges.Reverse().Take(2).Reverse().ToList();
            Bins.Add(new HistogramBin(lastTwoEdges[0], lastTwoEdges[1], true, EndsClosed));
        }
        else
        {
            // add all but first normally
            var firstTwoEdges = Edges.Take(2).ToList();
            Bins.Add(new HistogramBin(firstTwoEdges[0], firstTwoEdges[1], EndsClosed, true));

            for (var i = 2; i < _edges.Count; i++)
            {
                Bins.Add(new HistogramBin(_edges[i - 1], _edges[i], false, true));
            }
        }

        _binsAreInitialized = true;
    }

    protected override long GetNobs() => _nobs;

    public override void Fit(double value, long count)
    {
        Require.NotNaN(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    private void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }

        var firstBin = Bins.First();
        var lastBin = Bins.Last();

        if (value < firstBin.Lower)
        {
            _oob.Fit(lower: true, count);
            return;
        }

        if (value > lastBin.Upper)
        {
            _oob.Fit(lower: false, count);
            return;
        }

        if (value.Equals(lastBin.Upper))
        {
            if (lastBin.ClosedRight)
            {
                lastBin.Increment(count);
            }
            else
            {
                _oob.Fit(lower: false, count);
            }
            return;
        }

        if (value.Equals(firstBin.Lower))
        {
            if (firstBin.ClosedLeft)
            {
                firstBin.Increment(count);
            }
            else
            {
                _oob.Fit(lower: true, count);
            }
            return;
        }

        var bin = Bins[FindBinIndex(value)];
        bin.Increment(count);
    }

    /// <summary>
    /// Finds the index of the bin that holds a value the caller has already placed
    /// strictly inside the outer edges.
    ///
    /// The constructor leaves the edges sorted, distinct and free of NaN, so the
    /// comparisons below are total and a window halving search is well defined. The
    /// window keeps the invariant that <c>_edges[low]</c> is at or below the value
    /// while <c>_edges[high]</c> is above it. The checks in <see cref="UncheckedFit" />
    /// set that window before this method runs, which is also why the value can
    /// never sit on an outer edge here.
    /// </summary>
    private int FindBinIndex(double value)
    {
        var low = 0;
        var high = _edges.Count - 1;

        // Stop once the window holds neighbouring edges: the value then sits
        // inside the bin they bound, unless it matches the lower one exactly.
        while (high - low > 1)
        {
            var mid = low + (high - low) / 2;
            if (_edges[mid] <= value)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        // A value exactly on an interior edge belongs to the bin above when the
        // bins close on the left, and to the bin below when they close on the
        // right. That is the answer HistogramBin.Contains gives for that edge,
        // so the route leaves the bin counts unchanged. low stays above zero
        // here, because a value equal to the first edge returns earlier.
        if (!LeftClosed && value.Equals(_edges[low])) return low - 1;

        return low;
    }

    public override void Reset()
    {
        _nobs = 0;

        foreach (var bin in Bins)
        {
            bin.Reset();
        }

        _oob.Reset();
    }

    public override Histogram CloneEmpty() => new(_edges, LeftClosed, EndsClosed);

    /// <remarks>
    /// The merge works in two branches. Same bins mean the same number of bins with equal
    /// bounds and equal closure flags. In that case, the merge adds the counts of the matching
    /// bins directly and the result stays exact.
    ///
    /// When the bins differ, in their edges or in their closure flags, the merge does not throw.
    /// Instead it refits each source bin's count at that bin's midpoint. The midpoint
    /// approximates where the observations actually were. A midpoint outside the receiver falls
    /// into its out-of-bounds counters. The merge folds the source's out-of-bounds counters in
    /// both branches.
    /// </remarks>
    public override void Merge(Histogram histogram)
    {
        _oob.Merge(histogram._oob);

        if (BinsAreMatching(histogram.Bins))
        {
            checked { _nobs += histogram.Nobs; }

            for (var j = 0; j < NumBins; j++)
            {
                Bins[j].Merge(histogram.Bins[j]);
            }
        }
        else
        {
            // The out-of-bounds counters were folded into this histogram above, but
            // those observations never go through the replay below, which only walks
            // the source's bins. The total has to pick them up here. Without this,
            // a mismatched merge drops the source's out-of-bounds observations from
            // Nobs while its counters still report them, so the count identity
            // Nobs = bins + counters no longer holds.
            checked { _nobs += histogram.OutOfBoundsCounts.Lower + histogram.OutOfBoundsCounts.Upper; }

            // Using midpoints of source bins as approximate locations for merging
            foreach (var bin in histogram.Bins)
            {
                UncheckedFit(bin.Midpoint, bin.Nobs);
            }
        }
    }

    /// <summary>
    /// Checks if the bins of another histogram match this histogram.
    /// </summary>
    /// <param name="other">The other histogram bins to check.</param>
    /// <returns>True if the bins match, otherwise false.</returns>
    private bool BinsAreMatching(List<HistogramBin> other)
    {
        return Bins.Count == other.Count
               && this.Zip(other, (a, b) => (a, b)).All(z => z.a.Equals(z.b));
    }

    public IEnumerator<HistogramBin> GetEnumerator() => Bins.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => base.ToString() + $" | NumBins={NumBins}";
}