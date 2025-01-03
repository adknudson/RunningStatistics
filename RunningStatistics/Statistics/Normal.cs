using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a running statistic for a normal distribution. I.e., it keeps track of the mean and
/// variance of a set of values. The variance is returned as the unbiased sample variance.
/// </summary>
public sealed class Normal : RunningStatisticBase<double, Normal>
{
    private readonly Mean _mean = new();
    private readonly Variance _variance = new();

    /// <summary>
    /// Gets the mean of the distribution.
    /// </summary>
    public double Mean => _mean.Value;
    
    /// <summary>
    /// Gets the unbiased sample variance of the distribution.
    /// </summary>
    public double Variance => _variance.Value;
    
    /// <summary>
    /// Gets the corrected sample standard deviation of the distribution.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Variance);
    
    
    protected override long GetNobs() => _mean.Nobs;

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
    
    private void UncheckedFit(double value, long count)
    {
        _mean.UncheckedFit(value, count);
        _variance.UncheckedFit(value, count);
    }

    private void UncheckedFit(List<double> values)
    {
        _mean.UncheckedFit(values);
        _variance.UncheckedFit(values);
    }

    public override void Reset()
    {
        _mean.Reset();
        _variance.Reset();
    }

    public override Normal CloneEmpty() => new();
    
    public override void Merge(Normal normal)
    {
        _mean.Merge(normal._mean);
        _variance.Merge(normal._variance);
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

    protected override string GetStatsString() => $"μ={Mean}, σ²={Variance}";
}