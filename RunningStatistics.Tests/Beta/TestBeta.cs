using System;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using Xunit;

namespace RunningStatistics.Tests.Beta;

public partial class TestBeta()
    : AbstractRunningStatsTest<bool, RunningStatistics.Beta>(
        () => Random.Shared.NextBoolean(), 
        () => new RunningStatistics.Beta())
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var beta = new RunningStatistics.Beta();
        beta.Fit(true, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => beta.Fit(true, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, beta.Nobs);
    }

    [Fact]
    public void ParameterlessConstructor_InitializesParametersCorrectly()
    {
        RunningStatistics.Beta beta = new();
        Assert.Equal(0, beta.Successes);
        Assert.Equal(0, beta.Failures);
        Assert.Equal(0, beta.Nobs);
    }
    
    [Fact]
    public void Constructor_InitializesParametersCorrectly()
    {
        RunningStatistics.Beta beta = new(5, 10);
        Assert.Equal(5, beta.Successes);
        Assert.Equal(10, beta.Failures);
        Assert.Equal(15, beta.Nobs);
    }
    
    [Fact]
    public void Constructor_GuardsAgainstNegativeParameters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.Beta(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.Beta(10, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.Beta(-1, -1));
    }
    
    // The (bool, count) negative-count guard lives in AbstractRunningStatsTest; this fact
    // keeps the Beta-specific (successes, failures) overload guards.
    [Fact]
    public void FitSf_GuardsAgainstNegativeSuccessesAndFailures()
    {
        RunningStatistics.Beta beta = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(-1, 1));
    }

    // CloneEmpty on a Beta built with the constructor that takes starting successes and failures
    // must give a true zero state: those counts are state, not configuration, so a clone that
    // leaked them would double-count through the usual pattern of accumulating into a clone with
    // Merge.
    [Fact]
    public void CloneEmpty_IsTrueZeroState()
    {
        var beta = new RunningStatistics.Beta(7, 3);
        beta.Fit(false, 4);
        Assert.Equal(7L, beta.Successes);
        Assert.Equal(7L, beta.Failures);
        Assert.Equal(14L, beta.Nobs);

        var clone = beta.CloneEmpty();
        Assert.Equal(0L, clone.Nobs);
        Assert.Equal(0L, clone.Successes);
        Assert.Equal(0L, clone.Failures);
        Assert.Equal(double.NaN, clone.Mean);
        Assert.True(double.IsNaN(clone.Variance));
        Assert.True(double.IsNaN(clone.Median));
        Assert.True(double.IsNaN(clone.Mode));
        Assert.Matches("Nobs=0\\) \\| α=0, β=0", clone.ToString());

        // round-trip: merging the source into the empty clone rebuilds it exactly — the
        // counters are exact longs, so every getter evaluates bitwise identically
        clone.Merge(beta);
        Assert.Equal(7L, clone.Successes);
        Assert.Equal(7L, clone.Failures);
        Assert.Equal(14L, clone.Nobs);
        Assert.Equal(beta.Mean, clone.Mean);
        Assert.Equal(beta.Variance, clone.Variance);
        Assert.Equal(beta.Median, clone.Median);
        Assert.Equal(beta.Mode, clone.Mode);
    }

    [Fact]
    public void Getters_AccurateAt3E9()
    {
        // A lopsided scenario: three billion successes and one failure. The mean and
        // variance still match the values computed in double arithmetic, and this pins
        // them so a later change cannot move them. (The old getter bug, multiplying the
        // counts before converting them, would not have shown here: the product fits.)
        RunningStatistics.Beta beta = new();
        beta.Fit(true, 3_000_000_000L);
        beta.Fit(false, 1L);
        var a = 3_000_000_000.0;
        var b = 1.0;
        Utils.AssertIsApproximate(a / (a + b), beta.Mean, 0);
        Utils.AssertIsApproximate(a * b / (Math.Pow(a + b, 2) * (a + b + 1)), beta.Variance, 0);
        Assert.Equal(3_000_000_000L, beta.Successes);
        Assert.Equal(1L, beta.Failures);
        
        // The regression this fact guards against: if the Variance getter multiplied the
        // two counts as whole numbers before converting them, 3.1 billion times 3.1
        // billion is 9.61e18, larger than the biggest long, so the product would wrap to
        // about -8.8e18 and the variance would come out negative (about -3.708e-11)
        // instead of the correct positive value (about 4.032e-11). The getter converts
        // first, so the positive value is required here. These counts total 6.2 billion,
        // which is far below the overflow threshold in Fit, so this state is reachable
        // through the public API alone.
        RunningStatistics.Beta overflow = new();
        overflow.Fit(true, 3_100_000_000L);
        overflow.Fit(false, 3_100_000_000L);
        var c = 3_100_000_000.0;
        Utils.AssertIsApproximate(c / (c + c), overflow.Mean, 0);
        Utils.AssertIsApproximate(c * c / (Math.Pow(c + c, 2) * (c + c + 1)), overflow.Variance, 0);
        Assert.Equal(3_100_000_000L, overflow.Successes);
        Assert.Equal(3_100_000_000L, overflow.Failures);
    }
    
    [Fact]
    public void Getters_ExactAtSmallCounts()
    {
        // At small counts, the getters must match the documented formulas exactly: the
        // mean is successes divided by the total count, and the variance is the
        // documented Beta formula on the two counts.
        RunningStatistics.Beta beta = new(7, 3);
        Assert.Equal(0.7, beta.Mean);
        Utils.AssertIsApproximate(21.0 / (Math.Pow(10.0, 2) * 11.0), beta.Variance, 0);
        Utils.AssertIsApproximate(7.0 * 3.0 / (Math.Pow(7.0 + 3.0, 2) * (7.0 + 3.0 + 1)), beta.Variance, 0);
    }

    // The batch path for bool collections is pinned against hand-counted truths from the
    // same collection: fitting a whole collection must agree with counting its true
    // values and fitting those two counts, several batches in a row must agree with
    // fitting them one at a time, a batch must agree with the element-by-element loop,
    // and the empty collection, all-true and all-false runs are pinned too. Beta's
    // counters are exact whole numbers, so both paths land on identical state and the
    // double getters compute bit-for-bit identical results. These facts therefore assert
    // exact equality, which is stronger than (and satisfies) the 1e-8 relative
    // tolerance that would also be acceptable.

    [Fact]
    public void BatchFit_CountsMatchHandCount()
    {
        var values = SeededBooleans();
        var successes = CountTrue(values);
        var failures = values.Count - successes;

        // Sanity check on the seeded stream: the one thousand random booleans from seed 20251114
        // are a mix of outcomes, so both successes and failures are present and the variance
        // getter, which needs at least one of each, has a value.
        Assert.True(successes > 0);
        Assert.True(failures > 0);

        var beta = new RunningStatistics.Beta();
        beta.Fit(values);

        Assert.Equal(values.Count, beta.Nobs);
        Assert.Equal(successes, beta.Successes);
        Assert.Equal(failures, beta.Failures);

        var a = (double)successes;
        var b = (double)failures;
        Utils.AssertIsApproximate(a / (a + b), beta.Mean, 0);
        Utils.AssertIsApproximate(a * b / (Math.Pow(a + b, 2) * (a + b + 1)), beta.Variance, 0);
    }

    [Fact]
    public void BatchFit_BatchEqualsLoop()
    {
        var values = SeededBooleans();

        var batch = new RunningStatistics.Beta();
        batch.Fit(values);

        var loop = new RunningStatistics.Beta();
        foreach (var v in values)
        {
            loop.Fit(v);
        }

        // The counters are exact whole numbers, so fitting the collection as one batch and
        // fitting the same values one at a time end on exactly the same success and
        // failure counts. The two statistics therefore report exactly equal counts and
        // bit-for-bit identical mean and variance.
        Assert.Equal(batch.Nobs, loop.Nobs);
        Assert.Equal(batch.Successes, loop.Successes);
        Assert.Equal(batch.Failures, loop.Failures);
        Assert.Equal(batch.Mean, loop.Mean);
        Assert.Equal(batch.Variance, loop.Variance);
    }

    [Fact]
    public void BatchFit_BatchAfterBatchEqualsLoop()
    {
        var values = SeededBooleans();
        var successes = CountTrue(values);

        var stat = new RunningStatistics.Beta();
        stat.Fit(values);
        stat.Fit(values);

        var loop = new RunningStatistics.Beta();
        foreach (var v in values)
        {
            loop.Fit(v);
        }
        foreach (var v in values)
        {
            loop.Fit(v);
        }

        Assert.Equal(2 * values.Count, stat.Nobs);
        Assert.Equal(2 * successes, stat.Successes);
        Assert.Equal(stat.Nobs, loop.Nobs);
        Assert.Equal(stat.Successes, loop.Successes);
        Assert.Equal(stat.Failures, loop.Failures);
        Assert.Equal(stat.Mean, loop.Mean);
        Assert.Equal(stat.Variance, loop.Variance);
    }

    [Fact]
    public void BatchFit_EmptyIterableIsNoOp()
    {
        // Fitting an empty collection turns into "zero successes and zero failures", and
        // the guards accept zero and the checked addition simply adds nothing, so the
        // call is a clean no-op. (Unlike Normal's collection path, which would crash
        // computing an average of nothing.) The library needed no special guard for
        // this; the fact just pins the behavior.
        var fresh = new RunningStatistics.Beta();
        fresh.Fit(new List<bool>());
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(0, fresh.Successes);
        Assert.Equal(0, fresh.Failures);
        Assert.Equal(double.NaN, fresh.Mean);
        Assert.Equal(double.NaN, fresh.Variance);

        var filled = new RunningStatistics.Beta();
        filled.Fit(true, 4);
        filled.Fit(false, 1);
        filled.Fit(new List<bool>());
        Assert.Equal(4L, filled.Successes);
        Assert.Equal(1L, filled.Failures);
        Assert.Equal(5L, filled.Nobs);
        Utils.AssertIsApproximate(4.0 / 5.0, filled.Mean, 0);
        Utils.AssertIsApproximate(4.0 * 1.0 / (Math.Pow(5.0, 2) * 6.0), filled.Variance, 0);
    }

    [Fact]
    public void BatchFit_AllTrueIterable()
    {
        var beta = new RunningStatistics.Beta();
        beta.Fit(AllTrue(250));

        Assert.Equal(250L, beta.Nobs);
        Assert.Equal(250L, beta.Successes);
        Assert.Equal(0L, beta.Failures);
        Assert.Equal(1.0, beta.Mean);
        Assert.Equal(double.NaN, beta.Variance);
        Assert.Equal(double.NaN, beta.Median);

        // leaves the same state as a Beta built with 250 successes and no failures
        var ctor = new RunningStatistics.Beta(250, 0);
        Assert.Equal(ctor.Successes, beta.Successes);
        Assert.Equal(ctor.Failures, beta.Failures);
        Assert.Equal(ctor.Mean, beta.Mean);
    }

    [Fact]
    public void BatchFit_AllFalseIterable()
    {
        var beta = new RunningStatistics.Beta();
        beta.Fit(AllFalse(250));

        Assert.Equal(250L, beta.Nobs);
        Assert.Equal(0L, beta.Successes);
        Assert.Equal(250L, beta.Failures);
        Assert.Equal(0.0, beta.Mean);
        Assert.Equal(double.NaN, beta.Variance);
        Assert.Equal(double.NaN, beta.Median);

        var ctor = new RunningStatistics.Beta(0, 250);
        Assert.Equal(ctor.Successes, beta.Successes);
        Assert.Equal(ctor.Failures, beta.Failures);
        Assert.Equal(ctor.Mean, beta.Mean);
    }

    [Fact]
    public void BatchFit_MatchesWeightedFitAndConstructor()
    {
        var values = SeededBooleans();
        var successes = CountTrue(values);
        var failures = values.Count - successes;

        var batch = new RunningStatistics.Beta();
        batch.Fit(values);

        var weighted = new RunningStatistics.Beta();
        weighted.Fit(successes, failures);

        var ctor = new RunningStatistics.Beta(successes, failures);
        Assert.Equal(batch.Nobs, weighted.Nobs);
        Assert.Equal(batch.Successes, weighted.Successes);
        Assert.Equal(batch.Failures, weighted.Failures);
        Assert.Equal(ctor.Successes, batch.Successes);
        Assert.Equal(ctor.Failures, batch.Failures);
        Assert.Equal(ctor.Mean, batch.Mean);
        Assert.Equal(ctor.Variance, batch.Variance);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.

    private static List<bool> SeededBooleans()
    {
        var values = new List<bool>();
        var rng = new Random(20251114);
        for (var i = 0; i < 1000; i++)
        {
            values.Add(rng.NextBoolean());
        }
        return values;
    }

    private static int CountTrue(List<bool> values)
    {
        var count = 0;
        foreach (var v in values)
        {
            if (v)
            {
                count++;
            }
        }
        return count;
    }

    private static List<bool> AllTrue(int length)
    {
        var values = new List<bool>();
        for (var i = 0; i < length; i++)
        {
            values.Add(true);
        }
        return values;
    }

    private static List<bool> AllFalse(int length)
    {
        var values = new List<bool>();
        for (var i = 0; i < length; i++)
        {
            values.Add(false);
        }
        return values;
    }
}