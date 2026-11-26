using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// Represents the average value of a series of numbers.
///
/// The single-value path blends the new observation into the running mean, which keeps the
/// error near one epsilon of the mean. The batch path averages the batch first, and that
/// average used to be a plain fold. A plain fold drops whole units once a term's spacing
/// exceeds the running total: on canceling blocks whose terms pass 2^53, a plain fold of the
/// batch reads zero where the exact batch average is a whole unit. This class folds the
/// batch through the same branch-free two-sum the Sum class uses, so the batch average keeps
/// the low-order bits a rounded addition drops. The correction lives only inside one batch
/// fold, so the state stays two fields.
///
/// The step that collects those bits is skipped once the batch total saturates to an
/// infinity, because its subtraction means nothing there. A saturated batch then averages to
/// that infinity, which is what the plain fold did before.
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
        Require.NotNull(values);
        // One pass over the iterable, no List and no copy: the fold carries the correction
        // beside its total one observation at a time, so a batch of canceling terms keeps the
        // ones a plain fold would drop once the running total's spacing exceeds them. Each
        // value is checked before it is folded and the state is committed only after the whole
        // batch passed validation, so a value rejected mid-batch leaves the statistic exactly
        // as it was, and the count while folding supplies the weight the blend needs.
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
        // The blend the batch path ran before: the batch weight over the merged count, and the
        // batch average read once, so the state update matches to the last bit.
        var g = (double)count / Nobs;
        var prior = _value;
        _value = prior + g * ((total + correction) / count - prior);
    }

    internal void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }
        // The public Fit checked value and count, and count is positive past the zero
        // return, so the weight count / Nobs is a ratio of non-negative counts with
        // Nobs >= count: a valid probability, blendable unchecked at the identical bit.
        _value = Utils.SmoothUnchecked(_value, value, (double)count / Nobs);
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
        // Nobs is at least other.Nobs and positive past the ternary, so the merged
        // weight other.Nobs / Nobs is a ratio of non-negative counts in (0, 1]: the
        // unchecked blend costs the same bits.
        _value = Nobs == 0 ? 0 : Utils.SmoothUnchecked(_value, other._value, (double)other.Nobs / Nobs);
    }

    public override string ToString() => base.ToString() + $" | μ={Value}";
}