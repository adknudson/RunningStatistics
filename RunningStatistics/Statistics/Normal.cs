using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a running statistic for a normal distribution. I.e., it keeps track of the mean and
/// variance of a set of values. The class returns the variance as the unbiased sample variance.
/// </summary>
public sealed class Normal : RunningStatisticBase<double, Normal>
{
    private double _mean, _varianceMean, _variance;
    private long _nobs;


    public override long Nobs => _nobs;

    /// <summary>
    /// Gets the mean of the distribution.
    /// </summary>
    public double Mean => Nobs < 1 ? double.NaN : _mean;
    
    /// <summary>
    /// Gets the unbiased sample variance of the distribution.
    /// </summary>
    public double Variance => Nobs < 2 ? double.NaN : _variance * Utils.Bessel(Nobs);
    
    /// <summary>
    /// Gets the corrected sample standard deviation of the distribution.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Variance);


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

        var mu = _varianceMean;
        var g = (double)count / Nobs;

        _mean = Utils.Smooth(_mean, value, g);
        _varianceMean = Utils.Smooth(_varianceMean, value, g);
        _variance = Utils.Smooth(_variance, (value - _varianceMean) * (value - mu), g);
    }

    public override void Fit(IEnumerable<double> values)
    {
        // A List is re-iterable: the two fold passes and the validation walk can use the
        // caller's own storage. Any other iterable may be a one-shot generator, so only
        // those have to be buffered before the second pass.
        var ys = values.ToList();
        if (ys.Count == 0) return;
        ys.ForEach(Require.Finite);
        UncheckedFit(ys);
    }

    // The unchecked twin: the caller vouches for the values, so the batch goes straight
    // into the compensated fold without the validation pass.
    public override void UncheckedFit(IEnumerable<double> values)
    {
        UncheckedFit([.. values]);
    }

    // Each blend uses its own batch result. The exposed mean blends with
    // the compensated batch average. The variance route blends with the
    // plain batch average and the population variance. The update also
    // adds the mean-shift term delta * delta * g * (1 - g).
    private void UncheckedFit(List<double> values)
    {
        checked { _nobs += values.Count; }

        // One traversal carries both averages. Its running total is the plain fold, and the
        // correction held beside that total is what makes the other one compensated, so the
        // two folds share the total instead of walking the batch twice.
        var total = 0.0;
        var correction = 0.0;
        for (var i = 0; i < values.Count; i++)
        {
            var x = values[i];
            var t = total + x;
            // A saturated total would send infinities through the two-sum below and turn the
            // batch average into NaN, so the collection step is skipped there and the
            // infinity stays, the way the plain fold averaged it before.
            if (Math.Abs(t) < double.PositiveInfinity)
            {
                // Knuth two-sum: these six operations return exactly the bits that the
                // rounded addition above lost. The branch-free form matters on mixed-magnitude
                // batches, where picking a formula by magnitude mispredicts often.
                var bvirt = t - total;
                var aver = t - bvirt;
                correction += (total - aver) + (x - bvirt);
            }

            total = t;
        }

        var mean = total / values.Count;
        var blended = (total + correction) / values.Count;
        var variance = ComputeVariance(values, mean);
        var g = (double)values.Count / Nobs;
        var delta = _varianceMean - mean;

        _mean = Utils.Smooth(_mean, blended, g);
        _varianceMean = Utils.Smooth(_varianceMean, mean, g);
        _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
    }

    // The method returns the population variance of the batch. The mean
    // argument is the plain batch average. This matches the batch
    // variance path of the Variance class. The fold is written out because the
    // LINQ form drives a delegate once per observation, about seven times the cost of the
    // arithmetic below.
    private static double ComputeVariance(List<double> xs, double mean)
    {
        var total = 0.0;
        var n = xs.Count;
        for (var i = 0; i < n; i++)
        {
            var d = xs[i] - mean;
            total += d * d;
        }

        return total / n;
    }

    public override void Reset()
    {
        _nobs = 0;
        _mean = 0;
        _varianceMean = 0;
        _variance = 0;
    }

    public override Normal CloneEmpty() => new();

    // One count update and one blend weight drive both routes. A merged
    // count of zero means the receiver and the source are both empty.
    // The class then keeps the zero state of a new statistic.
    public override void Merge(Normal normal)
    {
        checked { _nobs += normal.Nobs; }
        if (Nobs == 0)
        {
            _mean = 0;
            return;
        }

        var g = (double)normal.Nobs / Nobs;
        var delta = _varianceMean - normal._varianceMean;

        _mean = Utils.Smooth(_mean, normal._mean, g);
        _varianceMean = Utils.Smooth(_varianceMean, normal._varianceMean, g);
        _variance = Utils.Smooth(_variance, normal._variance, g) + delta * delta * g * (1 - g);
    }

    /// <summary>
    /// Computes the probability density function (PDF) for a given value.
    /// </summary>
    public double Pdf(double x)
    {
        // Read the standard deviation once: the getter carries a virtual count check
        // and a square root, and both uses here want the same value.
        var s = StandardDeviation;
        var d = (x - Mean) / s;
        return Math.Exp(-0.5 * d * d) / (Constants.Sqrt2Pi * s);
    }

    /// <summary>
    /// Computes the cumulative distribution function (CDF) for a given value.
    /// </summary>
    public double Cdf(double x)
    {
        return 0.5 * SpecialFunctions.Erfc((Mean - x) / (StandardDeviation * Constants.Sqrt2));
    }

    /// <summary>
    /// Computes the quantile function for a given probability.
    /// </summary>
    /// <param name="p">The probability to compute the quantile for.</param>
    /// <returns>The quantile value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="p"/> is not
    /// in the interval [0, 1].</exception>
    public double Quantile(double p)
    {
        Require.ValidProbability(p);
        return Mean - StandardDeviation * Constants.Sqrt2 * SpecialFunctions.ErfcInv(2.0 * p);
    }

    public override string ToString() => base.ToString() + $" | μ={Mean}, σ²={Variance}";
}
