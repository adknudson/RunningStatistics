using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a running sum of observations.
/// </summary>
public sealed class Sum : RunningStatisticBase<double, Sum>
{
    private long _nobs;
    
    
    /// <summary>
    /// The current sum of the observations.
    /// </summary>
    public double Value { get; private set; }


    protected override long GetNobs() => _nobs;

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        _nobs += count;
        Value += value * count;
    }

    public override void Fit(IEnumerable<double> values)
    {
        var xs = values.ToList();
        xs.ForEach(Require.Finite);
        _nobs += xs.Count;
        Value += xs.Sum();
    }

    public override void Reset()
    {
        _nobs = 0;
        Value = 0;
    }

    public override Sum CloneEmpty() => new();
    
    public override void Merge(Sum sum)
    {
        _nobs += sum.Nobs;
        Value += sum.Value;
    }

    public override string ToString() => base.ToString() + $" | Σ={Value}";
}