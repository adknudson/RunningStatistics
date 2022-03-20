using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public class EmpiricalCdf : IRunningStat<double, EmpiricalCdf>
{
    private readonly int _numBins;
    private readonly Extrema _extrema;
    private IList<double> _values, _buffer;
    
    
    
    
    public EmpiricalCdf(int numBins = 200)
    {
        Count = 0;
        _numBins = numBins;
        _values = Enumerable.Repeat(0.0, _numBins).ToList();
        _buffer = Enumerable.Repeat(0.0, _numBins).ToList();
        _extrema = new Extrema();
    }

    public EmpiricalCdf(EmpiricalCdf other)
    {
        Count = other.Count;
        _numBins = other._numBins;
        _values = new List<double>(other._values);
        _buffer = new List<double>(other._buffer);
        _extrema = new Extrema(other._extrema);
    }
    
    
    
    public long Count { get; private set; }
    public double Median => Quantile(0.5);
    public double Min => _extrema.Min;
    public double Max => _extrema.Max;
    public double this[double p] => SortedQuantile(p);


    private double Quantile(double p) => SortedQuantile(p);
    
    public void Merge(EmpiricalCdf other)
    {
        if (_numBins != other._numBins)
        {
            throw new Exception($"The two {nameof(EmpiricalCdf)} objects must have the same batch size. " +
                                $"Got {_numBins} and {other._numBins}.");
        }

        Count += other.Count;
        _extrema.Merge(other._extrema);
        for (var k = 0; k < _numBins; k++)
        {
            _values[k] = Utils.Smooth(_values[k], other._values[k], (double)Count / other.Count);
        }
    }

    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value)
    {
        var i = (int)(Count % _numBins);

        Count += 1;
        _extrema.Fit(value);
        _buffer[i] = value;

        if (i + 1 != _numBins) return;
        
        _buffer = _buffer.OrderBy(t => t).ToList();
        for (var k = 0; k < _numBins; k++)
        {
            _values[k] = Utils.Smooth(_values[k], _buffer[k], (double)_numBins / Count);
        }
    }

    public void Reset()
    {
        Count = 0;
        _values = _values.Select(_ => 0.0).ToList();
        _buffer = _values.Select(_ => 0.0).ToList();
        _extrema.Reset();
    }

    public void Print(StreamWriter stream)
    {
        var quantileValue = Enumerable.Range(0, _numBins).Select(i => (double) i / (_numBins - 1)).Zip(_values);
        
        stream.WriteLine(ToString());
        stream.WriteLine("Quantile\tValue");
        foreach (var (quantile, value) in quantileValue)
        {
            stream.WriteLine($"{quantile:F3}\t{value}");
        }
    }

    public override string ToString() => $"{typeof(EmpiricalCdf)}(n={Count})";

    /// <summary>
    /// Finds the pth quantile of a collection that is assumed to be sorted.
    /// </summary>
    private double SortedQuantile(double p)
    {
        if (p is < 0 or > 1)
        {
            throw new Exception($"p must be in range [0, 1]. Got {p}.");
        }

        var i = (int)Math.Floor((_numBins - 1) * p);
        return _values.ElementAt(i);
    }

    private static EmpiricalCdf Merge(EmpiricalCdf a, EmpiricalCdf b)
    {
        var merged = new EmpiricalCdf(a);
        merged.Merge(b);
        return merged;
    }
    public static EmpiricalCdf operator +(EmpiricalCdf a, EmpiricalCdf b) => Merge(a, b);
}