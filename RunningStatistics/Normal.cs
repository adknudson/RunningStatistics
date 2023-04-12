using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class Normal : AbstractRunningStatistic<double, Normal>
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
    
    public double StandardDeviation => Math.Sqrt(_variance.Value);
    
    
    
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

    public override Normal CloneEmpty()
    {
        return new Normal();
    }

    public override Normal Clone()
    {
        return new Normal(this);
    }
    public override void Merge(Normal normal)
    {
        _mean.Merge(normal._mean);
        _variance.Merge(normal._variance);
    }

    public override string ToString()
    {
        return $"{typeof(Normal)} Nobs={Nobs} | μ={Mean}, σ²={Variance}";
    }
}