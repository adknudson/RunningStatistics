using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents the average value of a series of numbers.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
{
    private long _nobs;
    private double _value;


    /// <summary>
    /// Gets the current average value.
    /// </summary>
    public double Value => Nobs < 1 ? double.NaN : _value;


    protected override long GetNobs() => _nobs;

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
    
    internal void UncheckedFit(List<double> values)
    {
        _nobs += values.Count;
        _value = Utils.Smooth(_value, values.Average(), (double)values.Count / Nobs);
    }

    internal void UncheckedFit(double value, long count)
    {
        _nobs += count;
        _value = Utils.Smooth(_value, value, (double)count / Nobs);
    }

    public override void Reset()
    {
        _nobs = 0;
        _value = 0;
    }

    public override Mean CloneEmpty() => new();

    public override void Merge(Mean other)
    {
        _nobs += other.Nobs;
        _value = Nobs == 0 ? 0 : Utils.Smooth(_value, other._value, (double)other.Nobs / Nobs);
    }

    protected override string GetStatsString() => $"μ={Value}";
}