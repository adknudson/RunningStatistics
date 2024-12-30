using System;
using System.Linq;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace RunningStatistics;

/// <summary>
/// Approximate order statistics (CDF) with batches of a given size.
/// </summary>
public sealed class EmpiricalCdf : RunningStatisticBase<double, EmpiricalCdf>
{
    private readonly double[] _buffer;
    private readonly double[] _values;
    private readonly Extrema _extrema;
    
    
    public EmpiricalCdf(int numBins = 200)
    {
        if (numBins < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numBins), numBins, "The number of bins must be >=2.");
        }
        
        NumBins = numBins;
        _buffer = new double[NumBins];
        _values = new double[NumBins + 2];
        _extrema = new Extrema();
    }

    private EmpiricalCdf(EmpiricalCdf other)
    {
        NumBins = other.NumBins;
        _buffer = other._buffer.ToArray();
        _extrema = other._extrema.Clone();
        _values = other._values.ToArray();
    }


    /// <summary>
    /// A reference to the values excluding the min and max. The min and max are handled by the
    /// <see cref="Extrema"/> object.
    /// </summary>
    private Span<double> Values => _values.AsSpan(1, NumBins);
    
    private int NumBins { get; }
    
    public double Median => Quantile(0.5);
    
    public double Min => _extrema.Min;
    
    public double Max => _extrema.Max;

    
    protected override long GetNobs() => _extrema.Nobs;

    public override void Fit(double value)
    {
        Require.Finite(value);
        
        var i = Nobs % NumBins;
        var bufferCount = i + 1;
        _buffer[i] = value;
        
        _extrema.Fit(value);

        if (bufferCount < NumBins) return;
        
        Array.Sort(_buffer);
        
        for (var k = 0; k < NumBins; k++)
        {
            Values[k] = Utils.Smooth(Values[k], _buffer[k], (double) NumBins / Nobs);
        }
    }
    
    public override void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            Values[i] = 0;
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
            Values[k] = Utils.Smooth(Values[k], empiricalCdf.Values[k], (double) empiricalCdf.Nobs / Nobs);
        }
    }
    
    /// <summary>
    /// Finds the pth quantile of the empirical CDF.
    /// </summary>
    public double Quantile(double p)
    {
        Require.ValidProbability(p);

        if (p == 0)
        {
            return Min;
        }

        if (p == 1)
        {
            return Max;
        }

        // [min, V[0], V[1], ..., V[NumBins - 1], max]
        var i = (NumBins - 1 + 2) * p;
        var r = i % 1; // fractional part of i
        
        // if i is less than 1, return the interpolation between min and V[0]
        if (i < 1)
        {
            return Utils.Smooth(Min, Values[0], r);
        }
        
        // if i is greater than NumBins, return the interpolation between V[^1] and max
        if (i >= NumBins)
        {
            return Utils.Smooth(Values[^1], Max, r);
        }
        
        // otherwise, return the interpolation between V[floor(i)] and V[ceil(i)]
        var fi = (int)Math.Floor(i) - 1;
        var ci = (int)Math.Ceiling(i) - 1;
        return Utils.Smooth(Values[fi], Values[ci], r);
    }

    /// <summary>
    /// The probability that X will take on a value less than or equal to x.
    /// </summary>
    public double Cdf(double x)
    {
        if (x < Min)
        {
            return 0;
        }
        
        if (x >= Max)
        {
            return 1;
        }
        
        // set the min and max values. The rest are already set due to the Values span.
        _values[0] = Min;
        _values[^1] = Max;
        
        // find the index of the first value greater than x
        // ___v[i]___x___v[i+1]___
        var i = Array.FindIndex(_values, v => v > x);
        
        // x is now between v[i-1] and v[i]
        
        // get the fractional amount between v[i-1] and v[i]
        var r = (x - _values[i - 1]) / (_values[i] - _values[i - 1]);
        
        // return the interpolation between v[i-1] and v[i]
        return Utils.Smooth(i - 1, i, r) / _values.Length;
    }

    protected override string GetStatsString()
    {
        return $"NumBins={NumBins}, Min={Min}, Max={Max}";
    }
}
