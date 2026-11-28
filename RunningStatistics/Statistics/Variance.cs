using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// Represents a running variance statistic. Returns the unbiased sample variance.
/// </summary>
public sealed class Variance : RunningStatisticBase<double, Variance>
{
    private double _mean, _variance;
    private long _nobs;


    public override long Nobs => _nobs;

    /// <summary>
    /// Gets the unbiased sample variance. Returns <see cref="double.NaN"/> if the number of
    /// observations is less than 2.
    /// </summary>
    public double Value => Nobs < 2 ? double.NaN : _variance * Utils.Bessel(Nobs);

    /// <summary>
    /// Gets the mean of the observations. Returns <see cref="double.NaN"/> if there are no
    /// observations.
    /// </summary>
    public double Mean => Nobs > 0 ? _mean : double.NaN;

    /// <summary>
    /// The corrected sample standard deviation. Returns <see cref="double.NaN"/> if the number of
    /// observations is less than 2.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Value);

    /// <summary>
    /// Gets the raw value of the mean.
    /// </summary>
    internal double RawMean => _mean;

    /// <summary>
    /// Gets the raw, uncorrected variance of the observations.
    /// </summary>
    internal double RawVariance => _variance;


    public override void Fit(double value)
    {
        Require.Finite(value);
        UncheckedFit(value, 1);
    }

    public override void UncheckedFit(double value)
    {
        UncheckedFit(value, 1);
    }
    
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
        
        var mu = _mean;
        var g = (double) count / Nobs;
        
        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, (value - _mean) * (value - mu), g);
    }

    public override void Reset()
    {
        _nobs = 0;
        _mean = 0;
        _variance = 0;
    }

    public override Variance CloneEmpty() => new();
    
    public override void Merge(Variance variance)
    {
        checked { _nobs += variance.Nobs; }

        if (Nobs == 0) return;

        var g = (double) variance.Nobs / Nobs;
        var delta = _mean - variance._mean;

        _mean = Utils.Smooth(_mean, variance._mean, g);
        _variance = Utils.Smooth(_variance, variance._variance, g) + delta * delta * g * (1 - g);
    }
    
    public static explicit operator double(Variance variance) => variance.Value;

    public override string ToString() => base.ToString() + $" | s²={Value}";
}