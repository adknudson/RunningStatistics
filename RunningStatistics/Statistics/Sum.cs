using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Represents a running sum of observations.
///
/// The sum keeps a correction next to its running total. Each observation goes in through a
/// branch-free two-sum, which returns the low-order bits that the rounded addition dropped;
/// the readout adds those bits back. The error therefore stays near one epsilon of the
/// running totals, instead of growing with the number of observations. A plain fold loses
/// whole units once a term's spacing exceeds the size of the answer, which is where this
/// class keeps them: on canceling blocks whose terms pass 2^53, a plain fold reads zero
/// where the exact answer is the count of the small terms.
///
/// The step that collects those bits is skipped once the running total saturates to an
/// infinity, because its subtraction means nothing there. A saturated total then reads back
/// as that infinity, which is what the plain accumulation did before.
/// </summary>
public sealed class Sum : RunningStatisticBase<double, Sum>
{
    private long _nobs;
    private double _total, _correction;


    /// <summary>
    /// The current sum of the observations.
    /// </summary>
    public double Value => _total + _correction;


    protected override long GetNobs() => _nobs;

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
        var xs = values.ToList();
        if (xs.Count == 0) return;
        xs.ForEach(Require.Finite);
        checked { _nobs += xs.Count; }
        foreach (var x in xs) Accumulate(x);
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

    public double Mean()
    {
        return Nobs > 0 ? Value / Nobs : double.NaN;
    }

    public override string ToString() => base.ToString() + $" | Σ={Value}";
}
