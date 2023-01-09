using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public class EmpiricalCdf : IRunningStatistic<double, double[], EmpiricalCdf>
{
    private readonly Extrema _extrema;
    private readonly double[] _buffer;

    

    public EmpiricalCdf(int numBins = 200)
    {
        NumBins = numBins;
        Value = new double[NumBins];
        _buffer = new double[NumBins];
        _extrema = new Extrema();
    }

    private EmpiricalCdf(EmpiricalCdf other)
    {
        NumBins = other.NumBins;
        Value = other.Value.ToArray();
        _buffer = other._buffer.ToArray();
        _extrema = other._extrema.Clone();
    }


    
    public long Nobs => _extrema.Nobs;
    
    public double[] Value { get; }

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
        var i = (Nobs % NumBins);
        var bufferCount = i + 1;
        _buffer[i] = value;
        
        _extrema.Fit(value);

        if (bufferCount < NumBins) return;
        
        Array.Sort(_buffer);
        for (var k = 0; k < NumBins; k++)
        {
            Value[k] = Utils.Smooth(Value[k], _buffer[k], (double) NumBins / Nobs);
        }
    }

    public EmpiricalCdf CloneEmpty()
    {
        return new EmpiricalCdf(NumBins);
    }

    public EmpiricalCdf Clone() => new(this);

    public void Merge(EmpiricalCdf empiricalCdf)
    {
        if (NumBins != empiricalCdf.NumBins)
        {
            throw new Exception($"The two {nameof(EmpiricalCdf)} objects must have the same batch size. " +
                                $"Got {NumBins} and {empiricalCdf.NumBins}.");
        }

        _extrema.Merge(empiricalCdf._extrema);

        if (Nobs == 0) return;

        for (var k = 0; k < NumBins; k++)
        {
            Value[k] = Utils.Smooth(Value[k], empiricalCdf.Value[k], (double) empiricalCdf.Nobs / Nobs);
        }
    }

    public static EmpiricalCdf Merge(EmpiricalCdf first, EmpiricalCdf second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            Value[i] = 0;
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
        return Value[i];
    }
    
    public override string ToString() => $"{typeof(EmpiricalCdf)}(n={Nobs})";
}
