using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public class EmpiricalCdf : IRunningStatistic<double>
{
    private readonly Extrema _extrema;
    private readonly double[] _buffer, _values;


    public EmpiricalCdf(int numBins = 200)
    {
        NumBins = numBins;
        _values = new double[NumBins];
        _buffer = new double[NumBins];
        _extrema = new Extrema();
    }

    public EmpiricalCdf(EmpiricalCdf other)
    {
        NumBins = other.NumBins;
        _values = other._values;
        _buffer = other._buffer;
        _extrema = new Extrema(other._extrema);
    }


    public long Count => _extrema.Count;
    public double Median => Quantile(0.5);
    public double Min => _extrema.Min;
    public double Max => _extrema.Max;
    private int NumBins { get; }

    
    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value)
    {
        var i = (Count % NumBins);
        var bufferCount = i + 1;
        _buffer[i] = value;
        
        _extrema.Fit(value);

        if (bufferCount < NumBins) return;
        
        Array.Sort(_buffer);
        for (var k = 0; k < NumBins; k++)
        {
            _values[k] = Utils.Smooth(_values[k], _buffer[k], (double) NumBins / Count);
        }
    }
    
    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not EmpiricalCdf empiricalCdf) return;
        
        if (NumBins != empiricalCdf.NumBins)
        {
            throw new Exception($"The two {nameof(EmpiricalCdf)} objects must have the same batch size. " +
                                $"Got {NumBins} and {empiricalCdf.NumBins}.");
        }

        _extrema.Merge(empiricalCdf._extrema);

        if (Count == 0) return;

        for (var k = 0; k < NumBins; k++)
        {
            _values[k] = Utils.Smooth(_values[k], empiricalCdf._values[k], (double) empiricalCdf.Count / Count);
        }
    }

    public static EmpiricalCdf Merge(EmpiricalCdf a, EmpiricalCdf b)
    {
        var c = new EmpiricalCdf(a);
        c.Merge(b);
        return c;
    }

    public void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            _values[i] = 0;
            _buffer[i] = 0;
        }

        _extrema.Reset();
    }


    /// <summary>
    /// Finds the pth quantile of the empirical CDF.
    /// </summary>
    public double Quantile(double p)
    {
        if (p is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException($"p must be in range [0, 1]. Got {p}.");
        }

        var i = (int) Math.Floor((NumBins - 1) * p);
        return _values[i];
    }
    
    public override string ToString() => $"{typeof(EmpiricalCdf)}(n={Count})";

    public void Print(StreamWriter stream)
    {
        Print(stream, NumBins);
    }

    public void Print(StreamWriter stream, char sep)
    {
        Print(stream, NumBins, sep);
    }

    public void Print(StreamWriter stream, int numQuantiles, char sep = '\t')
    {
        var quantiles = Enumerable.Range(0, numQuantiles)
            .Select(p => (double) p / (numQuantiles - 1))
            .ToList();
        var values = quantiles.Select(Quantile);
        
        stream.WriteLine($"{GetType()}(n={Count})");
        foreach (var (q, x) in quantiles.Zip(values))
        {
            stream.WriteLine($"{q:F3}{sep}{x}");
        }
    }
}
