using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class Variance : RunningStatisticBase<double, Variance>
{
    private double _mean, _variance;
    private long _nobs;

    
    /// <summary>
    /// Returns the bias-corrected variance.
    /// </summary>
    public double Value
    {
        get
        {
            return Nobs switch
            {
                0 => double.NaN,
                1 => double.IsInfinity(_mean) ? double.NaN : 0.0,
                _ => _variance * Utils.Bessel(Nobs)
            };
        }
    }
    
    /// <summary>
    /// The sample standard deviation.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Value);


    protected override long GetNobs() => _nobs;
    
    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        
        _nobs += count;
        
        var mu = _mean;
        var g = (double) count / Nobs;
        
        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, (value - _mean) * (value - mu), g);
    }

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        ys.ForEach(Require.Finite);
        
        _nobs += ys.Count;

        var mean = ys.Average();
        var variance = Utils.Variance(ys, mean);
        var g = (double) ys.Count / Nobs;
        var delta = _mean - mean;

        _mean = Utils.Smooth(_mean, mean, g);
        _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
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
        _nobs += variance.Nobs;

        if (Nobs == 0) return;

        var g = (double) variance.Nobs / Nobs;
        var delta = _mean - variance._mean;

        _mean = Utils.Smooth(_mean, variance._mean, g);
        _variance = Utils.Smooth(_variance, variance._variance, g) + delta * delta * g * (1 - g);
    }
    
    public static explicit operator double(Variance variance) => variance.Value;

    protected override string GetStatsString() => $"σ²={Value}";
}