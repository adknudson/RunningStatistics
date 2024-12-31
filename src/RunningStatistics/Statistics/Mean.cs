using System.Collections.Generic;
using System.Linq;
using RunningStatistics.Unchecked;

// ReSharper disable UnusedMember.Global

namespace RunningStatistics;

/// <summary>
/// Tracks the average of a stream of numbers.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
{
    private readonly UncheckedMean _mean = new();


    public double Value => Nobs == 0 ? double.NaN : _mean.Value;


    protected override long GetNobs() => _mean.Nobs;
    
    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        _mean.Fit(value, count);
    }

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        ys.ForEach(Require.Finite);
        _mean.Fit(ys);
    }

    public override void Reset() => _mean.Reset();

    public override Mean CloneEmpty() => new();
    
    public override void Merge(Mean other) => _mean.Merge(other._mean);

    public static explicit operator double(Mean mean) => mean.Value;

    protected override string GetStatsString() => $"μ={Value}";
}