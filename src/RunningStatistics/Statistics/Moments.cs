using System;
using RunningStatistics.UnsafeStatistics;

namespace RunningStatistics;

/// <summary>
/// Tracks the first four non-central moments, stored as a <see cref="double"/>.
/// </summary>
public sealed class Moments : RunningStatisticBase<double, Moments>
{
    private readonly UnsafeMoments _moment = new();
    
    
    public Moments() { }
    
    private Moments(Moments other)
    {
        _moment = other._moment.Clone();
    }

    
    /// <summary>
    /// The first central moment is the measure of central location.
    /// </summary>
    public double Mean => Nobs == 0 ? double.NaN : _moment.Mean;

    /// <summary>
    /// The second central moment is the measure of the spread of the distribution. This returns the
    /// bias-corrected variance.
    /// </summary>
    public double Variance => Nobs == 0 ? double.NaN : _moment.Variance;

    /// <summary>
    /// The sample standard deviation.
    /// </summary>
    public double StandardDeviation => Math.Sqrt(Variance);
    
    /// <summary>
    /// The third central moment is the measure of the lopsidedness of the distribution
    /// </summary>
    public double Skewness => Nobs == 0 ? double.NaN : _moment.Skewness;

    /// <summary>
    /// The fourth central moment is a measure of the heaviness of the tail of the distribution. This is the non-excess (historical) kurtosis.
    /// </summary>
    public double Kurtosis => Nobs == 0 ? double.NaN : _moment.Kurtosis;

    /// <summary>
    /// The amount of kurtosis relative to a Gaussian distribution.
    /// </summary>
    public double ExcessKurtosis => Kurtosis - 3;


    protected override long GetNobs() => _moment.Nobs;

    public override void Fit(double value)
    {
        Require.Finite(value);
        _moment.Fit(value);
    }

    public override void Reset() => _moment.Reset();

    public override Moments CloneEmpty() => new();

    public override Moments Clone() => new(this);

    public override void Merge(Moments other)
    {
        _moment.Merge(other._moment);
    }
    
    protected override string GetStatsString()
    {
        return $"M1={Mean:F2}, M2={Variance:F2}, M3={Skewness:F2}, M4={Kurtosis:F2}";
    }
}