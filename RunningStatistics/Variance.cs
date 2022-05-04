using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

public class Variance : IRunningStatistic<double>
{
    private double _mean, _variance;


    public Variance()
    {
        Count = 0;
        _mean = 0;
        _variance = 0;
    }

    public Variance(Variance other)
    {
        Count = other.Count;
        _mean = other._mean;
        _variance = other._variance;
    }
    

    public long Count { get; private set; }

    /// <summary>
    /// Returns the bias-corrected variance.
    /// </summary>
    public double Value
    {
        get
        {
            if (Count > 1) return _variance * Utils.BesselCorrection(Count);
            return double.IsFinite(_mean) ? 1.0 : double.NaN;
        }
    }

    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Count += ys.Count;

        var mean = ys.Average();
        var variance = Utils.Variance(ys);
        var g = (double) ys.Count / Count;
        var delta = _mean - mean;

        _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
        _mean = Utils.Smooth(_mean, mean, g);
    }

    public void Fit(double value)
    {
        Count += 1;
        var mu = _mean;
        var g = 1.0 / Count;
        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, (value - _mean) * (value - mu), g);
    }

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Variance variance) return;
        
        Count += variance.Count;

        if (Count <= 0) return;

        var g = (double) variance.Count / Count;
        var delta = _mean - variance._mean;

        _variance = Utils.Smooth(_variance, variance._variance, g) + delta * delta * g * (1 - g);
        _mean = Utils.Smooth(_mean, variance._mean, g);
    }

    public static Variance Merge(Variance a, Variance b)
    {
        var c = new Variance(a);
        c.Merge(b);
        return c;
    }

    public void Reset()
    {
        Count = 0;
        _mean = 0;
        _variance = 0;
    }

    public override string ToString() => $"{typeof(Variance)}(ν={Value}, n={Count})";
    
    public static explicit operator double(Variance value) => value.Value;

    public void Print(StreamWriter stream)
    {
        stream.WriteLine($"{GetType()}(σ²={Value}, n={Count})");
    }
}