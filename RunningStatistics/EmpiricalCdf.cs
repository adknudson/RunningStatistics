using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public class EmpiricalCdf : IRunningStatistic<double, EmpiricalCdf>
{
    private readonly int _numBins;
    private readonly Extrema _extrema;
    private readonly double[] _buffer, _values;

    

    public EmpiricalCdf(int numBins = 200)
    {
        _numBins = numBins;
        _values = new double[_numBins];
        _buffer = new double[_numBins];
        _extrema = new Extrema();
    }

    public EmpiricalCdf(EmpiricalCdf other)
    {
        _numBins = other._numBins;
        
        _values = new double[_numBins];
        _buffer = new double[_numBins];
        other._values.CopyTo(_values, 0);
        other._buffer.CopyTo(_buffer, 0);
        
        _extrema = new Extrema(other._extrema);
    }
    


    public long Count => _extrema.Count;
    public double Median => Quantile(0.5);
    public double Min => _extrema.Min;
    public double Max => _extrema.Max;


    public double Quantile(double p) => SortedQuantile(p);
    
    public void Merge(EmpiricalCdf other)
    {
        if (_numBins != other._numBins)
        {
            throw new Exception($"The two {nameof(EmpiricalCdf)} objects must have the same batch size. " +
                                $"Got {_numBins} and {other._numBins}.");
        }

        _extrema.Merge(other._extrema);
        
        if (Count == 0) return;
        
        for (var k = 0; k < _numBins; k++)
        {
            _values[k] = Utils.Smooth(_values[k], other._values[k], (double)other.Count / Count);
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
        _extrema.Fit(value);
        _buffer[i] = value;

        // if the buffer is full, then merge buffer into value vector
        if (i == _numBins - 1)
        {
            Array.Sort(_buffer);
            for (var k = 0; k < _numBins; k++)
            {
                _values[k] = Utils.Smooth(_values[k], _buffer[k], (double) _numBins / Count);
            }
        }
        
    }

    public void Reset()
    {
        for (var i = 0; i < _numBins; i++)
        {
            _values[i] = 0;
            _buffer[i] = 0;
        }
        _extrema.Reset();
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
}