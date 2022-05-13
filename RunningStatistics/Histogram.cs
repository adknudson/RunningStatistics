using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A histogram with bin partitions defined by edges.
/// </summary>
public class Histogram : IRunningStatistic<double>, IEnumerable<(string BinName, nint Count)>
{
    private OutOfBounds _outOfBounds = new();


    public Histogram(IEnumerable<double> edges, bool leftClosed = true, bool endsClosed = true)
    {
        var es = edges.OrderBy(e => e).ToList();

        Bins = new List<Bin>(es.Count - 1);
        if (leftClosed)
        {
            // add all but last normally
            for (var i = 1; i < es.Count - 1; i++)
            {
                Bins.Add(new Bin(es[i-1], es[i], true, false));
            }

            var lastTwoEdges = es.TakeLast(2).ToList();
            Bins.Add(new Bin(lastTwoEdges[0], lastTwoEdges[1], true, endsClosed));
        }
        else
        {
            // add all but first normally
            var firstTwoEdges = es.Take(2).ToList();
            Bins.Add(new Bin(firstTwoEdges[0], firstTwoEdges[1], endsClosed, true));
            for (var i = 2; i < es.Count; i++)
            {
                Bins.Add(new Bin(es[i-1], es[i], false, true));
            }
        }
    }

    public Histogram(Histogram other)
    {
        Count = other.Count;
        Bins = other.Bins.Select(b => new Bin(b)).ToList();
        _outOfBounds = new OutOfBounds(other._outOfBounds);
    }


    public nint Count { get; private set; }
    public (nint Lower, nint Upper) OutOfBoundsCounts => _outOfBounds.Counts;
    private int NumBins => Bins.Count;
    private IList<Bin> Bins { get; }


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

    public void Fit(double value, nint k)
    {
        Count += k;
        
        var firstBin = Bins.First();
        var lastBin = Bins.Last();

        if (value < firstBin.Lower)
        {
            _outOfBounds.Update(-1, k);
            return;
        }

        if (value > lastBin.Upper)
        {
            _outOfBounds.Update(NumBins, k);
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
                _outOfBounds.Update(NumBins, k);
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
                _outOfBounds.Update(-1, k);
            }
            return;
        }

        var bin = Bins.First(bin => bin.Contains(value));
        bin.Increment(k);
    }

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Histogram histogram) return;

        _outOfBounds.Merge(histogram._outOfBounds);
        
        if (BinsAreMatching(histogram.Bins))
        {
            Count += histogram.Count;

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
                Fit(bin.Midpoint, bin.Count);
            }
        }
    }

    public static Histogram Merge(Histogram a, Histogram b)
    {
        var c = new Histogram(a);
        c.Merge(b);
        return c;
    }

    public void Reset()
    {
        Count = 0;
        foreach (var bin in Bins)
        {
            bin.Reset();
        }
        _outOfBounds.Reset();
    }

    public nint NumValuesLessThan(double value)
    {
        return (nint) Bins.Where(bin => bin.Upper < value).Sum(bin => bin.Count);
    }

    public nint NumValuesLessThanOrEqualTo(double value)
    {
        return (nint) Bins.Where(bin => bin.Upper <= value).Sum(bin => bin.Count);
    }

    public nint NumValuesGreaterThan(double value)
    {
        return (nint) Bins.Where(bin => bin.Lower > value).Sum(bin => bin.Count);
    }

    public nint NumValuesGreaterThanOrEqualTo(double value)
    {
        return (nint) Bins.Where(bin => bin.Lower >= value).Sum(bin => bin.Count);
    }


    public IEnumerator<(string BinName, nint Count)> GetEnumerator()
    {
        return Bins.Select(bin => (bin.BinName, bin.Count)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"{typeof(Histogram)}(n={Count})";

    public void Print(StreamWriter stream)
    {
        Print(stream, '\t');
    }

    public void Print(StreamWriter stream, char sep)
    {
        stream.WriteLine($"{GetType()}(n={Count})");
        stream.WriteLine($"Bin{sep}Count");
        foreach (var (bin, count) in this)
        {
            stream.WriteLine($"{bin}{sep}{count}");
        }
    }
    

    private bool BinsAreMatching(ICollection<Bin> other)
    {
        return Bins.Count == other.Count && Bins.Zip(other).All(z => z.First.Equals(z.Second));
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

        
        private nint Lower { get; set; }
        private nint Upper { get; set; }
        public (nint, nint) Counts => (Lower, Upper);

        
        public void Reset()
        {
            Lower = 0;
            Upper = 0;
        }

        public void Update(int index, nint k)
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

        public void Merge(OutOfBounds other)
        {
            Lower += other.Lower;
            Upper += other.Upper;
        }
    }
    
    private class Bin
    {
        public Bin(double lower, double upper, bool closedLeft, bool closedRight)
        {
            if (lower >= upper)
            {
                throw new ArgumentException("Lower bound must be strictly less than upper bound");
            }
            
            Lower = lower;
            Upper = upper;
            ClosedLeft = closedLeft;
            ClosedRight = closedRight;

            var leftBrace = ClosedLeft ? '[' : '(';
            var rightBrace = ClosedRight ? ']' : ')';
            BinName = $"{leftBrace}{Lower:F2}, {Upper:F2}{rightBrace}";

            Count = 0;
        }

        public Bin(Bin other) : this(other.Lower, other.Upper, other.ClosedLeft, other.ClosedRight)
        {
            Count = other.Count;
        }
        
        
        public nint Count { get; private set; }
        public string BinName { get; }
        public double Lower { get; }
        public double Upper { get; }
        public bool ClosedLeft { get; }
        public bool ClosedRight { get; }

        public double Midpoint => (Upper + Lower) / 2;


        public bool Contains(double value)
        {
            if (value < Lower || value > Upper)
            {
                return false;
            }

            if (Lower < value && value < Upper)
            {
                return true;
            }

            if (value.Equals(Lower))
            {
                return ClosedLeft;
            }

            if (value.Equals(Upper))
            {
                return ClosedRight;
            }

            return false;
        }

        public void Reset()
        {
            Count = 0;
        }

        public void Merge(Bin other)
        {
            Count += other.Count;
        }

        public void Increment(nint k = 1)
        {
            Count += k;
        }

        public override string ToString()
        {
            return $"Bin {BinName}, n={Count}";
        }

        public bool Equals(Bin other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Lower.Equals(other.Lower) 
                   && Upper.Equals(other.Upper) 
                   && ClosedLeft == other.ClosedLeft 
                   && ClosedRight == other.ClosedRight;
        }
    }
}