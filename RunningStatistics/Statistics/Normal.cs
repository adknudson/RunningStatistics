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
        Require.NotNull(values);
        var ys = values.ToList();
        if (ys.Count == 0) return;
        ys.ForEach(Require.Finite);
        UncheckedFit(ys);
    }

    // One count update and one blend weight drive all three fields. The
    // spread-sum update reads the old and the new mean of the variance
    // route, and that form matches Variance.UncheckedFit. The exposed
    // mean takes its own blend with the same weight.
    private void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }

        var mu = _varianceMean;
        var g = (double)count / Nobs;

        _mean = Utils.Smooth(_mean, value, g);
        _varianceMean = Utils.Smooth(_varianceMean, value, g);
        _variance = Utils.Smooth(_variance, (value - _varianceMean) * (value - mu), g);
    }

    // Each blend uses its own batch result. The exposed mean blends with
    // the compensated batch average. The variance route blends with the
    // plain batch average and the population variance. The update also
    // adds the mean-shift term delta * delta * g * (1 - g).
    private void UncheckedFit(List<double> values)
    {
        checked { _nobs += values.Count; }

        var mean = values.Average();
        var variance = ComputeVariance(values, mean);
        var blended = BatchAverage(values);
        var g = (double)values.Count / Nobs;
        var delta = _varianceMean - mean;

        _mean = Utils.Smooth(_mean, blended, g);
        _varianceMean = Utils.Smooth(_varianceMean, mean, g);
        _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
    }

    /// <summary>
    /// The average of the batch that the batch path blends into the exposed
    /// mean. The fold keeps a correction beside its total, and updates that
    /// correction one observation at a time. A canceling batch keeps the
    /// ones a plain fold would drop, once the running total's spacing
    /// exceeds them. The fold matches the Mean class batch fold to the last
    /// bit.
    /// </summary>
    internal double BatchAverage(List<double> values)
    {
        var total = 0.0;
        var correction = 0.0;
        foreach (var x in values)
        {
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

        return (total + correction) / values.Count;
    }

    // The method returns the population variance of the batch. The mean
    // argument is the plain batch average. This matches the batch
    // variance path of the Variance class.
    private static double ComputeVariance(List<double> xs, double mean)
    {
        return xs.Average(s => (s - mean) * (s - mean));
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
        var d = (x - Mean) / StandardDeviation;
        return Math.Exp(-0.5 * d * d) / (Constants.Sqrt2Pi * StandardDeviation);
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
