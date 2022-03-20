using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A histogram with bin partitions defined by edges.
/// </summary>
public class Histogram : IRunningStat<double, Histogram>, IEnumerable<(string, int)>
{
    private readonly bool _left, _closed;
    private OutOfBounds _outOfBounds;
    private readonly IList<string> _binNames;
    private readonly double _tolerance = Math.Sqrt(double.Epsilon);

    
    
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

    public Histogram(Histogram other)
    {
        Count = other.Count;

        Edges = new List<double>(other.Edges);
        _left = other._left;
        _closed = other._closed;

        BinCounts = new List<int>(other.BinCounts);
        _outOfBounds = new OutOfBounds(other._outOfBounds);
        _binNames = new List<string>(other._binNames);
    }
    
    
    
    public long Count { get; private set; }
    private int NumBins => Edges.Count - 1;
    public (int Lower, int Upper) OutOfBoundsCounts => _outOfBounds.Counts;
    public IList<double> Edges { get; }

    public IList<int> BinCounts { get; private set; }


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

    private void Fit(double value, int k)
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
    
    public void Merge(Histogram other)
    {
        if (EdgesMatch(other.Edges))
        {
            Count += other.Count;
            
            for (var j = 0; j < NumBins; j++)
            {
                BinCounts[j] += other.BinCounts[j];
            }
        }
        else
        {
            // Using midpoints of source edges as approximate locations for merging
            var midpoints = Utils.Midpoints(other.Edges);
            for (var j = 0; j < midpoints.Count; j++)
            {
                Fit(midpoints[j], other.BinCounts[j]);
            }
        }
    }

    public void Reset()
    {
        Count = 0;
        BinCounts = BinCounts.Select(_ => 0).ToList();
        _outOfBounds.Reset();
    }

    public void Print(StreamWriter stream)
    {
        stream.WriteLine(ToString());
        stream.WriteLine("Bin\tCount");
        foreach (var (bin, count) in this)
        {
            stream.WriteLine($"{bin}\t{count}");
        }
    }

    public override string ToString() => $"{typeof(Histogram)}(n={Count})";

    public IEnumerator<(string, int)> GetEnumerator() => _binNames.Zip(BinCounts).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    
    
    #region Private Methods, Classes

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

        return _closed switch
        {
            true when Math.Abs(y - a) < _tolerance => 0,
            true when Math.Abs(y - b) < _tolerance => NumBins - 1,
            _ => _left ? Utils.SearchSortedLast(Edges, y) : Utils.SearchSortedFirst(Edges, y) - 1
        };
    }
    
    private bool EdgesMatch(ICollection<double> other)
    {
        if (Edges.Count != other.Count) return false;
        
        return Edges
            .Zip(other)
            .All(z => Math.Abs((z.First - z.Second)) <= _tolerance);
    }
    
    private struct OutOfBounds
    {
        public OutOfBounds()
        {
            Lower = 0;
            Upper = 0;
        }

        public OutOfBounds(OutOfBounds other)
        {
            Lower = other.Lower;
            Upper = other.Upper;
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
    
    #endregion

    #region Static Methods

    private static Histogram Merge(Histogram a, Histogram b)
    {
        Histogram merged = new(a);
        merged.Merge(b);
        return merged;
    }
    
    public static Histogram operator +(Histogram a, Histogram b) => Merge(a, b);

    #endregion
}