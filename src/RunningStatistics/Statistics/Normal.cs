using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class Normal : RunningStatisticBase<double, Normal>
{
    private readonly Mean _mean;
    private readonly Variance _variance;

    
    public Normal()
    {
        _mean = new Mean();
        _variance = new Variance();
    }
    
    private Normal(Normal other)
    {
        _mean = other._mean.Clone();
        _variance = other._variance.Clone();
    }

    
    public double Mean => _mean.Value;
    
    /// <summary>
    /// The bias-corrected variance.
    /// </summary>
    public double Variance => _variance.Value;
    
    /// <summary>
    /// The sample standard deviation.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Variance);
    
    
    protected override long GetNobs() => _mean.Nobs;
    
    public override void Fit(IEnumerable<double> values)
    {
        var xs = values.ToList();
        _mean.Fit(xs);
        _variance.Fit(xs);
    }

    public override void Fit(double value)
    {
        _mean.Fit(value);
        _variance.Fit(value);
    }

    public override void Reset()
    {
        _mean.Reset();
        _variance.Reset();
    }

    public override Normal CloneEmpty() => new();

    public override Normal Clone() => new(this);

    public override void Merge(Normal normal)
    {
        _mean.Merge(normal._mean);
        _variance.Merge(normal._variance);
    }

    public double Pdf(double x)
    {
        var d = (x - Mean) / StandardDeviation;
        return Math.Exp(-0.5 * d * d) / (Constants.Sqrt2Pi * StandardDeviation);
    }

    public double Cdf(double x)
    {
        return 0.5 * SpecialFunctions.Erfc((Mean - x) / (StandardDeviation * Constants.Sqrt2));
    }

    public double Quantile(double p)
    {
        if (p is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(p), p, "The quantile must be between 0 and 1.");
        }
        
        return Mean - StandardDeviation * Constants.Sqrt2 * SpecialFunctions.ErfcInv(2.0 * p);
    }

    public override string ToString() => $"{typeof(Normal)} Nobs={Nobs} | μ={Mean}, σ²={Variance}";
}