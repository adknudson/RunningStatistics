using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Variance;

public partial class TestVariance()
    : AbstractRunningStatsTest<double, RunningStatistics.Variance>(
        () => Random.Shared.NextDouble(),
        () => new RunningStatistics.Variance())
{
    [Fact]
    public void Fit_GuardsAgainstNaN()
    {
        var v = new RunningStatistics.Variance();
        Assert.Throws<ArgumentException>(() => v.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => v.Fit(double.NaN, 2));
        Assert.Throws<ArgumentException>(() => v.Fit([1.0, double.NaN, 2.0]));
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.NaN, 2),
                new(2.0, 3)
            };

            v.Fit(keyValuePairs);
        });
    }
    
    // Scalar negative-count and pairs-with-negative-count guards now live in
    // AbstractRunningStatsTest (Fit_NegativeCountThrowsStateUntouched and
    // FitPairsWithNegativeCount_ThrowsPrecedingPairsApplied).
    
    [Fact]
    public void Fit_GuardsAgainstInfiniteValues()
    {
        var v = new RunningStatistics.Variance();
        Assert.Throws<ArgumentException>(() => v.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => v.Fit(double.NegativeInfinity));
        
        Assert.Throws<ArgumentException>(() => v.Fit(double.PositiveInfinity, 2));
        Assert.Throws<ArgumentException>(() => v.Fit(double.NegativeInfinity, 2));

        Assert.Throws<ArgumentException>(() => v.Fit([1.0, double.PositiveInfinity, 2.0]));
        Assert.Throws<ArgumentException>(() => v.Fit([1.0, double.NegativeInfinity, 2.0]));
        
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.PositiveInfinity, 2),
                new(2.0, 3)
            };

            v.Fit(keyValuePairs);
        });
        
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.NegativeInfinity, 2),
                new(2.0, 3)
            };

            v.Fit(keyValuePairs);
        });
    }
    
    [Fact]
    public void Fit_ZeroCountDoesNotChangeState()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        v.Fit(3.0, 0);
        
        Assert.Equal(2, v.Nobs);
        Assert.Equal(0.5, v.Value);
    }

    [Fact]
    public void BatchFit_AccurateNear1e9()
    {
        var offset = 1e9;
        var rng = new Random(42);
        var values = new List<double>();
        for (var i = 0; i < 100; i++)
        {
            values.Add(offset + rng.NextDouble());
        }
        
        var batch = new RunningStatistics.Variance();
        batch.Fit(values);
        
        var mean = 0.0;
        foreach (var v in values)
        {
            mean += v;
        }
        mean /= values.Count;
        var sumSquaredDeviation = 0.0;
        foreach (var v in values)
        {
            sumSquaredDeviation += (v - mean) * (v - mean);
        }
        var expected = sumSquaredDeviation / (values.Count - 1);
        
        Utils.AssertIsApproximate(expected, batch.Value, 0);
        
        var incremental = new RunningStatistics.Variance();
        foreach (var v in values)
        {
            incremental.Fit(v);
        }
        Utils.AssertIsApproximate(expected, incremental.Value, 0);
    }
    
    [Fact]
    public void ConstantStream_VarianceNeverNegative()
    {
        var v1 = new RunningStatistics.Variance();
        for (var i = 0; i < 20; i++)
        {
            v1.Fit(7.0);
        }
        
        var v2 = new RunningStatistics.Variance();
        for (var i = 0; i < 5; i++)
        {
            v2.Fit(7.0);
        }
        
        var v3 = new RunningStatistics.Variance();
        v3.Fit([7.0, 7.0, 7.0]);
        v2.Merge(v3);
        
        v1.Merge(v2);
        
        Assert.Equal(28, v1.Nobs);
        Assert.Equal(0, v1.Value);
        Assert.Equal(0, v1.StandardDeviation);
    }
    
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => v.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, v.Nobs);
    }
    
    [Fact]
    public void Merge_DoesNotChangeOtherState()
    {
        var v1 = new RunningStatistics.Variance();
        v1.Fit(1.0);
        v1.Fit(2.0);
        
        var v2 = new RunningStatistics.Variance();
        v2.Fit(3.0);
        v2.Fit(4.0);
        
        // pre-conditions
        Assert.Equal(2, v2.Nobs);
        Assert.Equal(0.5, v2.Value);
        
        v1.Merge(v2);
        
        // post-conditions
        Assert.Equal(4, v1.Nobs);
        Assert.Equal(1.6666666666666667, v1.Value, 10);
        Assert.Equal(2, v2.Nobs);
        Assert.Equal(0.5, v2.Value);
    }
    
    [Fact]
    public void Merge_WithEmptyOther()
    {
        var v1 = new RunningStatistics.Variance();
        v1.Fit(1.0);
        v1.Fit(2.0);
        
        var v2 = new RunningStatistics.Variance();
        v1.Merge(v2);
        
        Assert.Equal(2, v1.Nobs);
        Assert.Equal(0.5, v1.Value);
    }
    
    [Fact]
    public void Merge_WithNonEmptyOther()
    {
        var v1 = new RunningStatistics.Variance();
        v1.Fit(1.0);
        v1.Fit(2.0);
        
        var v2 = new RunningStatistics.Variance();
        v2.Fit(3.0);
        v2.Fit(4.0);
        
        v1.Merge(v2);
        
        Assert.Equal(4, v1.Nobs);
        Assert.Equal(1.6666666666666667, v1.Value, 10);
    }
    
    [Fact]
    public void MergeEmpty_WithEmptyOther()
    {
        var v1 = new RunningStatistics.Variance();
        var v2 = new RunningStatistics.Variance();
        
        v1.Merge(v2);
        
        Assert.Equal(0, v1.Nobs);
        Assert.Equal(double.NaN, v1.Value);
    }

    // PERMANENT merge-algebra gate (keep): Variance Merge associativity. Three seeded KappaStream
    // chunks (n=100, spread 1) per κ, κ = 10^0..10^8; (a∪b)∪c vs a∪(b∪c) with fresh Clone()
    // (CloneEmpty + Merge) receivers. Value and StandardDeviation of both groupings must match
    // the two-pass reference on a∪b∪c, and each other, within relTol = 1e-8 + 1e4·κ·ε.
    // Probe worst observed: rel 2.40E-8 at κ=1e8 (~1·κ·ε; merge errors scale ~linearly in κ,
    // not κ²) → headroom ~1e4x; cross-grouping rel ≤ 1.1E-9.
    [Fact]
    public void Gate_MergeAssociativityVariance()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 11251128 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 33251129 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 55251131 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Variance();
            var b = new RunningStatistics.Variance();
            var c = new RunningStatistics.Variance();
            for (var i = 0; i < n; i++) a.Fit(chunkA[i]);
            for (var i = 0; i < n; i++) b.Fit(chunkB[i]);
            for (var i = 0; i < n; i++) c.Fit(chunkC[i]);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var bc = b.Clone();
            bc.Merge(c);
            var g2 = a.Clone();
            g2.Merge(bc);

            AssertVarianceAgainstReference("(a∪b)∪c", reference, g1, relTol);
            AssertVarianceAgainstReference("a∪(b∪c)", reference, g2, relTol);
            AssertVarianceGroupingsAgree("(a∪b)∪c vs a∪(b∪c)", g1, g2, relTol);
        }
    }

    // Same contract, commutative direction: a∪b∪c vs c∪b∪a, same κ range (10^0..10^8),
    // fresh Clone() receivers and margins relTol = 1e-8 + 1e4·κ·ε.
    [Fact]
    public void Gate_MergeCommutativityVariance()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 22251138 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 44251139 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 66251141 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Variance();
            var b = new RunningStatistics.Variance();
            var c = new RunningStatistics.Variance();
            for (var i = 0; i < n; i++) a.Fit(chunkA[i]);
            for (var i = 0; i < n; i++) b.Fit(chunkB[i]);
            for (var i = 0; i < n; i++) c.Fit(chunkC[i]);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var g3 = c.Clone();
            g3.Merge(b);
            g3.Merge(a);

            AssertVarianceAgainstReference("a∪b∪c", reference, g1, relTol);
            AssertVarianceAgainstReference("c∪b∪a", reference, g3, relTol);
            AssertVarianceGroupingsAgree("a∪b∪c vs c∪b∪a", g1, g3, relTol);
        }
    }

    private static void AssertVarianceAgainstReference(string tag, MomentsReference reference, RunningStatistics.Variance merged, double relTol)
    {
        Assert.True(PropertyHelpers.Within(reference.Variance, merged.Value, relTol), PropertyHelpers.Describe($"{tag} Value vs reference", reference.Variance, merged.Value));
        Assert.True(PropertyHelpers.Within(reference.StandardDeviation, merged.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation vs reference", reference.StandardDeviation, merged.StandardDeviation));
    }

    private static void AssertVarianceGroupingsAgree(string tag, RunningStatistics.Variance left, RunningStatistics.Variance right, double relTol)
    {
        Assert.True(PropertyHelpers.Within(left.Value, right.Value, relTol), PropertyHelpers.Describe($"{tag} Value", left.Value, right.Value));
        Assert.True(PropertyHelpers.Within(left.StandardDeviation, right.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation", left.StandardDeviation, right.StandardDeviation));
    }

    // Getter-parity gate at merge-doubling scale (AGENTS.md's readout-getter budget:
    // candidate/baseline ns-per-observation <= 1.05, i.e. the claim is "not slower" - a getter is
    // one interface dispatch plus a few flops, so ~1% measurement noise straddles 1.00 and the
    // band is NOT to be widened. A red run means the readouts got measurably slower once Nobs
    // reaches 2^62 (an O(Nobs)/formatting/allocation path crept into a getter): re-gate the
    // algorithm, do not relax the budget.)
    // Construction mirrors AbstractRunningStatsTest.MergeSelf_NobsOverflowThrows (self-merge
    // doubles Nobs in lockstep), but from TWO distinct observations doubled 61 times so the
    // state has spread: from Nobs 1 the doubled state is constant data, Value and StandardDeviation
    // land at 0 and StandardDeviation reads sqrt of a zero variance, which is degenerate rather
    // than a finite readout. From two distinct fits, Nobs reaches the same 2^62 and both readouts
    // stay finite and positive there (measured: Value ~1e-2, StandardDeviation ~1e-1), the same
    // branches (Nobs >= 2, Bessel factor n/(n-1) ~ 1) as at small Nobs. Variance exposes Value
    // (unbiased sample variance) and StandardDeviation only - its mean is private - so the sweep
    // reads those two.
    [Fact]
    public void Bench_GetterParityAtMergeDoublingNobs_Variance()
    {
        // 5M sweeps x 4 measured repeats (the harness's default shape): the sweep body is shared
        // by both sides of the comparison (see SweepVariance below), so the ratio compares
        // identical machine code and AGENTS.md's parity band [0.5, 1.05] holds at the default
        // workload size rather than being bought by a bigger workload.
        const int obs = 5_000_000;
        const int repeats = 4;

        var values = PropertyHelpers.UniformUnit(6, 20251118L);

        var big = new RunningStatistics.Variance();
        big.Fit(values[0]);
        big.Fit(values[1]);
        for (var i = 0; i < 61; i++) big.Merge(big);
        Assert.Equal(1L << 62, big.Nobs);

        var small = new RunningStatistics.Variance();
        foreach (var value in values) small.Fit(value);
        Assert.Equal(6, small.Nobs);

        // preconditions: finite readouts on both sides, and the checksum below stays well inside
        // long range when BenchmarkHarness folds it in
        Assert.False(double.IsNaN(big.Value) || double.IsNaN(big.StandardDeviation));
        Assert.False(double.IsNaN(small.Value) || double.IsNaN(small.StandardDeviation));

        var bigNsPerObs = BenchmarkHarness.Measure(
            "Variance readout sweep at Nobs=2^62, ns per sweep",
            obs,
            repeats,
            () => SweepVariance(big, obs));

        var smallNsPerObs = BenchmarkHarness.Measure(
            "Variance readout sweep at Nobs=6, ns per sweep",
            obs,
            repeats,
            () => SweepVariance(small, obs));

        var ratio = BenchmarkHarness.Compare(
            "Variance getter readout at Nobs=2^62 vs at Nobs=6, ns per sweep",
            smallNsPerObs,
            bigNsPerObs);

        Assert.True(ratio >= 0.5 && ratio <= 1.05,
                    $"2^62-scale readout sweep costs {ratio:F3} of the same sweep at Nobs=6 " +
                    $"({bigNsPerObs:F4} vs {smallNsPerObs:F4} ns per sweep): the readout getters " +
                    $"got slower once Nobs reaches 2^62. A getter is one interface dispatch plus a " +
                    $"few flops and the claim is parity (not speed), so re-gate the algorithm - " +
                    $"do not widen the band.");
    }

    // One compiled sweep body serves both sides of the comparison: the stat is a parameter, so the
    // 2^62 and the small-Nobs workloads run literally the same machine code (same JIT tier, same
    // code layout). Two separately compiled lambda bodies drifted 7-8% on code layout alone, which
    // is noise on a parity claim, not a regression.
    private static double SweepVariance(RunningStatistics.Variance stat, int obs)
    {
        var sum = 0.0;
        for (var i = 0; i < obs; i++)
        {
            sum += stat.Value;
            sum += stat.StandardDeviation;
        }

        return sum % 1_000_000.0;
    }
}