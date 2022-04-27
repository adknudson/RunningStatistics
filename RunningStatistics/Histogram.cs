using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A histogram with bin partitions defined by edges.
/// </summary>
public class Histogram : IRunningStatistic<double>, IEnumerable<(string BinName, int Count)>
{
    private readonly bool _left, _closed;
    private OutOfBounds _outOfBounds;
    private readonly IList<string> _binNames;
    private const double Tolerance = 1.4901161193847656e-8;


    public Histogram(IEnumerable<double> edges, bool left = true, bool closed = true)
    {
        Count = 0;

        Edges = edges.OrderBy(e => e).ToList();
        _left = left;
        _closed = closed;

        BinCounts = Enumerable.Repeat(0, NumBins).ToList();
        _outOfBounds = new OutOfBounds();
        _binNames = Utils.GetPrintableBins(Edges, _left, _closed);
    }


    public long Count { get; private set; }
    public (int Lower, int Upper) OutOfBoundsCounts => _outOfBounds.Counts;

    private int NumBins => Edges.Count - 1;
    private IList<double> Edges { get; }
    private IList<int> BinCounts { get; set; }


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

    public void Fit(double value, int k)
    {
        Count += k;

        var i = GetBinIndex(value);
        if (0 <= i && i < NumBins)
        {
            BinCounts[i] += k;
        }
        else
        {
            _outOfBounds.Update(i, k);
        }
    }

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Histogram histogram) return;
        
        if (EdgesMatch(histogram.Edges))
        {
            Count += histogram.Count;

            for (var j = 0; j < NumBins; j++)
            {
                BinCounts[j] += histogram.BinCounts[j];
            }
        }
        else
        {
            // Using midpoints of source edges as approximate locations for merging
            var midpoints = Utils.Midpoints(histogram.Edges);
            for (var j = 0; j < midpoints.Count; j++)
            {
                Fit(midpoints[j], histogram.BinCounts[j]);
            }
        }
    }

    public void Reset()
    {
        Count = 0;
        BinCounts = BinCounts.Select(_ => 0).ToList();
        _outOfBounds.Reset();
    }


    public IEnumerator<(string BinName, int Count)> GetEnumerator()
    {
        return _binNames.Zip(BinCounts).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"{typeof(Histogram)}(n={Count})";


    /// <summary>
    /// Get the index of the bin that an observation falls in.
    /// </summary>
    private int GetBinIndex(double y)
    {
        var a = Edges.First();
        var b = Edges.Last();

        if (y < a)
        {
            return -1;
        }

        if (y > b)
        {
            return NumBins;
        }

        if (_closed)
        {
            if (y.Equals(a))
            {
                return 0;
            }

            if (y.Equals(b))
            {
                return NumBins - 1;
            }
        }

        return _left ? Utils.SearchSortedLast(Edges, y) : Utils.SearchSortedFirst(Edges, y) - 1;
    }

    private bool EdgesMatch(ICollection<double> other)
    {
        if (Edges.Count != other.Count) return false;

        return Edges
            .Zip(other)
            .All(z => Math.Abs(z.First - z.Second) <= Tolerance);
    }

    private struct OutOfBounds
    {
        public OutOfBounds()
        {
            Lower = 0;
            Upper = 0;
        }

        
        private int Lower { get; set; }
        private int Upper { get; set; }
        public (int, int) Counts => (Lower, Upper);

        
        public void Reset()
        {
            Lower = 0;
            Upper = 0;
        }

        public void Update(int index, int k)
        {
            if (index >= 0)
            {
                Upper += k;
            }
            else
            {
                Lower += k;
            }
        }
    }
}