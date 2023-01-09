using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A histogram with bin partitions defined by edges.
/// </summary>
public class Histogram : IRunningStatistic<double, IEnumerable<HistogramBin>, Histogram>
{
    private OutOfBounds _outOfBounds;
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

    public void Fit(double value)
    {
        Fit(value, 1);
    }

    public void Fit(double value, long k)
    {
        Nobs += k;
        
        var firstBin = Bins.First();
        var lastBin = Bins.Last();

        if (value < firstBin.Lower)
        {
            _outOfBounds.Update(Side.Lower, k);
            return;
        }

        if (value > lastBin.Upper)
        {
            _outOfBounds.Update(Side.Upper, k);
            return;
        }

        if (value.Equals(lastBin.Upper))
        {
            if (lastBin.ClosedRight)
            {
                lastBin.Increment(k);
            }
            else
            {
                _outOfBounds.Update(Side.Upper, k);
            }
            return;
        }

        if (value.Equals(firstBin.Lower))
        {
            if (firstBin.ClosedLeft)
            {
                firstBin.Increment(k);
            }
            else
            {
                _outOfBounds.Update(Side.Lower, k);
            }
            return;
        }

        var bin = Bins.First(bin => bin.Contains(value));
        bin.Increment(k);
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
    
    public override string ToString() => $"{typeof(Histogram)}(n={Nobs})";
    
    private bool BinsAreMatching(ICollection<HistogramBin> other)
    {
        return Bins.Count == other.Count && Bins.Zip(other).All(z => z.First.Equals(z.Second));
    }
    
    private enum Side
    {
        Lower, Upper
    }

    private struct OutOfBounds
    {
        private long Lower { get; set; }

        private long Upper { get; set; }
        
        public (long, long) Counts => (Lower, Upper);

        
        
        public void Reset()
        {
            Lower = 0;
            Upper = 0;
        }

        public void Update(Side side, long k)
        {
            switch (side)
            {
                case Side.Lower:
                    Lower += k;
                    return;
                case Side.Upper:
                    Upper += k;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void Merge(OutOfBounds other)
        {
            Lower += other.Lower;
            Upper += other.Upper;
        }

        public OutOfBounds Clone()
        {
            return new OutOfBounds
            {
                Lower = Lower,
                Upper = Upper
            };
        }
    }
}