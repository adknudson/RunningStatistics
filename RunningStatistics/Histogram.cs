using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A histogram with bin partitions defined by edges.
/// </summary>
public class Histogram : IRunningStatistic<double, IEnumerable<HistogramBin>, Histogram>, IEnumerable<HistogramBin>
{
    private HistogramOutOfBounds _outOfBounds;
    private readonly IList<double> _edges;
    private readonly bool _leftClosed, _endsClosed;

    
    
    public Histogram(IEnumerable<double> edges, bool leftClosed = true, bool endsClosed = true)
    {
        var es = edges.OrderBy(e => e).ToList();
        _edges = es;
        _leftClosed = leftClosed;
        _endsClosed = endsClosed;
        
        Bins = new List<HistogramBin>(es.Count - 1);
        if (leftClosed)
        {
            // add all but last normally
            for (var i = 1; i < es.Count - 1; i++)
            {
                Bins.Add(new HistogramBin(es[i-1], es[i], true, false));
            }

            var lastTwoEdges = es.TakeLast(2).ToList();
            Bins.Add(new HistogramBin(lastTwoEdges[0], lastTwoEdges[1], true, endsClosed));
        }
        else
        {
            // add all but first normally
            var firstTwoEdges = es.Take(2).ToList();
            Bins.Add(new HistogramBin(firstTwoEdges[0], firstTwoEdges[1], endsClosed, true));
            for (var i = 2; i < es.Count; i++)
            {
                Bins.Add(new HistogramBin(es[i-1], es[i], false, true));
            }
        }
    }
    
    
    
    public long Nobs { get; private set; }

    public IEnumerable<HistogramBin> Value => Bins;

    public (long Lower, long Upper) OutOfBoundsCounts => _outOfBounds.Counts;
    
    private int NumBins => Bins.Count;
    
    private IList<HistogramBin> Bins { get; }


    
    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    /// <summary>
    /// Fit a list of value-count pairs.
    /// </summary>
    public void Fit(IEnumerable<KeyValuePair<double, long>> keyValuePairs)
    {
        foreach (var (value, count) in keyValuePairs)
        {
            Fit(value, count);
        }
    }

    public void Fit(double value)
    {
        Fit(value, 1);
    }

    /// <summary>
    /// Fit a value with a specified number of observations.
    /// </summary>
    /// <param name="value">The value being fitted.</param>
    /// <param name="count">The number of times the value is observed.</param>
    public void Fit(double value, long count)
    {
        Nobs += count;
        
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

    public void Merge(Histogram histogram)
    {
        _outOfBounds.Merge(histogram._outOfBounds);
        
        if (BinsAreMatching(histogram.Bins))
        {
            Nobs += histogram.Nobs;

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
                Fit(bin.Midpoint, bin.Nobs);
            }
        }
    }

    public static Histogram Merge(Histogram first, Histogram second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public void Reset()
    {
        Nobs = 0;
        foreach (var bin in Bins)
        {
            bin.Reset();
        }
        _outOfBounds.Reset();
    }

    public Histogram CloneEmpty()
    {
        return new Histogram(_edges, _leftClosed, _endsClosed);
    }

    public Histogram Clone()
    {
        var hist = CloneEmpty();
        hist._outOfBounds = _outOfBounds.Clone();
        
        foreach (var bin in Value)
        {
            hist.Fit(bin.Midpoint, bin.Nobs);
        }
        
        return hist;
    }

    public IEnumerator<HistogramBin> GetEnumerator() => Bins.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"{typeof(Histogram)}(n={Nobs})";
    
    private bool BinsAreMatching(ICollection<HistogramBin> other)
    {
        return Bins.Count == other.Count && Bins.Zip(other).All(z => z.First.Equals(z.Second));
    }
}