using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Normal;

public partial class TestNormal
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => normal.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, normal.Nobs);
    }

    [Fact]
    public void NoData_MomentsAreNaN()
    {
        var x = new RunningStatistics.Normal();
        Assert.Equal(double.NaN, x.Mean);
        Assert.Equal(double.NaN, x.Variance);
        Assert.Equal(double.NaN, x.StandardDeviation);
    }
    
    [Fact]
    public void NoData_NobsIsZero()
    {
        var x = new RunningStatistics.Normal();
        Assert.Equal(0, x.Nobs);
    }
    
    [Fact]
    public void Fit_SingleFiniteObservation()
    {
        var x = new RunningStatistics.Normal();
        x.Fit(10);
        Assert.Equal(1, x.Nobs);
        Assert.Equal(10, x.Mean);
        Assert.Equal(double.NaN, x.Variance);
        Assert.Equal(double.NaN, x.StandardDeviation);
    }
    
    [Fact]
    public void SingleInfiniteObservation_Throws()
    {
        var x = new RunningStatistics.Normal();
        Assert.Throws<ArgumentException>(() => x.Fit(double.PositiveInfinity));
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(0, 5)]
    [InlineData(0, 10)]
    public void Pdf_ReturnsCorrectValues(double mean, double std)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, std);
        
        var norm = new RunningStatistics.Normal();
        
        for (var i = 0; i < n; i++)
        {
            norm.Fit(dist.Sample());
        }
        
        Utils.AssertIsApproximate(mean, norm.Mean, 1e-2);
        Utils.AssertIsApproximate(std, norm.StandardDeviation, 1e-2);
        
        Utils.AssertIsApproximate(dist.Density(-2), norm.Pdf(-2), 1e-2);
        Utils.AssertIsApproximate(dist.Density(-1), norm.Pdf(-1), 1e-2);
        Utils.AssertIsApproximate(dist.Density( 0), norm.Pdf( 0), 1e-2);
        Utils.AssertIsApproximate(dist.Density( 1), norm.Pdf( 1), 1e-2);
        Utils.AssertIsApproximate(dist.Density( 2), norm.Pdf( 2), 1e-2);
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(0, 5)]
    [InlineData(0, 10)]
    public void Cdf_ReturnsCorrectValues(double mean, double std)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, std);
        
        var norm = new RunningStatistics.Normal();
        
        for (var i = 0; i < n; i++)
        {
            norm.Fit(dist.Sample());
        }
        
        Utils.AssertIsApproximate(mean, norm.Mean, 1e-2);
        Utils.AssertIsApproximate(std, norm.StandardDeviation, 1e-2);
        
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-2), norm.Cdf(-2), 1e-2);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-1), norm.Cdf(-1), 1e-2);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 0), norm.Cdf( 0), 1e-2);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 1), norm.Cdf( 1), 1e-2);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 2), norm.Cdf( 2), 1e-2);
    }

    [Theory]
    [InlineData(0, 1, 0.01)]
    [InlineData(0, 2, 0.01)]
    [InlineData(0, 5, 0.01)]
    [InlineData(0, 10, 0.1)]
    public void Quantile_ReturnsCorrectValues(double mean, double std, double tol)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, std);
        
        var norm = new RunningStatistics.Normal();
        
        for (var i = 0; i < n; i++)
        {
            norm.Fit(dist.Sample());
        }
        
        Utils.AssertIsApproximate(mean, norm.Mean, 1e-2);
        Utils.AssertIsApproximate(std, norm.StandardDeviation, 1e-2);
        
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), norm.Quantile(0.01), tol);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.05), norm.Quantile(0.05), tol);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), norm.Quantile(0.50), tol);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.95), norm.Quantile(0.95), tol);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), norm.Quantile(0.99), tol);
    }
    
    // --------------------------------------------------------------------
    // The facts below pin the degenerate-spread behavior of Normal as the current code
    // has it. A Normal keeps its readouts as a mean and a variance. The smoothing
    // helper blends two values by a weighted path between them. Blending equal
    // values is therefore exact, so the variance terms collapse to exact zero on
    // all-equal data. The values below are exact except where the fact pins
    // them with the approximate assertion. That assertion allows a relative
    // hundred-millionth where square roots and divisions are involved.
    // --------------------------------------------------------------------
    
    [Fact]
    public void Degenerate_AllEqualLoop_ExactMeanZeroSpread()
    {
        var normal = new RunningStatistics.Normal();
        for (var i = 0; i < 5; i++)
        {
            normal.Fit(7.0);
        }
        Assert.Equal(5, normal.Nobs);
        Assert.Equal(7.0, normal.Mean);
        Assert.Equal(0.0, normal.Variance);
        Assert.Equal(0.0, normal.StandardDeviation);
    }
    
    [Fact]
    public void Degenerate_AllEqualWeightedFit_ExactMeanZeroSpread()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(7.0, 5);
        Assert.Equal(5, normal.Nobs);
        Assert.Equal(7.0, normal.Mean);
        Assert.Equal(0.0, normal.Variance);
        Assert.Equal(0.0, normal.StandardDeviation);
    }
    
    [Fact]
    public void Degenerate_AllEqualBatchFit_ExactMeanZeroSpread()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(new[] { 7.0, 7.0, 7.0, 7.0 });
        Assert.Equal(4, normal.Nobs);
        Assert.Equal(7.0, normal.Mean);
        Assert.Equal(0.0, normal.Variance);
        Assert.Equal(0.0, normal.StandardDeviation);
    }
    
    [Fact]
    public void Degenerate_TwoPointSpread_MatchesSampleSD()
    {
        // The fact fits 2 and 8. The mean is 5. The internal spread sum works out to
        // exactly 9. The unbiased variance is exactly 18, half the squared gap of 6. The
        // standard deviation is that gap over the square root of two.
        var normal = new RunningStatistics.Normal();
        normal.Fit(2.0);
        normal.Fit(8.0);
        Assert.Equal(2, normal.Nobs);
        Assert.Equal(5.0, normal.Mean);
        Assert.Equal(18.0, normal.Variance);
        Utils.AssertIsApproximate(Math.Abs(8.0 - 2.0) / Math.Sqrt(2.0), normal.StandardDeviation, 0);
    }
    
    [Fact]
    public void Degenerate_TwoPointWeightedSpread_MatchesPooledSD()
    {
        // The values 2, 2, 2, 8, 8, 8, 8, 8: the mean 5.75 is exact. The spread sum is 67.5, so
        // the unbiased variance is 67.5 over 7, not exactly representable, and the sample
        // standard deviation its square root.
        var normal = new RunningStatistics.Normal();
        normal.Fit(2.0, 3);
        normal.Fit(8.0, 5);
        Assert.Equal(8, normal.Nobs);
        Assert.Equal(5.75, normal.Mean);
        Utils.AssertIsApproximate(67.5 / 7.0, normal.Variance, 0);
        Utils.AssertIsApproximate(Math.Sqrt(67.5 / 7.0), normal.StandardDeviation, 0);
    }
    
    [Fact]
    public void Degenerate_MergeTwoConstantStats_SpreadStaysZero()
    {
        // Both statistics hold only 7s. The merge therefore sees no shift between the
        // means, and both internal spread sums are zero. The spread stays exactly zero
        // and the mean stays exactly 7.
        var left = new RunningStatistics.Normal();
        left.Fit(7.0, 3);
        var right = new RunningStatistics.Normal();
        right.Fit(7.0, 4);
        left.Merge(right);
        Assert.Equal(7, left.Nobs);
        Assert.Equal(7.0, left.Mean);
        Assert.Equal(0.0, left.Variance);
        Assert.Equal(0.0, left.StandardDeviation);
    }
    
    [Fact]
    public void Degenerate_MergeTwoConstants_PooledSpreadMatchesReference()
    {
        // The pair 7, 7 merged with the pair 5, 5: the mean 6 is exact. Pooling leaves a spread of
        // four over the other three observations, 4/3, and a standard deviation of two over the
        // square root of three.
        var left = new RunningStatistics.Normal();
        left.Fit(7.0, 2);
        var right = new RunningStatistics.Normal();
        right.Fit(5.0, 2);
        left.Merge(right);
        Assert.Equal(4, left.Nobs);
        Assert.Equal(6.0, left.Mean);
        Utils.AssertIsApproximate(4.0 / 3.0, left.Variance, 0);
        Utils.AssertIsApproximate(Math.Sqrt(4.0 / 3.0), left.StandardDeviation, 0);
    }
    
    [Fact]
    public void Degenerate_MergeWithEmptyBothDirections()
    {
        // merging an empty statistic into a filled one contributes nothing, leaving its state
        // untouched.
        var data = new RunningStatistics.Normal();
        data.Fit(7.0, 2);
        var empty = new RunningStatistics.Normal();
        data.Merge(empty);
        Assert.Equal(2, data.Nobs);
        Assert.Equal(7.0, data.Mean);
        Assert.Equal(0.0, data.Variance);
        Assert.Equal(0.0, data.StandardDeviation);
        
        // the empty statistic still holds no data: the merge leaves its argument unmodified.
        Assert.Equal(0, empty.Nobs);
        Assert.Equal(double.NaN, empty.Mean);
        
        // merging a filled statistic into an empty one copies its degenerate state exactly.
        var other = new RunningStatistics.Normal();
        other.Fit(3.0, 2);
        empty.Merge(other);
        Assert.Equal(2, empty.Nobs);
        Assert.Equal(3.0, empty.Mean);
        Assert.Equal(0.0, empty.Variance);
        Assert.Equal(0.0, empty.StandardDeviation);
    }
    
    [Fact]
    public void Degenerate_FitZeroCountAndEmptyBatch_AreNoOps()
    {
        var fresh = new RunningStatistics.Normal();
        fresh.Fit(7.0, 0);
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(double.NaN, fresh.Mean);
        Assert.Equal(double.NaN, fresh.Variance);
        Assert.Equal(double.NaN, fresh.StandardDeviation);
        
        // variance and standard deviation stay NaN, as they must, because a single observation is
        // fewer than two.
        var stat = new RunningStatistics.Normal();
        stat.Fit(9.0);
        stat.Fit(2.5, 0);
        Assert.Equal(1, stat.Nobs);
        Assert.Equal(9.0, stat.Mean);
        Assert.Equal(double.NaN, stat.Variance);
        Assert.Equal(double.NaN, stat.StandardDeviation);
        
        // fitting a zero count must do nothing, matching how the mean and the variance treat empty
        // batches.
        stat.Fit(new double[0]);
        Assert.Equal(1, stat.Nobs);
        Assert.Equal(9.0, stat.Mean);
        Assert.Equal(double.NaN, stat.Variance);
        Assert.Equal(double.NaN, stat.StandardDeviation);
        
        fresh.Fit(new double[0]);
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(double.NaN, fresh.Mean);
    }

    // This merge-algebra gate is permanent, so keep it. Merging normal statistics is
    // associative. The fact builds three seeded KappaStream chunks for each
    // condition number. The walk runs powers of ten from one to one hundred million.
    // Each chunk holds one hundred observations with spread one. The grouping of a
    // merged with b, then merged with c, is compared against a merged with the
    // result of b merged with c. Each grouping runs on a fresh copy made through
    // CloneEmpty and Merge. The mean, variance and standard deviation of both
    // groupings must match the two-pass reference over all three chunks together.
    // The groupings must also match each other. The relative margin is a
    // hundred-millionth plus ten thousand times the condition number times
    // machine epsilon. Every readout of Normal carries the data's units, so a
    // relative margin is the right shape. The probe found the worst errors at
    // condition number 1e8. They were a relative 2.40E-8 on variance, 1.20E-8
    // on standard deviation and 3.7E-16 on mean. Merge errors grow roughly
    // linearly in the condition number, not its square. That leaves about ten
    // thousand times headroom.
    [Fact]
    public void Gate_MergeAssociativityNormal()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 14251128 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 24251129 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 34251131 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Normal();
            var b = new RunningStatistics.Normal();
            var c = new RunningStatistics.Normal();
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

            AssertNormalAgainstReference("(a∪b)∪c", reference, g1, relTol);
            AssertNormalAgainstReference("a∪(b∪c)", reference, g2, relTol);
            AssertNormalGroupingsAgree("(a∪b)∪c vs a∪(b∪c)", g1, g2, relTol);
        }
    }

    // This fact checks the same contract in the commutative direction. It merges a,
    // b, c against c, b, a over the same condition-number range. Both sides merge
    // into fresh copies as receivers, with the same margins as the associativity
    // gate above.
    [Fact]
    public void Gate_MergeCommutativityNormal()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 41251138 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 51251139 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 61251141 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Normal();
            var b = new RunningStatistics.Normal();
            var c = new RunningStatistics.Normal();
            for (var i = 0; i < n; i++) a.Fit(chunkA[i]);
            for (var i = 0; i < n; i++) b.Fit(chunkB[i]);
            for (var i = 0; i < n; i++) c.Fit(chunkC[i]);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var g3 = c.Clone();
            g3.Merge(b);
            g3.Merge(a);

            AssertNormalAgainstReference("a∪b∪c", reference, g1, relTol);
            AssertNormalAgainstReference("c∪b∪a", reference, g3, relTol);
            AssertNormalGroupingsAgree("a∪b∪c vs c∪b∪a", g1, g3, relTol);
        }
    }

    private static void AssertNormalAgainstReference(string tag, MomentsReference reference, RunningStatistics.Normal merged, double relTol)
    {
        Assert.True(PropertyHelpers.Within(reference.Mean, merged.Mean, relTol), PropertyHelpers.Describe($"{tag} Mean vs reference", reference.Mean, merged.Mean));
        Assert.True(PropertyHelpers.Within(reference.Variance, merged.Variance, relTol), PropertyHelpers.Describe($"{tag} Variance vs reference", reference.Variance, merged.Variance));
        Assert.True(PropertyHelpers.Within(reference.StandardDeviation, merged.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation vs reference", reference.StandardDeviation, merged.StandardDeviation));
    }

    private static void AssertNormalGroupingsAgree(string tag, RunningStatistics.Normal left, RunningStatistics.Normal right, double relTol)
    {
        Assert.True(PropertyHelpers.Within(left.Mean, right.Mean, relTol), PropertyHelpers.Describe($"{tag} Mean", left.Mean, right.Mean));
        Assert.True(PropertyHelpers.Within(left.Variance, right.Variance, relTol), PropertyHelpers.Describe($"{tag} Variance", left.Variance, right.Variance));
        Assert.True(PropertyHelpers.Within(left.StandardDeviation, right.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation", left.StandardDeviation, right.StandardDeviation));
    }

    // This fact checks that reading the getters does not get slower as the statistic sees
    // more observations. The getters look at a fixed amount of state. Reading them should
    // therefore cost the same on a huge statistic as on a small one. The test builds one
    // statistic that has seen 2 to the 62nd power observations, and one that has seen 6.
    // The test times millions of reads of Mean, Variance and StandardDeviation on each
    // statistic. It requires the big one to be within [0.5, 1.05] of the small one. The
    // band is deliberately tight. A getter is one interface dispatch plus a few
    // arithmetic operations, so the honest claim here is "not slower". Measurement noise
    // of about 1% straddles 1.00, so a ratio outside about 1% is noise, not a real gain
    // or regression. If this fact ever goes red, a getter has picked up work that grows
    // with the observation count. Fix the algorithm, and do not widen the band.
    //
    // The code builds the big statistic in one way. It fits two distinct values, then
    // merges the statistic with itself 61 times. Each merge doubles the count, up to
    // 2 to the 62nd power. Two distinct values are needed at the start. Doubling a
    // single observation keeps the data constant, which leaves the variance at 0. The
    // standard-deviation getter then reads the square root of a zero variance. That
    // value is degenerate, so the sweep could time an early exit instead of a real
    // readout. From two distinct values all three readouts stay finite at 2 to the
    // 62nd power. The measured values are Mean about 0.5, Variance about 1e-2 and
    // StandardDeviation about 1e-1. The big and the small statistic go through
    // exactly the same getter code paths. Normal computes its readouts from its
    // internal Mean and Variance statistics, so each read is a hop into a
    // component plus that component's arithmetic. The sweep reads Mean, Variance
    // and StandardDeviation. Those are Normal's readout getters, and Pdf, Cdf
    // and Quantile are not.
    [Fact]
    public void Bench_GetterCostIndependentOfNobs_Normal()
    {
        // The harness default is five million getter reads per measured run, over four
        // measured runs. Both sides of the comparison run the same sweep body, which the
        // helper below provides. The ratio therefore compares identical machine code. The
        // [0.5, 1.05] band is expected at the default size, not at a bigger workload
        // chosen to make it pass.
        const int obs = 5_000_000;
        const int repeats = 4;

        var values = PropertyHelpers.UniformUnit(6, 20251118L);

        var big = new RunningStatistics.Normal();
        big.Fit(values[0]);
        big.Fit(values[1]);
        for (var i = 0; i < 61; i++) big.Merge(big);
        Assert.Equal(1L << 62, big.Nobs);

        var small = new RunningStatistics.Normal();
        foreach (var value in values) small.Fit(value);
        Assert.Equal(6, small.Nobs);

        // preconditions: finite readouts on both sides, and the checksum below stays well inside
        // long range when BenchmarkHarness folds it in
        Assert.False(double.IsNaN(big.Mean) || double.IsNaN(big.Variance) || double.IsNaN(big.StandardDeviation));
        Assert.False(double.IsNaN(small.Mean) || double.IsNaN(small.Variance) || double.IsNaN(small.StandardDeviation));

        var bigNsPerObs = BenchmarkHarness.Measure(
            "Normal readout sweep at Nobs=2^62, ns per sweep",
            obs,
            repeats,
            () => SweepNormal(big, obs));

        var smallNsPerObs = BenchmarkHarness.Measure(
            "Normal readout sweep at Nobs=6, ns per sweep",
            obs,
            repeats,
            () => SweepNormal(small, obs));

        var ratio = BenchmarkHarness.Compare(
            "Normal getter readout at Nobs=2^62 vs at Nobs=6, ns per sweep",
            smallNsPerObs,
            bigNsPerObs);

        Assert.True(ratio >= 0.5 && ratio <= 1.05,
                    $"2^62-scale readout sweep costs {ratio:F3} of the same sweep at Nobs=6 " +
                    $"({bigNsPerObs:F4} vs {smallNsPerObs:F4} ns per sweep): the readout getters " +
                    $"got slower once Nobs reaches 2^62. A getter is one interface dispatch plus a " +
                    $"few flops and the claim is parity (not speed), so re-gate the algorithm - " +
                    $"do not widen the band.");
    }

    // One compiled sweep body serves both sides of the comparison. The statistic is passed in
    // as a parameter. Both workloads therefore run literally the same machine code, with the
    // same JIT tier and code layout. The two workloads are the 2 to the 62nd-power one and
    // the small-Nobs one. This matters because two separately compiled copies of the same
    // loop drifted 7-8% on code layout alone. That drift is noise on a "not slower" claim,
    // not a regression.
    private static double SweepNormal(RunningStatistics.Normal stat, int obs)
    {
        var sum = 0.0;
        for (var i = 0; i < obs; i++)
        {
            sum += stat.Mean;
            sum += stat.Variance;
            sum += stat.StandardDeviation;
        }

        return sum % 1_000_000.0;
    }
}