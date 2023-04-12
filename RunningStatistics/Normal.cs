using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class Normal : IRunningStatistic<double, Normal>
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

    

    public long Nobs => _mean.Nobs;

    public double Mean => _mean.Value;
    
    /// <summary>
    /// The bias-corrected variance.
    /// </summary>
    public double Variance => _variance.Value;
    
    public double StandardDeviation => Math.Sqrt(_variance.Value);
    
    
    
    public void Fit(IEnumerable<double> values)
    {
        var xs = values.ToList();
        _mean.Fit(xs);
        _variance.Fit(xs);
    }

    public void Fit(double value)
    {
        _mean.Fit(value);
        _variance.Fit(value);
    }

    public void Reset()
    {
        _mean.Reset();
        _variance.Reset();
    }

    public void Merge(Normal normal)
    {
        _mean.Merge(normal._mean);
        _variance.Merge(normal._variance);
    }

    public static Normal Merge(Normal first, Normal second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public Normal CloneEmpty()
    {
        return new Normal();
    }

    public Normal Clone()
    {
        return new Normal(this);
    }

    public override string ToString()
    {
        return $"{typeof(Normal)} Nobs={Nobs} | μ={Mean}, σ²={Variance}";
    }
}