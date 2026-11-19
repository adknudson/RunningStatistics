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
    // Pinned: Normal's degenerate-spread behavior on current code.
    // Normal = Mean + Variance; Utils.Smooth(a, b, p) = a + p*(b - a), so
    // smoothing equal values is exact and the variance terms collapse to
    // exact 0 on all-equal data. All values below are exact unless pinned
    // via AssertIsApproximate (absTol 0 = 1e-8 relative) for sqrt/division.
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
        // Fit(2); Fit(8): internal M2 = (8-5)*(8-2) = 18/2 = 9 exactly; unbiased
        // variance = 9 * Bessel(2) = 18 exactly = (a-b)^2 / 2; SD = |a-b|/sqrt(2).
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
        // Values {2,2,2,8,8,8,8,8}: mean 5.75 exact; ssq 67.5; unbiased variance
        // 67.5/7 (not exactly representable) vs sqrt(67.5/7) sample SD.
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
        // Both stats all-7: merge delta = 0, both internal M2 = 0 -> spread stays
        // exactly 0, mean stays exactly 7.
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
        // {7,7} merged with {5,5}: mean 6 exact; pooled unbiased variance =
        // ssq(4)/(4-1) = 4/3; SD = sqrt(4/3) = 2/sqrt(3).
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
        // data.merge(empty): empty other contributes g = 0 -> state untouched.
        var data = new RunningStatistics.Normal();
        data.Fit(7.0, 2);
        var empty = new RunningStatistics.Normal();
        data.Merge(empty);
        Assert.Equal(2, data.Nobs);
        Assert.Equal(7.0, data.Mean);
        Assert.Equal(0.0, data.Variance);
        Assert.Equal(0.0, data.StandardDeviation);
        
        // empty still has no data after being merged FROM (other is not mutated).
        Assert.Equal(0, empty.Nobs);
        Assert.Equal(double.NaN, empty.Mean);
        
        // empty.merge(other) copies other's degenerate state exactly.
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
        
        // Variance/SD stay NaN because Nobs = 1 < 2 (identity preserved).
        var stat = new RunningStatistics.Normal();
        stat.Fit(9.0);
        stat.Fit(2.5, 0);
        Assert.Equal(1, stat.Nobs);
        Assert.Equal(9.0, stat.Mean);
        Assert.Equal(double.NaN, stat.Variance);
        Assert.Equal(double.NaN, stat.StandardDeviation);
        
        // Empty batch Fit must be a no-op like Mean/Variance batch Fits.
        stat.Fit(new double[0]);
        Assert.Equal(1, stat.Nobs);
        Assert.Equal(9.0, stat.Mean);
        Assert.Equal(double.NaN, stat.Variance);
        Assert.Equal(double.NaN, stat.StandardDeviation);
        
        fresh.Fit(new double[0]);
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(double.NaN, fresh.Mean);
    }

    // PERMANENT merge-algebra gate (keep): Normal Merge associativity. Three seeded KappaStream
    // chunks (n=100, spread 1) per κ, κ = 10^0..10^8; (a∪b)∪c vs a∪(b∪c) with fresh Clone()
    // (CloneEmpty + Merge) receivers. Mean, Variance and StandardDeviation of both groupings
    // must match the two-pass reference on a∪b∪c, and each other, within relTol = 1e-8 +
    // 1e4·κ·ε (all Normal getters are scale-valued, so relative margins apply).
    // Probe worst observed: Variance rel 2.40E-8, StdDev rel 1.20E-8, Mean rel 3.7E-16 at
    // κ=1e8 (merge errors scale ~linearly in κ, not κ²) → headroom ~1e4x.
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

    // Same contract, commutative direction: a∪b∪c vs c∪b∪a, same κ range (10^0..10^8),
    // fresh Clone() receivers and margins relTol = 1e-8 + 1e4·κ·ε.
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
}