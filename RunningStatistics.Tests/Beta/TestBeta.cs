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
    
    [Fact]
    public void Fit_GuardsAgainstNegativeCount()
    {
        RunningStatistics.Beta beta = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(true, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(false, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(-1, 1));
    }
    
    [Fact]
    public void Getters_AccurateAt3E9()
    {
        // Heavy-tail scenario: 3e9 successes, 1 failure. Mean and Variance are
        // double-accurate (the getter bug is the long product in Variance, and
        // a*b fits at this scale), pinned here so the fix must not change them.
        RunningStatistics.Beta beta = new();
        beta.Fit(true, 3_000_000_000L);
        beta.Fit(false, 1L);
        var a = 3_000_000_000.0;
        var b = 1.0;
        Utils.AssertIsApproximate(a / (a + b), beta.Mean, 0);
        Utils.AssertIsApproximate(a * b / (Math.Pow(a + b, 2) * (a + b + 1)), beta.Variance, 0);
        Assert.Equal(3_000_000_000L, beta.Successes);
        Assert.Equal(1L, beta.Failures);
        
        // Overflow scenario: the Variance getter's `_a * _b` is a long product, and
        // 3_100_000_000 * 3_100_000_000 = 9.61e18 > long.MaxValue wraps to
        // -8_836_744_073_709_551_616 -> Variance comes out negative (about -3.708e-11),
        // while the double-accurate value is about 4.032e-11. The sums (6.2e9) never
        // trip the checked Fit accumulation, so this is reachable through the public API.
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
        // Pin the class's documented convention at small counts against a double
        // reference: Mean = a/(a+b); Variance = a*b / ((a+b)^2 * (a+b+1)).
        RunningStatistics.Beta beta = new(7, 3);
        Assert.Equal(0.7, beta.Mean);
        Utils.AssertIsApproximate(21.0 / (Math.Pow(10.0, 2) * 11.0), beta.Variance, 0);
        Utils.AssertIsApproximate(7.0 * 3.0 / (Math.Pow(7.0 + 3.0, 2) * (7.0 + 3.0 + 1)), beta.Variance, 0);
    }

    // Commit 21 pins: the batch path Fit(IEnumerable<bool>) (values.ToList(), count the
    // trues, then Fit(s, n - s)) against hand-counted truths of the SAME iterable, batch-
    // after-batch and batch-vs-loop agreement, the empty-iterable outcome, and all-true/
    // all-false runs. Beta's counters are exact integers, so both paths produce identical
    // state and the double getters evaluate BITWISE IDENTICALLY — pinned exactly here,
    // stronger than the plan's 1e-8 relative tolerance (which the exact pins satisfy).

    [Fact]
    public void BatchFit_CountsMatchHandCount()
    {
        var values = SeededBooleans();
        var successes = CountTrue(values);
        var failures = values.Count - successes;

        // Seeded stream sanity: the 1000-bool Random(20251114) stream is mixed, so the
        // getters' Variance (needs successes > 0 and failures > 0) exists.
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

        // Counters are exact longs: the batch Fit(s, n - s) and the per-element loop land on
        // the same (_a, _b), so Nobs/Successes/Failures are exact-equal and the getters
        // evaluate from identical integers (bitwise-identical doubles).
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
        // Empirical (source read first): Fit(IEnumerable<bool>) does values.ToList(),
        // then Fit(s = 0, n - s = 0) — the NonNegative guards accept 0 and the checked
        // block adds zero, so empty is a CLEAN no-op, not a Linq Average crash (unlike
        // Normal's path). Pinned green; no library guard was needed.
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

        // Constructor-equivalence: same state as new Beta(250, 0).
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