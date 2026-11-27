using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a running variance statistic. Returns the unbiased sample variance.
/// </summary>
public sealed class Variance : RunningStatisticBase<double, Variance>
{
    private double _mean, _variance;
    private long _nobs;

    /// <summary>
    /// Gets the unbiased sample variance. Returns <see cref="double.NaN"/> if the number of
    /// observations is less than 2.
    /// </summary>
    public double Value => Nobs < 2 ? double.NaN : _variance * Utils.Bessel(Nobs);

    /// <summary>
    /// The corrected sample standard deviation. Returns <see cref="double.NaN"/> if the number of
    /// observations is less than 2.
    /// </summary>
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
        if (ys.Count == 0) return;
        ys.ForEach(Require.Finite);
        UncheckedFit(ys);
    }

    internal void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }
        
        var mu = _mean;
        // Fit validated count and returned on zero, so Nobs >= count > 0 and the weight
        // count / Nobs is a ratio of non-negative counts in (0, 1]: blend unchecked,
        // same bits as the guarded form.
        var g = (double) count / Nobs;
        
        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, (value - _mean) * (value - mu), g);
    }

    internal void UncheckedFit(List<double> values)
    {
        checked { _nobs += values.Count; }

        var mean = values.Average();
        var variance = ComputeVariance(values, mean);
        // The batch is non-empty and Nobs absorbed its count, so values.Count / Nobs is
        // a ratio of non-negative counts in (0, 1].
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
        checked { _nobs += variance.Nobs; }

        if (Nobs == 0) return;

        // Nobs is the merged count, at least variance.Nobs and positive, so variance.Nobs
        // / Nobs is a ratio of non-negative counts in (0, 1].
        var g = (double) variance.Nobs / Nobs;
        var delta = _mean - variance._mean;

        _mean = Utils.Smooth(_mean, variance._mean, g);
        _variance = Utils.Smooth(_variance, variance._variance, g) + delta * delta * g * (1 - g);
    }
    
    public static explicit operator double(Variance variance) => variance.Value;
    
    private static double ComputeVariance(List<double> xs, double mean)
    {
        return xs.Average(s => (s - mean) * (s - mean));
    }

    public override string ToString() => base.ToString() + $" | s²={Value}";
}