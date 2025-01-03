using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace RunningStatistics;

public sealed class Histogram : RunningStatisticBase<double, Histogram>, IEnumerable<HistogramBin>
{
    private HistogramOutOfBounds _outOfBounds;
    private readonly List<double> _edges;
    private bool _binsAreInitialized;
    private long _nobs;
    
    
    public Histogram(IEnumerable<double> edges, bool leftClosed = true, bool endsClosed = true)
    {
        _edges = edges.OrderBy(e => e).ToList();
        LeftClosed = leftClosed;
        EndsClosed = endsClosed;
        
        InitializeBins();
    }
    

    public (long Lower, long Upper) OutOfBoundsCounts => _outOfBounds.Counts;
    
    private int NumBins => Bins.Count;

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
    /// LeftClosed= true: [a, b), [b, c)
    /// LeftClosed=false: (a, b], (b, c]
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
        _nobs += count;
        
        var firstBin = Bins.First();
        var lastBin = Bins.Last();

        if (value < firstBin.Lower)
        {
            _outOfBounds.Update(OutOfBoundsSide.Lower, count);
            return;
        }

        if (value > lastBin.Upper)
        {
            _outOfBounds.Update(OutOfBoundsSide.Upper, count);
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
                _outOfBounds.Update(OutOfBoundsSide.Upper, count);
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
                _outOfBounds.Update(OutOfBoundsSide.Lower, count);
            }
            return;
        }

        var bin = Bins.First(bin => bin.Contains(value));
        bin.Increment(count);
    }

    public override void Reset()
    {
        _nobs = 0;
        
        foreach (var bin in Bins)
        {
            bin.Reset();
        }
        
        _outOfBounds.Reset();
    }

    public override Histogram CloneEmpty() => new(_edges, LeftClosed, EndsClosed);
    
    public override void Merge(Histogram histogram)
    {
        _outOfBounds.Merge(histogram._outOfBounds);
        
        if (BinsAreMatching(histogram.Bins))
        {
            _nobs += histogram.Nobs;

            for (var j = 0; j < NumBins; j++)
            {
                Bins[j].Merge(histogram.Bins[j]);
            }
        }
        else
        {
            // Using midpoints of source bins as approximate locations for merging
            foreach (var bin in histogram.Bins)
            {
                UncheckedFit(bin.Midpoint, bin.Nobs);
            }
        }
    }

    private bool BinsAreMatching(ICollection<HistogramBin> other)
    {
        return Bins.Count == other.Count && Bins.Zip(other, (bin1, bin2) => (bin1, bin2)).All(z => z.bin1.Equals(z.bin2));
    }
    
    public IEnumerator<HistogramBin> GetEnumerator() => Bins.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected override string GetStatsString() => $"NumBins={NumBins}";
}