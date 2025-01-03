using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the average of a stream of numbers.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
{
    private long _nobs;
    private double _value;


    public double Value => Nobs == 0 ? double.NaN : _value;


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

    public static explicit operator double(Mean mean) => mean.Value;

    protected override string GetStatsString() => $"μ={Value}";
}