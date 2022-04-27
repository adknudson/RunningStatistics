using System;
using System.Collections.Generic;
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


    public long Count => _extrema.Count;
    public double Median => Quantile(0.5);
    public double Min => _extrema.Min;
    public double Max => _extrema.Max;
    internal int NumBins { get; }


    public double Quantile(double p) => SortedQuantile(p);

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

    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value)
    {
        var i = (int) (Count % NumBins);
        _extrema.Fit(value);
        _buffer[i] = value;

        // if the buffer is full, then merge buffer into value vector
        if (i == NumBins - 1)
        {
            Array.Sort(_buffer);
            for (var k = 0; k < NumBins; k++)
            {
                _values[k] = Utils.Smooth(_values[k], _buffer[k], (double) NumBins / Count);
            }
        }
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

    public override string ToString() => $"{typeof(EmpiricalCdf)}(n={Count})";

    /// <summary>
    /// Finds the pth quantile of a collection that is assumed to be sorted.
    /// </summary>
    private double SortedQuantile(double p)
    {
        if (p is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException($"p must be in range [0, 1]. Got {p}.");
        }

        var i = (int) Math.Floor((NumBins - 1) * p);
        return _values.ElementAt(i);
    }
}