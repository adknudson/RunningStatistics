using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The running mean of the observations fitted so far.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
{
    private long _nobs;
    private double _value;


    public override long Nobs => _nobs;

    /// <summary>
    /// Gets the current average value.
    /// </summary>
    public double Value => Nobs < 1 ? double.NaN : _value;


    public override void Fit(double value)
    {
        Require.Finite(value);
        UncheckedFit(value);
    }

    public override void UncheckedFit(double value) => UncheckedFit(value, 1);

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }
    
    public override void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }
        _value = Utils.Smooth(_value, value, (double)count / Nobs);
    }

    public override void Reset()
    {
        _nobs = 0;
        _value = 0;
    }

    public override Mean CloneEmpty() => new();

    public override void Merge(Mean other)
    {
        checked { _nobs += other.Nobs; }
        _value = Nobs == 0 ? 0 : Utils.Smooth(_value, other._value, (double)other.Nobs / Nobs);
    }

    public override string ToString() => base.ToString() + $" | μ={Value}";
}