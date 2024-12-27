using System;

namespace RunningStatistics;

/// <summary>
/// Tracks the first four non-central moments, stored as a <see cref="double"/>.
/// </summary>
public sealed class Moments : RunningStatisticBase<double, Moments>
{
    private double _mean, _variance, _skewness, _kurtosis;
    private long _nobs;


    /// <summary>
    /// The first central moment is the measure of central location.
    /// </summary>
    public double Mean => Nobs == 0 ? double.NaN : _mean;

    /// <summary>
    /// The second central moment is the measure of the spread of the distribution. This returns the bias-corrected variance.
    /// </summary>
    public double Variance => Nobs == 0 ? double.NaN : Utils.BesselCorrection(Nobs) * (_variance - _mean * _mean);
    
    /// <summary>
    /// The third central moment is the measure of the lopsidedness of the distribution
    /// </summary>
    public double Skewness
    {
        get
        {
            if (Nobs == 0) return double.NaN;

            var mean2 = _mean * _mean;
            var vr = _variance - mean2;
            return (_skewness - 3.0 * _mean * vr - mean2 * _mean) / Math.Pow(vr, 1.5);
        }
    }

    /// <summary>
    /// The fourth central moment is a measure of the heaviness of the tail of the distribution. This is the non-excess (historical) kurtosis.
    /// </summary>
    public double Kurtosis
    {
        get
        {
            if (Nobs == 0) return double.NaN;

            var mean2 = _mean * _mean;
            var mean4 = mean2 * mean2;
            var variance = _variance - mean2;

            return (_kurtosis - 4.0 * _mean * _skewness + 6.0 * mean2 * _variance - 3.0 * mean4) / (variance * variance);
        }
    }
    
    /// <summary>
    /// The amount of kurtosis relative to a Gaussian distribution.
    /// </summary>
    public double ExcessKurtosis => Kurtosis - 3;

    public bool IsLeptokurtic => ExcessKurtosis > 0;

    public bool IsPlatykurtic => ExcessKurtosis < 0;


    protected override long GetNobs() => _nobs;

    public override void Fit(double value)
    {
        _nobs++;

        var g = 1.0 / Nobs;
        var y2 = value * value;

        _mean = Utils.Smooth(_mean, value, g);
        _variance = Utils.Smooth(_variance, y2, g);
        _skewness = Utils.Smooth(_skewness, value * y2, g);
        _kurtosis = Utils.Smooth(_kurtosis, y2 * y2, g);
    }

    public override void Reset()
    {
        _nobs = 0;
        _mean = 0;
        _variance = 0;
        _skewness = 0;
        _kurtosis = 0;
    }

    public override Moments CloneEmpty() => new();

    public override Moments Clone()
    {
        return new Moments
        {
            _nobs = Nobs,
            _mean = _mean,
            _variance = _variance,
            _skewness = _skewness,
            _kurtosis = _kurtosis
        };
    }
    
    public override void Merge(Moments moments)
    {
        _nobs += moments.Nobs;

        if (Nobs == 0) return;

        var g = (double) moments.Nobs / Nobs;
        _mean = Utils.Smooth(_mean, moments._mean, g);
        _variance = Utils.Smooth(_variance, moments._variance, g);
        _skewness = Utils.Smooth(_skewness, moments._skewness, g);
        _kurtosis = Utils.Smooth(_kurtosis, moments._kurtosis, g);
    }
    
    public override string ToString()
    {
        return $"{typeof(Moments)} Nobs={Nobs} | M1={Mean:F2}, M2={Variance:F2}, M3={Skewness:F2}, M4={Kurtosis:F2}";
    }

    public void Deconstruct(out double mean)
    {
        mean = Mean;
    }

    public void Deconstruct(out double mean, out double variance)
    {
        mean = Mean;
        variance = Variance;
    }

    public void Deconstruct(out double mean, out double variance, out double skewness)
    {
        mean = Mean;
        variance = Variance;
        skewness = Skewness;
    }
    
    public void Deconstruct(out double mean, out double variance, out double skewness, out double kurtosis)
    {
        mean = Mean;
        variance = Variance;
        skewness = Skewness;
        kurtosis = Kurtosis;
    }
}