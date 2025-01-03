﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class Variance : RunningStatisticBase<double, Variance>
{
    private double _mean, _variance;
    private long _nobs;

    
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
    
    public double StandardDeviation => Math.Sqrt(Value);


    protected override long GetNobs() => _nobs;
    
    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        ys.ForEach(Require.Finite);
        UncheckedFit(ys);
    }

    internal void UncheckedFit(double value, long count)
    {
        _nobs += count;
        
        var mu = _mean;
        var g = (double) count / Nobs;
        
        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, (value - _mean) * (value - mu), g);
    }

    internal void UncheckedFit(List<double> values)
    {
        _nobs += values.Count;

        var mean = values.Average();
        var variance = Utils.Variance(values, mean);
        var g = (double) values.Count / Nobs;
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