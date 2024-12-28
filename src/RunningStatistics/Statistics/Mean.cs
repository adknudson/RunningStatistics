using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean, stored as a <see cref="double"/>.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
{
    private double _value;
    private long _nobs;
    
    
    public double Value => Nobs == 0 ? double.NaN : _value;


    protected override long GetNobs() => _nobs;

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        _nobs += ys.Count;
        _value = Utils.Smooth(_value, ys.Average(), (double)ys.Count / Nobs);
    }

    public override void Fit(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value must be a finite number", nameof(value));
        }
        
        _nobs++;
        _value = Utils.Smooth(_value, value, 1.0 / Nobs);
    }

    public override void Reset()
    {
        _nobs = 0;
        _value = 0;
    }

    public override Mean CloneEmpty() => new();

    public override Mean Clone()
    {
        return new Mean
        {
            _nobs = Nobs,
            _value = Value
        };
    }
    public override void Merge(Mean mean)
    {
        _nobs += mean.Nobs;
        _value = Nobs == 0 ? 0 : Utils.Smooth(_value, mean._value, (double)mean.Nobs / Nobs);
    }
    
    public static explicit operator double(Mean mean) => mean.Value;

    public override string ToString() => $"{typeof(Mean)} Nobs={Nobs} | μ={Value}";
}