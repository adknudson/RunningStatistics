using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The running mean of the observations fitted so far.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
{
    private long _nobs;
    private double _value;


    public override long Nobs => _nobs;

    /// <summary>
    /// Gets the current average value.
    /// </summary>
    public double Value => Nobs < 1 ? double.NaN : _value;


    public override void Fit(double value)
    {
        Require.Finite(value);
        UncheckedFit(value);
    }

    public override void UncheckedFit(double value) => UncheckedFit(value, 1);

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }
    
    public override void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }
        _value = Utils.Smooth(_value, value, (double)count / Nobs);
    }

    // One pass over the iterable, no List and no copy: the fold carries a correction beside
    // its total one observation at a time, so a batch of canceling terms keeps the ones a
    // plain fold would drop once the running total's spacing exceeds them. Each value is
    // checked before it is folded and the state is committed only after the whole batch
    // passed validation, so a value rejected mid-batch leaves the statistic exactly as it
    // was, and the count while folding supplies the weight the blend needs.
    public override void Fit(IEnumerable<double> values)
    {
        var total = 0.0;
        var correction = 0.0;
        var count = 0L;
        foreach (var x in values)
        {
            Require.Finite(x);
            var t = total + x;
            // A saturated total would send infinities through the two-sum below and turn the
            // batch average into NaN, so the collection step is skipped there and the
            // infinity stays, the way the plain fold averaged it before.
            if (Math.Abs(t) < double.PositiveInfinity)
            {
                // Knuth two-sum: these six operations return exactly the bits that the
                // rounded addition above lost. The branch-free form matters on mixed-magnitude
                // batches, where picking a formula by magnitude mispredicts often.
                var bvirt = t - total;
                var aver = t - bvirt;
                correction += (total - aver) + (x - bvirt);
            }

            total = t;
            count++;
        }

        if (count == 0) return;
        checked { _nobs += count; }
        // The blend the batch path ran before: the batch weight over the merged count, and
        // the batch average read once, so the state update matches to the last bit.
        var g = (double)count / Nobs;
        var prior = _value;
        _value = prior + g * ((total + correction) / count - prior);
    }

    // The same compensated fold for the unchecked route: skipping the per-value checks is
    // the only difference, the caller vouches for the values. The correction beside the
    // total has to stay, or a batch of canceling terms reads back the wrong average even
    // when every argument was valid to begin with.
    public override void UncheckedFit(IEnumerable<double> values)
    {
        var total = 0.0;
        var correction = 0.0;
        var count = 0L;
        foreach (var x in values)
        {
            var t = total + x;
            if (Math.Abs(t) < double.PositiveInfinity)
            {
                var bvirt = t - total;
                var aver = t - bvirt;
                correction += (total - aver) + (x - bvirt);
            }

            total = t;
            count++;
        }

        if (count == 0) return;
        checked { _nobs += count; }
        var g = (double)count / Nobs;
        var prior = _value;
        _value = prior + g * ((total + correction) / count - prior);
    }

    public override void Reset()
    {
        _nobs = 0;
        _value = 0;
    }

    public override Mean CloneEmpty() => new();

    public override void Merge(Mean other)
    {
        checked { _nobs += other.Nobs; }
        _value = Nobs == 0 ? 0 : Utils.Smooth(_value, other._value, (double)other.Nobs / Nobs);
    }

    public override string ToString() => base.ToString() + $" | μ={Value}";
}