using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// Tracks the first four non-central moments, stored as a <see cref="double"/>.
/// </summary>
public class Moments : IRunningStatistic<double, (double Mean, double Variance, double Skewness, double ExcessKurtosis), Moments>
{
    private double _mean, _variance, _skewness, _kurtosis;

    

    public Moments()
    {
        Nobs = 0;
        _mean = 0;
        _variance = 0;
        _skewness = 0;
        _kurtosis = 0;
    }

    

    public long Nobs { get; private set; }
    
    public double Mean => Nobs == 0 ? double.NaN : _mean;

    /// <summary>
    /// The bias-corrected variance.
    /// </summary>
    public double Variance => Nobs == 0 ? double.NaN : Utils.BesselCorrection(Nobs) * (_variance - _mean * _mean);

    /// <summary>
    /// The sample standard deviation.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Variance);

    public double Skewness
    {
        get
        {
            if (Nobs == 0)
            {
                return double.NaN;
            }

            var mean2 = _mean * _mean;
            var vr = _variance - mean2;
            return (_skewness - 3.0 * _mean * vr - mean2 * _mean) / Math.Pow(vr, 1.5);
        }
    }

    public double Kurtosis
    {
        get
        {
            if (Nobs == 0)
            {
                return double.NaN;
            }

            var mean2 = _mean * _mean;
            var mean4 = mean2 * mean2;
            var variance = _variance - mean2;

            return (_kurtosis - 4.0 * _mean * _skewness + 6.0 * mean2 * _variance - 3.0 * mean4) / (variance * variance);
        }
    }
    
    public double ExcessKurtosis => Kurtosis - 3;

    public (double Mean, double Variance, double Skewness, double ExcessKurtosis) Value =>
        (Mean, Variance, Skewness, ExcessKurtosis);

    
    
    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value)
    {
        Nobs++;

        var g = 1.0 / Nobs;
        var y2 = value * value;

        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, y2, g);
        _skewness = Utils.Smooth(_skewness, value * y2, g);
        _kurtosis = Utils.Smooth(_kurtosis, y2 * y2, g);
    }

    public void Reset()
    {
        Nobs = 0;
        _mean = 0;
        _variance = 0;
        _skewness = 0;
        _kurtosis = 0;
    }

    public void Merge(Moments moments)
    {
        Nobs += moments.Nobs;

        if (Nobs <= 0) return;

        var g = (double) moments.Nobs / Nobs;
        _mean = Utils.Smooth(_mean, moments._mean, g);
        _variance = Utils.Smooth(_variance, moments._variance, g);
        _skewness = Utils.Smooth(_skewness, moments._skewness, g);
        _kurtosis = Utils.Smooth(_kurtosis, moments._kurtosis, g);
    }

    public static Moments Merge(Moments first, Moments second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public Moments CloneEmpty()
    {
        return new Moments();
    }

    public Moments Clone()
    {
        return new Moments()
        {
            Nobs = Nobs,
            _mean = _mean,
            _variance = _variance,
            _skewness = _skewness,
            _kurtosis = _kurtosis,
        };
    }

    public override string ToString()
    {
        return $"{typeof(Moments)}(M=[{Mean:F2}, {Variance:F2}, {Skewness:F2}, {ExcessKurtosis:F2}], n={Nobs})";
    }
}