﻿using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class Variance : AbstractRunningStatistic<double, Variance>
{
    private double _mean, _variance;

    
    /// <summary>
    /// Returns the bias-corrected variance.
    /// </summary>
    public double Value
    {
        get
        {
            if (Nobs > 1) return _variance * Utils.BesselCorrection(Nobs);
            return double.IsInfinity(_mean) ? double.NaN : 1.0;
        }
    }

    
    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Nobs += ys.Count;

        var mean = ys.Average();
        var variance = Utils.Variance(ys, mean);
        var g = (double) ys.Count / Nobs;
        var delta = _mean - mean;

        _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
        _mean = Utils.Smooth(_mean, mean, g);
    }

    public override void Fit(double value)
    {
        Nobs += 1;
        var mu = _mean;
        var g = 1.0 / Nobs;
        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, (value - _mean) * (value - mu), g);
    }

    public override void Reset()
    {
        Nobs = 0;
        _mean = 0;
        _variance = 0;
    }

    public override Variance CloneEmpty() => new();

    public override Variance Clone()
    {
        return new Variance
        {
            Nobs = Nobs,
            _mean = _mean,
            _variance = _variance
        };
    }
    
    public override void Merge(Variance variance)
    {
        Nobs += variance.Nobs;

        if (Nobs <= 0) return;

        var g = (double) variance.Nobs / Nobs;
        var delta = _mean - variance._mean;

        _variance = Utils.Smooth(_variance, variance._variance, g) + delta * delta * g * (1 - g);
        _mean = Utils.Smooth(_mean, variance._mean, g);
    }
    
    public override string ToString() => $"{typeof(Variance)} Nobs={Nobs} | σ²={Value}";

    public static explicit operator double(Variance variance) => variance.Value;
}