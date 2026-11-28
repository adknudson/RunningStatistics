using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The running sum of the observations fitted so far.
/// </summary>
public sealed class Sum : RunningStatisticBase<double, Sum>
{
    private long _nobs;
    private double _total, _correction;


    /// <summary>
    /// The current sum of the observations.
    /// </summary>
    public double Value => _total + _correction;
    
    /// <summary>
    /// The mean of the observations fitted so far.
    /// </summary>
    /// <remarks>
    /// With nothing fitted, the mean is undefined, and a double readout has NaN to stand in,
    /// so the property returns <see cref="double.NaN"/> instead of throwing.
    /// </remarks>
    public double Mean => Nobs > 0 ? Value / Nobs : double.NaN;


    protected override long GetNobs() => _nobs;

    public override void Fit(double value)
    {
        Require.Finite(value);
        checked { _nobs += 1; }
        Accumulate(value);
    }

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        checked { _nobs += count; }
        Accumulate(value * count);
    }

    public override void Fit(IEnumerable<double> values)
    {
        Require.NotNull(values);
        
        // The fold works on locals and commits to state only after the whole batch passed
        // validation. A value rejected mid-batch therefore leaves the statistic exactly as it
        // was, the all-or-nothing contract the materialize-and-validate pattern gave without
        // the List allocation, the copy, or the extra pass.
        var newSum = new Sum();
        
        foreach (var x in values)
        {
            newSum.Fit(x);
        }

        Merge(newSum);
    }

    private void Accumulate(double x)
    {
        var t = _total + x;
        // A saturated total would send infinities through the two-sum below and turn the
        // readout into NaN, so the collection step is skipped there and the infinity stays.
        if (Math.Abs(t) < double.PositiveInfinity)
        {
            // Knuth two-sum: these six operations return exactly the bits that the rounded
            // addition above lost. The branch-free form matters on mixed-magnitude streams,
            // where the alternative that picks a formula by magnitude mispredicts often.
            var bvirt = t - _total;
            var aver = t - bvirt;
            _correction += (_total - aver) + (x - bvirt);
        }

        _total = t;
    }

    public override void Reset()
    {
        _nobs = 0;
        _total = 0;
        _correction = 0;
    }

    public override Sum CloneEmpty() => new();

    public override void Merge(Sum sum)
    {
        if (sum.Nobs == 0) return;
        checked { _nobs += sum.Nobs; }
        Accumulate(sum._total);
        // The other statistic's correction is one rounding step of its own total, so a
        // plain addition folds it in; only the two-sum above contributes exact bits.
        _correction += sum._correction;
    }

    public override string ToString() => base.ToString() + $" | Σ={Value}";
}
