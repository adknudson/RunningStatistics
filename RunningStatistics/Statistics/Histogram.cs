using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a histogram for running statistics. A set of edges defines the boundaries of the
/// bins. Each bin has left and right ends that are open or closed.
/// </summary>
public sealed class Histogram : RunningStatisticBase<double, Histogram>, IEnumerable<Histogram.Bin>
{
    private readonly List<double> _edges;
    private readonly List<Bin> _bins;
    
    private long _nobs;
    private long _lowerOutOfBounds;
    private long _upperOutOfBounds;


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
        _edges = [..edges];

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


        _bins = [];

        if (LeftClosed)
        {
            // add all but last normally
            for (var i = 1; i < _edges.Count - 1; i++)
            {
                _bins.Add(new Bin(_edges[i - 1], _edges[i], true, false));
            }

            var lastTwoEdges = Edges.Reverse().Take(2).Reverse().ToList();
            _bins.Add(new Bin(lastTwoEdges[0], lastTwoEdges[1], true, EndsClosed));
        }
        else
        {
            // add all but first normally
            var firstTwoEdges = Edges.Take(2).ToList();
            _bins.Add(new Bin(firstTwoEdges[0], firstTwoEdges[1], EndsClosed, true));

            for (var i = 2; i < _edges.Count; i++)
            {
                _bins.Add(new Bin(_edges[i - 1], _edges[i], false, true));
            }
        }
    }


    /// <summary>
    /// Gets the counts of values that are outside the bounds of the histogram bins.
    /// </summary>
    public (long Lower, long Upper) OutOfBoundsCounts => (_lowerOutOfBounds, _upperOutOfBounds);

    /// <summary>
    /// Gets the number of bins in the histogram.
    /// </summary>
    public int NumBins => _bins.Count;

    /// <summary>
    /// Gets the list of histogram bins.
    /// </summary>
    public IEnumerable<Bin> Bins => _bins;

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
    /// EndsClosed =  true: [a, b), [b, c), [c, d]
    /// EndsClosed = false: [a, b), [b, c), [c, d)
    /// </code>
    /// </example>
    public bool EndsClosed { get; }


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

        var firstBin = _bins[0];
        var lastBin = _bins[NumBins - 1];

        if (value < firstBin.Lower)
        {
            _lowerOutOfBounds += count;
            return;
        }

        if (value > lastBin.Upper)
        {
            _upperOutOfBounds += count;
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
                _upperOutOfBounds += count;
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
                _lowerOutOfBounds += count;
            }
            return;
        }

        var bin = _bins[FindBinIndex(value)];
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
        // right. That is the answer Bin.Contains gives for that edge,
        // so the route leaves the bin counts unchanged. low stays above zero
        // here, because a value equal to the first edge returns earlier.
        if (!LeftClosed && value.Equals(_edges[low])) return low - 1;

        return low;
    }

    public override void Reset()
    {
        _nobs = 0;

        foreach (var bin in _bins)
        {
            bin.Reset();
        }

        _lowerOutOfBounds = 0;
        _upperOutOfBounds = 0;
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
        _lowerOutOfBounds += histogram._lowerOutOfBounds;
        _upperOutOfBounds += histogram._upperOutOfBounds;

        if (BinsAreMatching(histogram._bins))
        {
            checked { _nobs += histogram.Nobs; }

            for (var j = 0; j < NumBins; j++)
            {
                _bins[j].Merge(histogram._bins[j]);
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
            checked { _nobs += histogram._lowerOutOfBounds + histogram._upperOutOfBounds; }

            // Using midpoints of source bins as approximate locations for merging
            foreach (var bin in histogram._bins)
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
    private bool BinsAreMatching(List<Bin> other)
    {
        return _bins.Count == other.Count
               && _bins.Zip(other, (a, b) => (a, b)).All(z => z.a.Equals(z.b));
    }

    public IEnumerator<Bin> GetEnumerator() => _bins.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => base.ToString() + $" | NumBins={NumBins}";


    /// <summary>
    /// Represents a bin in a histogram.
    /// </summary>
    public sealed class Bin : IEquatable<Bin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bin"/> class.
        /// </summary>
        /// <param name="lower">The lower bound of the bin.</param>
        /// <param name="upper">The upper bound of the bin.</param>
        /// <param name="closedLeft">Shows if the bin has a closed left side.</param>
        /// <param name="closedRight">Shows if the bin has a closed right side.</param>
        /// <exception cref="ArgumentException">Thrown when the lower bound is not strictly less than
        /// the upper bound or both bounds are infinite.</exception>
        internal Bin(double lower, double upper, bool closedLeft, bool closedRight)
        {
            if (lower >= upper)
            {
                throw new ArgumentException(
                    $"Lower bound must be strictly less than upper bound. Lower: {lower}, Upper: {upper}");
            }

            if (double.IsInfinity(lower) && double.IsInfinity(upper))
            {
                throw new ArgumentException("Both bounds cannot be infinite");
            }

            Lower = lower;
            Upper = upper;
            ClosedLeft = closedLeft;
            ClosedRight = closedRight;

            Nobs = 0;
        }


        /// <summary>
        /// Gets the number of observations in the bin.
        /// </summary>
        public long Nobs { get; private set; }

        /// <summary>
        /// Gets the lower bound of the bin.
        /// </summary>
        public double Lower { get; }

        /// <summary>
        /// Gets the upper bound of the bin.
        /// </summary>
        public double Upper { get; }

        /// <summary>
        /// Gets a value that shows if the bin has a closed left side.
        /// </summary>
        public bool ClosedLeft { get; }

        /// <summary>
        /// Gets a value that shows if the bin has a closed right side.
        /// </summary>
        public bool ClosedRight { get; }

        /// <summary>
        /// Gets the midpoint of the bin. If either the lower or upper bound is infinite, then midpoint
        /// is set to the infinite bound.
        /// </summary>
        public double Midpoint
        {
            get
            {
                if (double.IsInfinity(Lower)) return Lower;
                if (double.IsInfinity(Upper)) return Upper;
                return Lower + (Upper - Lower) / 2;
            }
        }


        /// <summary>
        /// Determines if the specified value falls within the bin.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the bin contains the value, otherwise false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the bin encounters an unexpected state.</exception>
        internal bool Contains(double value)
        {
            // strictly outside of the bin
            if (value < Lower || value > Upper) return false;
            // strictly inside of the bin
            if (Lower < value && value < Upper) return true;

            // potentially on the boundary
            if (value == Lower) return ClosedLeft;
            if (value == Upper) return ClosedRight;

            // should never reach here
            throw new InvalidOperationException("Unexpected state");
        }

        /// <summary>
        /// Resets the number of observations in the bin.
        /// </summary>
        internal void Reset()
        {
            Nobs = 0;
        }

        /// <summary>
        /// Merges another bin into this bin.
        /// </summary>
        /// <param name="other">The other bin to merge.</param>
        internal void Merge(Bin other)
        {
            Nobs += other.Nobs;
        }

        /// <summary>
        /// Increments the number of observations in the bin by the specified count.
        /// </summary>
        /// <param name="count">The count to increment by.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the count is negative.</exception>
        internal void Increment(long count)
        {
            Require.NonNegative(count);
            Nobs += count;
        }

        public bool Equals(Bin? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Lower == other.Lower
                   && Upper == other.Upper
                   && ClosedLeft == other.ClosedLeft
                   && ClosedRight == other.ClosedRight;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Bin other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Lower.GetHashCode();
                hashCode = hashCode * 397 ^ Upper.GetHashCode();
                hashCode = hashCode * 397 ^ ClosedLeft.GetHashCode();
                hashCode = hashCode * 397 ^ ClosedRight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Bin? left, Bin? right) => Equals(left, right);

        public static bool operator !=(Bin? left, Bin? right) => !Equals(left, right);

        public override string ToString()
        {
            var leftBrace = ClosedLeft ? '[' : '(';
            var rightBrace = ClosedRight ? ']' : ')';
            var binRepr = $"{leftBrace}{Lower}, {Upper}{rightBrace}";
            
            return $"{nameof(Bin)}(Nobs={Nobs:N0}) | Bin={binRepr}";
        }
    }
}