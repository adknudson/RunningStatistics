using System;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public sealed class EmpiricalCdf : RunningStatisticBase<double, EmpiricalCdf>
{
    /// <summary>
    /// Square root of double machine precision
    /// </summary>
    private const double Tolerance = 1.4901161193847656e-8;
    
    private readonly Extrema _extrema;
    private readonly double[] _buffer, _values;
    

    public EmpiricalCdf(int numBins = 200)
    {
        NumBins = numBins;
        _values = new double[NumBins];
        _buffer = new double[NumBins];
        _extrema = new Extrema();
    }

    private EmpiricalCdf(EmpiricalCdf other)
    {
        NumBins = other.NumBins;
        _values = other._values.ToArray();
        _buffer = other._buffer.ToArray();
        _extrema = other._extrema.Clone();
    }


    
    public double Median => Quantile(0.5);
    
    public double Min => _extrema.Min;
    
    public double Max => _extrema.Max;
    
    private int NumBins { get; }


    
    protected override long GetNobs() => _extrema.Nobs;

    public override void Fit(double value)
    {
        var i = Nobs % NumBins;
        var bufferCount = i + 1;
        _buffer[i] = value;
        
        _extrema.Fit(value);

        if (bufferCount < NumBins) return;
        
        Array.Sort(_buffer);
        for (var k = 0; k < NumBins; k++)
        {
            _values[k] = Utils.Smooth(_values[k], _buffer[k], (double) NumBins / Nobs);
        }
    }
    
    public override void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            _values[i] = 0;
            _buffer[i] = 0;
        }

        _extrema.Reset();
    }
    
    public override EmpiricalCdf CloneEmpty() => new(NumBins);

    public override EmpiricalCdf Clone() => new(this);

    public override void Merge(EmpiricalCdf empiricalCdf)
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
            _values[k] = Utils.Smooth(_values[k], empiricalCdf._values[k], (double) empiricalCdf.Nobs / Nobs);
        }
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

    /// <summary>
    /// The probability that X will take on a value less than or equal to x.
    /// </summary>
    public double Cdf(double x)
    {
        if (x <= Min) return 0.0;
        if (x >= Max) return 1.0;
        
        // find the value in _value closest to x
        var deltas = _values.Select(v => Math.Abs(v - x)).ToList();
        var min = deltas.Min();
        var i = deltas.IndexOf(min);

        if (Math.Abs(x - _values[i]) < Tolerance) return (double)i / (NumBins - 1);

        double j;
        
        if (x < _values[i]) // ___V[i-1]_____x___V[i]___
        {
            if (i == 0) return 0;
            j = (x - _values[i - 1]) / (_values[i] - _values[i - 1]) + i - 1;
        }
        else // ___V[i]__x_____V[i+1]___
        {
            if (i == _values.Length - 1) return 1.0;
            j = (x - _values[i]) / (_values[i + 1] - _values[i]) + i;
        }

        return j / (NumBins - 1);
    }
    
    public override string ToString() => $"{typeof(EmpiricalCdf)} Nobs={Nobs} | NumBins={NumBins}";
}
