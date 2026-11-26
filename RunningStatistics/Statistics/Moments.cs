using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// The <c>Moments</c> class calculates and maintains running statistics for the first four central
/// moments: mean, variance, skewness, and kurtosis. The variance is the unbiased sample variance.
/// Internally it keeps the central sums of deviations about the running mean, with Chan's
/// pairwise update. That update stays accurate on badly offset streams where raw power sums
/// would lose all significant digits.
/// </summary>
public sealed class Moments : RunningStatisticBase<double, Moments>
{
    private double _mean, _c2, _c3, _c4;
    private long _nobs;


    /// <summary>
    /// The first central moment is the measure of central location.
    /// </summary>
    public double Mean => Nobs < 1 ? double.NaN : _mean;

    /// <summary>
    /// The second central moment is the measure of the spread of the distribution. This returns the
    /// unbiased sample variance.
    /// </summary>
    public double Variance => Nobs < 2 ? double.NaN : _c2 / (Nobs - 1);

    /// <summary>
    /// The corrected sample standard deviation.
    /// </summary>
    public double StandardDeviation => Nobs < 2 ? double.NaN : Math.Sqrt(Variance);

    /// <summary>
    /// The third central moment is the measure of the lopsidedness of the distribution.
    /// This is the standardized sample skewness: it needs at least three observations, and
    /// returns NaN below that limit, because two points cannot say which way they lean.
    /// </summary>
    public double Skewness
    {
        get
        {
            if (Nobs < 3) return double.NaN;

            var n = (double)Nobs;
            var m2 = _c2 / n;
            if (m2 <= 0) return double.NaN;
            return _c3 / n / (m2 * Math.Sqrt(m2));
        }
    }

    /// <summary>
    /// The fourth central moment is a measure of the heaviness of the tail of the distribution.
    /// This is the non-excess (historical) kurtosis. It needs at least four observations, and
    /// returns NaN below that limit: with three or fewer points there is no tail to weigh.
    /// </summary>
    public double Kurtosis
    {
        get
        {
            if (Nobs < 4) return double.NaN;

            var n = (double)Nobs;
            var m2 = _c2 / n;
            if (m2 <= 0) return double.NaN;
            return _c4 / n / (m2 * m2);
        }
    }

    /// <summary>
    /// The amount of kurtosis relative to a Gaussian distribution.
    /// </summary>
    public double ExcessKurtosis => Kurtosis - 3;


    protected override long GetNobs() => _nobs;

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    private void UncheckedFit(double value, long count)
    {
        checked { _nobs += count; }

        // The first block of observations sets the mean exactly and leaves zero spread,
        // so there is no earlier state to update against.
        if (_nobs == count)
        {
            _mean = value;
            _c2 = 0;
            _c3 = 0;
            _c4 = 0;
            return;
        }

        // Merge the constant block of identical observations into the current state: the
        // block's mean is the fitted value and its central sums are zero. The shifts
        // computed below measure how far each side's mean sits from the combined mean.
        var n = (double)_nobs;
        var nA = (double)(_nobs - count);
        var nB = (double)count;
        var invN = 1.0 / n;

        var d = value - _mean;
        var s = -d * nB * invN;
        var t = d * nA * invN;

        var s2 = s * s;
        var s3 = s2 * s;
        var t2 = t * t;
        var t3 = t2 * t;

        _c4 += 4.0 * s * _c3 + 6.0 * s2 * _c2 + nA * s2 * s2 + nB * t2 * t2;
        _c3 += 3.0 * s * _c2 + nA * s3 + nB * t3;
        _c2 += nA * s2 + nB * t2;
        _mean -= s;
    }

    // The batch route mirrors the sibling classes Variance and Normal: validate the whole
    // batch before touching state, then fold it once into its own batch mean and batch
    // central sums, and join the running state with the same pairwise update Merge and
    // UncheckedFit(value, count) use. A batch fitted to a fresh statistic ends in the
    // two-pass shape, so its readouts track the two-pass reference at any condition
    // number, while the replay loop's error grows with the condition number.
    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        if (ys.Count == 0) return;
        ys.ForEach(Require.Finite);
        UncheckedFit(ys);
    }

    // The batch plays the role of the constant block in the pairwise update: its mean is
    // the batch average and its central sums are the sums about that mean. The shifts
    // measure how far each side's mean sits from the combined mean.
    internal void UncheckedFit(List<double> values)
    {
        checked { _nobs += values.Count; }

        var mean = BatchAverage(values);
        var c2 = 0.0;
        var c3 = 0.0;
        var c4 = 0.0;
        for (var i = 0; i < values.Count; i++)
        {
            var dx = values[i] - mean;
            var dx2 = dx * dx;
            c2 += dx2;
            c3 += dx2 * dx;
            c4 += dx2 * dx2;
        }

        // The batch is the whole count, so the statistic was fresh: keep the batch state
        // directly, the way the constant-block route does for the first observations.
        var nA = (double)(_nobs - values.Count);
        if (nA == 0)
        {
            _mean = mean;
            _c2 = c2;
            _c3 = c3;
            _c4 = c4;
            return;
        }

        var nB = (double)values.Count;
        var n = (double)_nobs;
        var invN = 1.0 / n;

        var d = mean - _mean;
        var s = -d * nB * invN;
        var t = d * nA * invN;

        var s2 = s * s;
        var s3 = s2 * s;
        var t2 = t * t;
        var t3 = t2 * t;

        _c4 += c4 + 4.0 * t * c3 + 6.0 * t2 * c2 + nB * t2 * t2
             + 4.0 * s * _c3 + 6.0 * s2 * _c2 + nA * s2 * s2;
        _c3 += c3 + 3.0 * t * c2 + nB * t3 + 3.0 * s * _c2 + nA * s3;
        _c2 += c2 + nB * t2 + nA * s2;
        _mean -= s;
    }

    /// <summary>
    /// The average of the batch that the batch route folds. The fold keeps a correction
    /// beside its total, the same Knuth two-sum fold the Mean class batch route and
    /// Normal.BatchAverage use, so a batch of canceling terms keeps the ones a plain
    /// fold would drop. The central sums are taken about this mean.
    /// </summary>
    internal double BatchAverage(List<double> values)
    {
        var total = 0.0;
        var correction = 0.0;
        foreach (var x in values)
        {
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
        }

        return (total + correction) / values.Count;
    }

    public override void Reset()
    {
        _nobs = 0;
        _mean = 0;
        _c2 = 0;
        _c3 = 0;
        _c4 = 0;
    }

    public override Moments CloneEmpty() => new();

    public override void Merge(Moments other)
    {
        checked { _nobs += other.Nobs; }

        if (Nobs == 0) return;
        if (other.Nobs == 0) return;

        var nA = (double)(_nobs - other.Nobs);
        if (nA == 0)
        {
            _mean = other._mean;
            _c2 = other._c2;
            _c3 = other._c3;
            _c4 = other._c4;
            return;
        }

        var nB = (double)other.Nobs;
        var n = (double)_nobs;
        var invN = 1.0 / n;

        var d = other._mean - _mean;
        var s = -d * nB * invN;
        var t = d * nA * invN;

        var s2 = s * s;
        var s3 = s2 * s;
        var t2 = t * t;
        var t3 = t2 * t;

        _c4 += other._c4 + 4.0 * t * other._c3 + 6.0 * t2 * other._c2 + nB * t2 * t2
             + 4.0 * s * _c3 + 6.0 * s2 * _c2 + nA * s2 * s2;
        _c3 += other._c3 + 3.0 * t * other._c2 + nB * t3 + 3.0 * s * _c2 + nA * s3;
        _c2 += other._c2 + nB * t2 + nA * s2;
        _mean -= s;
    }

    public override string ToString() => base.ToString() + $" | M1={Mean:F2}, M2={Variance:F2}, M3={Skewness:F2}, M4={Kurtosis:F2}";
}
