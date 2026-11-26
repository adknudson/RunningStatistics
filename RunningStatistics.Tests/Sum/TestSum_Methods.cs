using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Sum;

public partial class TestSum
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => sum.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, sum.Nobs);
    }

    [Fact]
    public void SumWith_NoNumbers()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Equal(0, sum.Value);
    }

    [Fact]
    public void SumWith_SingleNumber()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(42);
        Assert.Equal(42, sum.Value);
    }
    
    [Fact]
    public void SumOf_PositiveNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(20);
        sum.Fit(30);
        Assert.Equal(60, sum.Value);
    }

    [Fact]
    public void SumOf_NegativeNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(-10);
        sum.Fit(-20);
        sum.Fit(-30);
        Assert.Equal(-60, sum.Value);
    }

    [Fact]
    public void SumOf_MixedNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(-20);
        sum.Fit(30);
        Assert.Equal(20, sum.Value);
    }
    
    [Fact]
    public void Mean_NoNumbers_IsNaN()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Equal(double.NaN, sum.Mean());
    }

    [Fact]
    public void Mean_SingleNumber()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(42);
        Assert.Equal(42, sum.Mean());
    }

    [Fact]
    public void Mean_PositiveNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(20);
        sum.Fit(30);
        Assert.Equal(20, sum.Mean());
    }

    [Fact]
    public void Mean_NegativeNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(-10);
        sum.Fit(-20);
        sum.Fit(-30);
        Assert.Equal(-20, sum.Mean());
    }

    [Fact]
    public void Mean_MixedNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(-20);
        sum.Fit(30);
        Assert.Equal(6.67, sum.Mean(), 2);
    }

    // This fact pins the promise about non-finite values exactly as the code behaves today. The
    // fact records behavior, not a library change. Fitting checks that the value is finite first,
    // and throws an ArgumentException. Only then does it check that the count is not negative, and
    // throws an ArgumentOutOfRangeException. Both the single-value path and the batch path check
    // their input before they touch any state.

    [Fact]
    public void Fit_RejectsInfinityAndNaN()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NegativeInfinity));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN));
        // every rejected call leaves the state at its identity
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
    }

    [Fact]
    public void Fit_GuardOrder_FiniteBeforeZeroCount()
    {
        // The code checks that the value is finite before it checks the count. A zero count
        // therefore does not let a non-finite value through. The shortcut for a zero count
        // sits after both checks, so a non-finite value still reports the bad value.
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.PositiveInfinity, 0));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NegativeInfinity, 0));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN, 0));
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
        // a finite value with count 0 is accepted and is a state no-op
        var fitted = new RunningStatistics.Sum();
        fitted.Fit(2.5, 4);
        fitted.Fit(5.0, 0);
        Assert.Equal(4, fitted.Nobs);
        Assert.Equal(10.0, fitted.Value);
    }

    [Fact]
    public void Fit_GuardOrder_NaNBeatsNegativeCount()
    {
        // The code rejects a bad value before it considers the bad count. Fitting NaN with a
        // negative count therefore reports the bad-value ArgumentException, not the bad-count
        // ArgumentOutOfRangeException. Only a finite value with a negative count gets the
        // count error.
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sum.Fit(1.0, -1));
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
    }

    [Fact]
    public void BatchFit_NonFiniteElementThrowsStateUntouched()
    {
        // Fitting a collection of doubles checks every element before it adds anything, even
        // when the batch folds in one pass: the fold works on locals and state is committed
        // only after the whole batch passed validation. A rejected call therefore leaves the
        // statistic exactly as it was. The counts and the sum read the same values the test
        // saved before the call.
        var fresh = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => fresh.Fit(new double[] { 1.0, double.PositiveInfinity, 2.0 }));
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(0.0, fresh.Value);

        var fitted = new RunningStatistics.Sum();
        fitted.Fit(1.5, 3); // sum of three 1.5 observations: Nobs 3, Value 4.5
        var nobsBefore = fitted.Nobs;
        var valueBefore = fitted.Value;
        Assert.Throws<ArgumentException>(() => fitted.Fit(new double[] { 1.0, double.NaN, 2.0 }));
        Assert.Equal(nobsBefore, fitted.Nobs);  // the saved values still match
        Assert.Equal(valueBefore, fitted.Value);
    }

    [Fact]
    public void Merge_HugeFiniteValuesSaturateValueToInfinity()
    {
        // This observation records the current behavior, and does not fix it. The running total
        // is a double, so adding huge finite values saturates it to an infinity, the way IEEE
        // arithmetic does. Only the total goes non-finite. The observation count stays finite and
        // exact. Merge has no guards of its own. Fit rejects non-finite observations, yet nothing
        // stops the accumulated total itself from overflowing.
        var pos1 = new RunningStatistics.Sum();
        var pos2 = new RunningStatistics.Sum();
        pos1.Fit(1.7e308);
        pos2.Fit(1.7e308);
        pos1.Merge(pos2);
        Assert.Equal(2, pos1.Nobs);
        Assert.Equal(double.PositiveInfinity, pos1.Value);
        Assert.Equal(double.PositiveInfinity, pos1.Mean());
        Assert.Equal(1, pos2.Nobs);
        Assert.Equal(1.7e308, pos2.Value);

        var neg1 = new RunningStatistics.Sum();
        var neg2 = new RunningStatistics.Sum();
        neg1.Fit(-1.7e308);
        neg2.Fit(-1.7e308);
        neg1.Merge(neg2);
        Assert.Equal(2, neg1.Nobs);
        Assert.Equal(double.NegativeInfinity, neg1.Value);

        // The same saturation happens on a weighted fit of a count above one. Value times
        // count passes beyond the largest double and lands on an infinity.
        var fitSaturated = new RunningStatistics.Sum();
        fitSaturated.Fit(1.7e308, 2);
        Assert.Equal(2, fitSaturated.Nobs);
        Assert.Equal(double.PositiveInfinity, fitSaturated.Value);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.

    // ---- Correctness gates and regression benches for Sum's accumulation ----
    //
    // Sum keeps a correction beside its running total, so its error stays near one epsilon
    // of the running totals. These facts hold that promise, and they hold the timing flat.
    // The staging run (2026-11-19) recorded what the plain fold used before this change was
    // worth. On the canceling blocks below, from kappa 1e14 up, the plain fold read 0 where
    // the exact answer is 75: an absolute error of 75, or 100% relative. On the one-sign
    // kappa sweep the plain fold was about one epsilon relative, up to 1.6e-2 absolute at
    // kappa 1e12. The compensated fold matched the compensated reference exactly on both,
    // and its cost came in at 0.95 to 1.04 of the plain fold's.
    //
    // A red Gate_ fact means the accumulation lost accuracy. A red Bench_ fact means a
    // regression in cost. Fix the algorithm in either case, and do not widen a band or a
    // tolerance to make a fact pass. The datasets are seeded, so a failure reproduces.

    [Fact]
    public void Gate_SumKappaSweepWithinTolerance()
    {
        // One-sign kappa sweep, from kappa 1 to 1e12. The readout must stay within a
        // hundredth of a percent of the compensated reference. A plain fold is inside
        // about one epsilon relative here, so this gate mainly catches a broken
        // compensation, not a missing one. The gate below catches a missing one.
        const int n = 100;
        var sets = PropertyHelpers.KappaSweep(n, 12, 20251125L);
        for (var d = 0; d <= 12; d++)
        {
            var xs = sets[d];
            var reference = PropertyHelpers.SumReference(xs);
            var sum = new RunningStatistics.Sum();
            for (var i = 0; i < n; i++) sum.Fit(xs[i]);

            var tolerance = 1e-8 * Math.Abs(reference);
            Assert.True(Math.Abs(sum.Value - reference) <= tolerance,
                        $"Sum vs compensated reference at kappa {Math.Pow(10.0, d):E0}: {PropertyHelpers.Describe("sum", reference, sum.Value)}, abs tol {tolerance:E2}");
        }
    }

    [Fact]
    public void Gate_SumCancellationWithinTolerance()
    {
        // Canceling blocks: one large power-of-two term, a run of ones, then the negation
        // of that term. Every term is exact, and a block's exact sum is its count of ones,
        // so the reference carries no rounding of its own. Once a term passes 2^53 its
        // spacing exceeds one, and a plain fold drops every one: it reads 0 against an
        // exact 75. The tolerance is a hundredth of a percent of that answer, so a plain
        // fold cannot pass and the compensation has to be there.
        const int n = 100;
        for (var d = 12; d <= 18; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var xs = PropertyHelpers.CancelingStream(n, kappa, 771_257L + d);
            var reference = PropertyHelpers.SumReference(xs);

            var sum = new RunningStatistics.Sum();
            for (var i = 0; i < xs.Count; i++) sum.Fit(xs[i]);

            var tolerance = 1e-8 * Math.Abs(reference);
            Assert.True(Math.Abs(sum.Value - reference) <= tolerance,
                        $"Sum vs compensated reference on canceling blocks at kappa {kappa:E0}: {PropertyHelpers.Describe("sum", reference, sum.Value)}, abs tol {tolerance:E2}");
        }
    }

    [Fact]
    public void Gate_SumDegeneratePaths()
    {
        // Constant data reads the compensated reference, within a hundredth of a percent.
        foreach (var c in new[] { 1234.5, -0.5, 1e12 })
        {
            var xs = PropertyHelpers.Constant(1000, c);
            var reference = PropertyHelpers.SumReference(xs);
            var sum = new RunningStatistics.Sum();
            sum.Fit(xs);
            Assert.True(Math.Abs(sum.Value - reference) <= 1e-8 * Math.Abs(reference),
                        $"constant {c}: {PropertyHelpers.Describe("sum", reference, sum.Value)}");
        }

        // A weighted fit matches the loop of single fits, and both match the naive
        // accumulation of the same value, within a hundredth of a percent.
        foreach (var value in new[] { 0.1, 1.0 / 3.0, 1.7 })
        {
            const long count = 10_003;
            var reference = 0.0;
            var exact = new RunningStatistics.Sum();
            for (var k = 0L; k < count; k++)
            {
                reference += value;
                exact.Fit(value);
            }

            var weighted = new RunningStatistics.Sum();
            weighted.Fit(value, count);

            var scale = Math.Abs(reference);
            Assert.True(Math.Abs(weighted.Value - exact.Value) <= 1e-8 * scale,
                        $"weighted vs loop for {value}: {PropertyHelpers.Describe("weighted vs loop", exact.Value, weighted.Value)}");
            Assert.True(Math.Abs(weighted.Value - reference) <= 1e-8 * scale,
                        $"weighted vs accumulation for {value}: {PropertyHelpers.Describe("weighted vs accumulation", reference, weighted.Value)}");
        }

        // A clone reads state equivalent to the original.
        var source = new RunningStatistics.Sum();
        source.Fit(PropertyHelpers.OffsetSpread(50, 1e9, 7.0, 20251126L));
        var clone = source.Clone();
        Assert.Equal(source.Nobs, clone.Nobs);
        Assert.True(Math.Abs(clone.Value - source.Value) <= 1e-8 * Math.Abs(source.Value),
                    $"clone drift: {PropertyHelpers.Describe("clone vs source", source.Value, clone.Value)}");

        // An empty merge is an identity on both sides.
        var filled = new RunningStatistics.Sum();
        filled.Fit(2.5, 4);
        var before = filled.Value;
        var fresh = new RunningStatistics.Sum();
        filled.Merge(fresh);
        Assert.Equal(before, filled.Value);
        fresh.Merge(filled);
        Assert.Equal(4, fresh.Nobs);
        Assert.Equal(filled.Value, fresh.Value);
    }

    [Fact]
    public void Gate_SumMergeAssociativity()
    {
        // (a merged with b) merged with c must equal a merged with (b merged with c), and
        // both must equal the compensated reference of the three chunks together. Three
        // chunks per condition number, up to kappa 1e8. The margins follow the Variance
        // merge gate: 1e-8 plus ten thousand epsilums of the condition number.
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 11251128 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 33251129 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 55251131 + d);

            var all = new List<double>();
            foreach (var v in chunkA) all.Add(v);
            foreach (var v in chunkB) all.Add(v);
            foreach (var v in chunkC) all.Add(v);
            var reference = PropertyHelpers.SumReference(all);

            var a = new RunningStatistics.Sum();
            var b = new RunningStatistics.Sum();
            var c = new RunningStatistics.Sum();
            foreach (var v in chunkA) a.Fit(v);
            foreach (var v in chunkB) b.Fit(v);
            foreach (var v in chunkC) c.Fit(v);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var bc = b.Clone();
            bc.Merge(c);
            var g2 = a.Clone();
            g2.Merge(bc);

            Assert.True(PropertyHelpers.Within(reference, g1.Value, relTol), PropertyHelpers.Describe("(a+b)+c vs reference", reference, g1.Value));
            Assert.True(PropertyHelpers.Within(reference, g2.Value, relTol), PropertyHelpers.Describe("a+(b+c) vs reference", reference, g2.Value));
            Assert.True(PropertyHelpers.Within(g1.Value, g2.Value, relTol), PropertyHelpers.Describe("(a+b)+c vs a+(b+c)", g1.Value, g2.Value));
        }
    }

    [Fact]
    public void Gate_SumMergeCommutativity()
    {
        // The same merge contract in the other order: a,b,c against c,b,a.
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 22251138 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 44251139 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 66251141 + d);

            var all = new List<double>();
            foreach (var v in chunkA) all.Add(v);
            foreach (var v in chunkB) all.Add(v);
            foreach (var v in chunkC) all.Add(v);
            var reference = PropertyHelpers.SumReference(all);

            var a = new RunningStatistics.Sum();
            var b = new RunningStatistics.Sum();
            var c = new RunningStatistics.Sum();
            foreach (var v in chunkA) a.Fit(v);
            foreach (var v in chunkB) b.Fit(v);
            foreach (var v in chunkC) c.Fit(v);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var g3 = c.Clone();
            g3.Merge(b);
            g3.Merge(a);

            Assert.True(PropertyHelpers.Within(reference, g1.Value, relTol), PropertyHelpers.Describe("a+b+c vs reference", reference, g1.Value));
            Assert.True(PropertyHelpers.Within(reference, g3.Value, relTol), PropertyHelpers.Describe("c+b+a vs reference", reference, g3.Value));
            Assert.True(PropertyHelpers.Within(g1.Value, g3.Value, relTol), PropertyHelpers.Describe("a+b+c vs c+b+a", g1.Value, g3.Value));
        }
    }

    // The bands below are the AGENTS.md budgets, read against the real class. The getter and
    // the clone facts compare identical machine code, so a ratio away from 1.00 by more than
    // the noise is a real change in cost, and the honest claim is "not slower". Do not widen
    // a band to make a fact green.

    [Fact]
    public void Bench_SumStreamFitCostFlatInWorkingSet()
    {
        // Both sweeps fit through the interface over one fixed-seed dataset. The 1k set
        // stays in cache and shows the arithmetic; the 1M set models realistic streaming
        // and is memory-bound. A compensated fold that hid its work behind the loads keeps
        // the two within 20%. A fold whose cost is bound up in latency or a mispredicted
        // branch shows up here as red.
        var smallData = PropertyHelpers.UniformUnit(1000, 20251126L);
        var bigData = PropertyHelpers.UniformUnit(1_000_000, 20251126L);

        var hotNs = BenchmarkHarness.StreamFit(() => new RunningStatistics.Sum(), smallData, 4);
        var coldNs = BenchmarkHarness.StreamFit(() => new RunningStatistics.Sum(), bigData, 4);
        var ratio = BenchmarkHarness.Compare(
            "Sum StreamFit 1M over 1k working set, ns per observation",
            hotNs,
            coldNs);
        Assert.True(ratio >= 0.5 && ratio <= 1.20,
                    $"StreamFit 1M over 1k ratio {ratio:F3} outside [0.5, 1.20]: the hot loop got slower than the memory-bound one; re-gate, do not re-tune the band");
    }

    [Fact]
    public void Bench_SumWeightedFitIsPerCallCost()
    {
        // One weighted call per observation, against one single call per observation. The
        // weighted path is O(1) in the count, so the two must stay within 20%. A weighted
        // fit that looped per observation would leave this fact red.
        var data = PropertyHelpers.UniformUnit(1000, 20251126L);
        var fitNs = BenchmarkHarness.StreamFit(() => new RunningStatistics.Sum(), data, 4);
        var weightedNs = BenchmarkHarness.WeightedFit(() => new RunningStatistics.Sum(), data, 1003, 4);
        var ratio = BenchmarkHarness.Compare(
            "Sum WeightedFit count=1003 over StreamFit, ns per observation",
            fitNs,
            weightedNs);
        Assert.True(ratio >= 0.5 && ratio <= 1.20,
                    $"WeightedFit over StreamFit ratio {ratio:F3} outside [0.5, 1.20]: the weighted path spends per-observation work; re-gate, do not re-tune the band");
    }

    [Fact]
    public void Bench_SumMergeIsPerChunkCost()
    {
        // Eight chunks merged one by one, against a straight stream. Merge is O(1) per
        // chunk, so the two must stay within 20%.
        var data = PropertyHelpers.UniformUnit(1000, 20251126L);
        var fitNs = BenchmarkHarness.StreamFit(() => new RunningStatistics.Sum(), data, 4);
        var mergeNs = BenchmarkHarness.MergeChunks(() => new RunningStatistics.Sum(), data, 8, 4);
        var ratio = BenchmarkHarness.Compare(
            "Sum MergeChunks 8 chunks over StreamFit, ns per observation",
            fitNs,
            mergeNs);
        Assert.True(ratio >= 0.5 && ratio <= 1.20,
                    $"MergeChunks over StreamFit ratio {ratio:F3} outside [0.5, 1.20]: merge spends per-observation work; re-gate, do not re-tune the band");
    }

    [Fact]
    public void Bench_SumBatchFitCostFlatInBatchSize()
    {
        // The batch path folds the iterable directly, no List and no separate validate
        // pass. The staging run of 2026-11-25 recorded the materialize-and-two-pass batch
        // baseline: the single-pass candidate cost 0.445 of it at 1k batches (15.7 vs 35.3
        // ns/obs) and 0.736 at 100k batches (15.0 vs 20.4 ns/obs), on bit-identical state.
        // Both sides here process one million observations at cache-resident sizes: one
        // hundred batches of ten thousand, against ten batches of one hundred thousand.
        // Per-batch setup is amortized far enough on both sides, so the per-observation
        // cost of the two sizes must stay within 20% of each other. A fold whose setup
        // grew with the square of the batch size leaves this fact red. Small batches of
        // one thousand are NOT swept here: their per-batch noise straddles the band on
        // slower machines, the way the Sum getter noise straddles its band. A single
        // one-million batch is not swept either: the memory-bound gap of an 8 MB batch
        // is not a regression in the fold, the same reason the Mean twin skips it.
        var smallBatches = PropertyHelpers.UniformUnit(10_000, 20251126L);
        var bigBatches = PropertyHelpers.UniformUnit(100_000, 20251126L);

        var smallNsPerObs = BenchmarkHarness.Measure(
            "Sum batch fit, 100 batches of 10000, ns per observation",
            1_000_000,
            4,
            () => SweepSumBatchFits(smallBatches, 100));
        var bigNsPerObs = BenchmarkHarness.Measure(
            "Sum batch fit, 10 batches of 100000, ns per observation",
            1_000_000,
            4,
            () => SweepSumBatchFits(bigBatches, 10));
        var ratio = BenchmarkHarness.Compare(
            "Sum batch fit 100k batches over 10k batches, ns per observation",
            smallNsPerObs,
            bigNsPerObs);
        Assert.True(ratio >= 0.5 && ratio <= 1.20,
                    $"batch fit 100k over 10k ratio {ratio:F3} outside [0.5, 1.20]: the fold or its setup does not scale linearly with the batch; re-gate, do not re-tune the band");
    }

    [Fact]
    public void Bench_SumGetterCostIndependentOfNobs()
    {
        // One statistic sees 2 to the 62nd observations through 61 self merges; the other
        // holds six. Both go through the same readout code, so the band is [0.5, 1.05] and
        // the honest claim is "not slower": the readout adds the correction back, one
        // addition more than a plain total. The Sum readouts are two cheap arithmetic
        // operations, so min-of-four noise straddles the band by a few percent here; eight
        // measured runs take the minimum away from that straddle. The workload stays at the
        // harness default of five million reads.
        const int obs = 5_000_000;
        const int repeats = 8;
        var values = PropertyHelpers.UniformUnit(6, 20251118L);

        var big = new RunningStatistics.Sum();
        big.Fit(values[0]);
        big.Fit(values[1]);
        for (var i = 0; i < 61; i++) big.Merge(big);
        Assert.Equal(1L << 62, big.Nobs);

        var small = new RunningStatistics.Sum();
        foreach (var value in values) small.Fit(value);
        Assert.Equal(6, small.Nobs);

        Assert.False(double.IsNaN(big.Value) || double.IsNaN(big.Mean()));
        Assert.False(double.IsNaN(small.Value) || double.IsNaN(small.Mean()));

        var bigNsPerObs = BenchmarkHarness.Measure(
            "Sum readout sweep at Nobs=2^62, ns per read",
            obs,
            repeats,
            () => SweepSumReadouts(big, obs));

        var smallNsPerObs = BenchmarkHarness.Measure(
            "Sum readout sweep at Nobs=6, ns per read",
            obs,
            repeats,
            () => SweepSumReadouts(small, obs));

        var ratio = BenchmarkHarness.Compare(
            "Sum readout at Nobs=2^62 vs at Nobs=6, ns per read",
            smallNsPerObs,
            bigNsPerObs);

        Assert.True(ratio >= 0.5 && ratio <= 1.05,
                    $"2^62-scale readout sweep costs {ratio:F3} of the same sweep at Nobs=6 " +
                    $"({bigNsPerObs:F4} vs {smallNsPerObs:F4} ns per read): the readout getters " +
                    $"got slower once Nobs reaches 2^62; the claim is parity, so re-gate - do not widen the band.");
    }

    [Fact]
    public void Bench_SumCloneCostIndependentOfNobs()
    {
        // A clone is an allocation plus an O(1) merge, so taking a snapshot after every
        // observation must not depend on the count. The band is [0.5, 1.05]: both sides run
        // the same body, and the honest claim is "not slower".
        const int obs = 500_000;
        const int repeats = 4;
        var values = PropertyHelpers.UniformUnit(6, 20251118L);

        var big = new RunningStatistics.Sum();
        big.Fit(values[0]);
        big.Fit(values[1]);
        for (var i = 0; i < 61; i++) big.Merge(big);
        Assert.Equal(1L << 62, big.Nobs);

        var small = new RunningStatistics.Sum();
        foreach (var value in values) small.Fit(value);
        Assert.Equal(6, small.Nobs);

        var bigNsPerObs = BenchmarkHarness.Measure(
            "Sum clone-per-step sweep at Nobs=2^62, ns per step",
            obs,
            repeats,
            () => SweepSumClones(big, obs));

        var smallNsPerObs = BenchmarkHarness.Measure(
            "Sum clone-per-step sweep at Nobs=6, ns per step",
            obs,
            repeats,
            () => SweepSumClones(small, obs));

        var ratio = BenchmarkHarness.Compare(
            "Sum clone-per-step at Nobs=2^62 vs at Nobs=6, ns per step",
            smallNsPerObs,
            bigNsPerObs);

        Assert.True(ratio >= 0.5 && ratio <= 1.05,
                    $"2^62-scale clone sweep costs {ratio:F3} of the same sweep at Nobs=6 " +
                    $"({bigNsPerObs:F4} vs {smallNsPerObs:F4} ns per step): cloning picked up work " +
                    $"that grows with the count; the claim is parity, so re-gate - do not widen the band.");
    }

    private static double SweepSumReadouts(RunningStatistics.Sum stat, int obs)
    {
        var s = 0.0;
        for (var i = 0; i < obs; i++) s += stat.Value + stat.Mean();
        return s;
    }

    private static double SweepSumClones(RunningStatistics.Sum stat, int obs)
    {
        var s = 0.0;
        for (var i = 0; i < obs; i++)
        {
            var copy = stat.Clone();
            s += copy.Nobs;
        }

        return s;
    }

    private static double SweepSumBatchFits(List<double> data, int passes)
    {
        var s = 0.0;
        for (var p = 0; p < passes; p++)
        {
            var stat = new RunningStatistics.Sum();
            stat.Fit(data);
            s += stat.Nobs;
        }

        return s;
    }
}