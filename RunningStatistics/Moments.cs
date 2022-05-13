using System;
using System.Collections.Generic;
using System.IO;

namespace RunningStatistics;

/// <summary>
/// Tracks the first four non-central moments, stored as a <see cref="double"/>.
/// </summary>
public class Moments : IRunningStatistic<double>
{
    private double _mean, _variance, _skewness, _kurtosis;


    public Moments()
    {
        Count = 0;
        _mean = 0;
        _variance = 0;
        _skewness = 0;
        _kurtosis = 0;
    }

    public Moments(Moments other)
    {
        Count = other.Count;
        _mean = other._mean;
        _variance = other._variance;
        _skewness = other._skewness;
        _kurtosis = other._kurtosis;
    }
    

    public nint Count { get; private set; }
    public double Mean => Count == 0 ? double.NaN : _mean;

    /// <summary>
    /// The bias-corrected variance.
    /// </summary>
    public double Variance => Count == 0 ? double.NaN : Utils.BesselCorrection(Count) * (_variance - _mean * _mean);

    /// <summary>
    /// The sample standard deviation.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Variance);

    public double Skewness
    {
        get
        {
            if (Count == 0)
            {
                return double.NaN;
            }

            var mean2 = _mean * _mean;
            var vr = _variance - mean2;
            return (_skewness - 3.0 * _mean * vr - mean2 * _mean) / Math.Pow(vr, 1.5);
        }
    }

    /// <summary>
    /// Returns the (excess) kurtosis.
    /// </summary>
    public double Kurtosis
    {
        get
        {
            if (Count == 0)
            {
                return double.NaN;
            }

            var mean2 = _mean * _mean;
            var mean4 = mean2 * mean2;
            var variance = _variance - mean2;

            return (_kurtosis - 4.0 * _mean * _skewness + 6.0 * mean2 * _variance - 3.0 * mean4)
                / (variance * variance) - 3.0;
        }
    }

    public (double Mean, double Variance, double Skewness, double Kurtosis) Values =>
        (Mean, Variance, Skewness, Kurtosis);

    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value)
    {
        Count++;

        var g = 1.0 / Count;
        var y2 = value * value;

        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, y2, g);
        _skewness = Utils.Smooth(_skewness, value * y2, g);
        _kurtosis = Utils.Smooth(_kurtosis, y2 * y2, g);
    }

    public void Reset()
    {
        Count = 0;
        _mean = 0;
        _variance = 0;
        _skewness = 0;
        _kurtosis = 0;
    }

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Moments moments) return;
        
        Count += moments.Count;

        if (Count <= 0) return;

        var g = (double) moments.Count / Count;
        _mean = Utils.Smooth(_mean, moments._mean, g);
        _variance = Utils.Smooth(_variance, moments._variance, g);
        _skewness = Utils.Smooth(_skewness, moments._skewness, g);
        _kurtosis = Utils.Smooth(_kurtosis, moments._kurtosis, g);
    }

    public static Moments Merge(Moments a, Moments b)
    {
        var c = new Moments(a);
        c.Merge(b);
        return c;
    }

    public override string ToString()
    {
        return $"{typeof(Moments)}(M=[{Mean:F2}, {Variance:F2}, {Skewness:F2}, {Kurtosis:F2}], n={Count})";
    }

    public void Print(StreamWriter stream)
    {
        stream.WriteLine($"{GetType()}(n={Count})");
        stream.WriteLine($"\tMean={Mean}");
        stream.WriteLine($"\tVariance={Variance}");
        stream.WriteLine($"\tSkewness={Skewness}");
        stream.WriteLine($"\tKurtosis={Kurtosis}");
    }
}