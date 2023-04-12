using System;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public sealed class EmpiricalCdf : AbstractRunningStatistic<double, EmpiricalCdf>
{
    private readonly Extrema _extrema;
    private readonly double[] _buffer, _value;

    

    public EmpiricalCdf(int numBins = 200)
    {
        NumBins = numBins;
        _value = new double[NumBins];
        _buffer = new double[NumBins];
        _extrema = new Extrema();
    }

    private EmpiricalCdf(EmpiricalCdf other)
    {
        NumBins = other.NumBins;
        _value = other._value.ToArray();
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
            _value[k] = Utils.Smooth(_value[k], _buffer[k], (double) NumBins / Nobs);
        }
    }
    
    public override void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            _value[i] = 0;
            _buffer[i] = 0;
        }

        _extrema.Reset();
    }
    
    public override EmpiricalCdf CloneEmpty()
    {
        return new EmpiricalCdf(NumBins);
    }

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
            _value[k] = Utils.Smooth(_value[k], empiricalCdf._value[k], (double) empiricalCdf.Nobs / Nobs);
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
        return _value[i];
    }
    
    public override string ToString() => $"{typeof(EmpiricalCdf)} Nobs={Nobs} | NumBins={NumBins}";
}
