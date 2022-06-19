using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

public class Normal : IRunningStatistic<double>
{
    private readonly Mean _mean = new();
    private readonly Variance _variance = new();


    public long Count => _mean.Count;
    public double Mean => _mean.Value;
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

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Normal normal) return;

        _mean.Merge(normal._mean);
        _variance.Merge(normal._variance);
    }

    public void Print(StreamWriter stream)
    {
        stream.WriteLine($"{GetType()}(μ={Mean}, σ²={Variance}, n={Count}");
    }
}