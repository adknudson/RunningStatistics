using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests.Moments;

public partial class TestMoments
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var m = new RunningStatistics.Moments();
        m.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => m.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, m.Nobs);
    }

    // Accuracy of the stable (Chan pairwise central-sum) Moments on jittered 1e6-scale data
    // (κ=1e6) vs a two-pass reference: the stable update shows ~κ·ε relative on Variance/StdDev
    // (~2e-10) and ~1e-9 absolute on Skewness/Kurtosis, so the tolerances below (~1e-6 abs on
    // g1, ~1e-5 abs on g2) have ~100-1000x margin while the pre-swap raw-power-sums
    // implementation could never come close (its observed errors: Variance rel ~1.7e-4,
    // Skewness abs ~4e4, Kurtosis abs ~2.1e11 — the values the original 1e5/1e12 pins
    // documented). Deterministic seed: these facts are the accuracy regression net.
    [Fact]
    public void Accuracy_StableMomentsNearOffset()
    {
        const int n = 150;
        const double offset = 1e6;
        var rng = new Random(20251114);

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = offset + rng.NextDouble();

        var moments = new RunningStatistics.Moments();
        for (var i = 0; i < n; i++) moments.Fit(values[i]);

        // Two-pass reference: simple mean, then central sums directly.
        var sum = 0.0;
        foreach (var v in values) sum += v;
        var mean = sum / n;

        var c2 = 0.0;
        var c3 = 0.0;
        var c4 = 0.0;
        foreach (var v in values)
        {
            var d = v - mean;
            var d2 = d * d;
            c2 += d2;
            c3 += d2 * d;
            c4 += d2 * d2;
        }

        var refVariance = c2 / (n - 1);
        var refStdDev = Math.Sqrt(refVariance);
        var refSkewness = (c3 / n) / Math.Pow(c2 / n, 1.5);
        var refKurtosis = (c4 / n) / ((c2 / n) * (c2 / n));

        Utils.AssertIsApproximate(mean, moments.Mean, 0.0);
        Utils.AssertIsApproximate(refVariance, moments.Variance, 0.0);
        Utils.AssertIsApproximate(refStdDev, moments.StandardDeviation, 0.0);
        Utils.AssertIsApproximate(refSkewness, moments.Skewness, 1e-6);
        Utils.AssertIsApproximate(refKurtosis, moments.Kurtosis, 1e-5);
        Utils.AssertIsApproximate(refKurtosis - 3, moments.ExcessKurtosis, 1e-5);
    }

    // Same accuracy facts as Accuracy_StableMomentsNearOffset, through the Merge path: the
    // stable merge must be as accurate as the stable stream (the pre-swap merge had the same
    // raw-power-sums garbage: Variance rel err ~4.7e-3, Skewness abs ~4e4, Kurtosis abs ~2.1e11).
    [Fact]
    public void Accuracy_StableMergedMomentsNearOffset()
    {
        const int n = 150;
        const double offset = 1e6;
        var rng = new Random(20251114);

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = offset + rng.NextDouble();

        var left = new RunningStatistics.Moments();
        var right = new RunningStatistics.Moments();
        for (var i = 0; i < n; i++)
        {
            if (i < n / 2) left.Fit(values[i]);
            else right.Fit(values[i]);
        }
        left.Merge(right);

        // Same two-pass reference as Accuracy_StableMomentsNearOffset.
        var sum = 0.0;
        foreach (var v in values) sum += v;
        var mean = sum / n;

        var c2 = 0.0;
        var c3 = 0.0;
        var c4 = 0.0;
        foreach (var v in values)
        {
            var d = v - mean;
            var d2 = d * d;
            c2 += d2;
            c3 += d2 * d;
            c4 += d2 * d2;
        }

        var refVariance = c2 / (n - 1);
        var refStdDev = Math.Sqrt(refVariance);
        var refSkewness = (c3 / n) / Math.Pow(c2 / n, 1.5);
        var refKurtosis = (c4 / n) / ((c2 / n) * (c2 / n));

        Utils.AssertIsApproximate(mean, left.Mean, 0.0);
        Utils.AssertIsApproximate(refVariance, left.Variance, 0.0);
        Utils.AssertIsApproximate(refStdDev, left.StandardDeviation, 0.0);
        Utils.AssertIsApproximate(refSkewness, left.Skewness, 1e-6);
        Utils.AssertIsApproximate(refKurtosis, left.Kurtosis, 1e-5);
    }

    [Fact]
    public void EmptyMoments_AreNaN()
    {
        RunningStatistics.Moments m = new();

        Assert.Equal(double.NaN, m.Mean);
        Assert.Equal(double.NaN, m.Variance);
        Assert.Equal(double.NaN, m.StandardDeviation);
        Assert.Equal(double.NaN, m.Skewness);
        Assert.Equal(double.NaN, m.Kurtosis);
        Assert.Equal(double.NaN, m.ExcessKurtosis);
    }

    [Fact]
    public void Fit_SingleFiniteObservation()
    {
        RunningStatistics.Moments m = new();
        m.Fit(1);
        
        Assert.Equal(1, m.Mean);
        Assert.Equal(double.NaN, m.Variance);
        Assert.Equal(double.NaN, m.StandardDeviation);
        Assert.Equal(double.NaN, m.Skewness);
        Assert.Equal(double.NaN, m.Kurtosis);
        Assert.Equal(double.NaN, m.ExcessKurtosis);
    }
    
    [Fact]
    public void Fit_TwoFiniteObservations()
    {
        RunningStatistics.Moments m = new();
        m.Fit(1);
        m.Fit(2);
        
        Assert.Equal(1.5, m.Mean);
        Assert.Equal(0.5, m.Variance);
        Assert.Equal(0.7071067811865476, m.StandardDeviation, 10);
        Assert.Equal(0, m.Skewness);
        Assert.Equal(1, m.Kurtosis);
        Assert.Equal(-2, m.ExcessKurtosis);
    }
    
    [Fact]
    public void Fit_RejectsNaNAndInfinity()
    {
        RunningStatistics.Moments m = new();
        Assert.Throws<ArgumentException>(() => m.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => m.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => m.Fit(double.NegativeInfinity));
    }

    [Fact]
    public void MergeEmpty_AreNaN()
    {
        RunningStatistics.Moments a = new(); 
        RunningStatistics.Moments b = new(); 
        a.Merge(b);

        Assert.Equal(double.NaN, a.Mean);
        Assert.Equal(double.NaN, a.Variance);
        Assert.Equal(double.NaN, a.Skewness);
        Assert.Equal(double.NaN, a.Kurtosis);
    }

    // Merging two stats fitted with the first and second halves of the same data must match
    // a stat fitted with all of the data at once.
    [Fact]
    public void Merge_PartsEqualsAll()
    {
        const int n = 2000;
        var rng = new Random();

        RunningStatistics.Moments a = new(); 
        RunningStatistics.Moments b = new(); 
        RunningStatistics.Moments c = new();

        double v;
        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            a.Fit(v);
            c.Fit(v);
        }

        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            b.Fit(v);
            c.Fit(v);
        }

        a.Merge(b);

        Assert.Equal(a.Mean, c.Mean, 2);
        Assert.Equal(a.Variance, c.Variance, 2);
        Assert.Equal(a.Skewness, c.Skewness, 2);
        Assert.Equal(a.Kurtosis, c.Kurtosis, 2);
    }

    // The static Type.Merge(a, b) returns a new stat; a and b keep their own state.
    [Fact]
    public void StaticMerge_DoesNotAffectOriginals()
    {
        RunningStatistics.Moments a = new();
        RunningStatistics.Moments b = new();
        var rng = new Random();
        
        for (var i = 0; i < 100; i++)
        {
            a.Fit(rng.NextDouble());
            b.Fit(rng.NextDouble());
        }

        var c = RunningStatistics.Moments.Merge(a, b);
        Assert.Equal(a.Nobs + b.Nobs, c.Nobs);
            
        c.Fit(rng.NextDouble());
        Assert.Equal(a.Nobs + b.Nobs + 1, c.Nobs);
    }

    // Fits 10M samples of the distribution and compares the statistic against its known
    // properties. SEEDED generator: an unseeded stream makes this fact flaky at the 0.01
    // tolerances (the excess-kurtosis estimator alone has a ~3 sigma spread on a fresh stream).
    [Fact]
    public void UnitUniformSample_MomentsMatchTheory()
    {
        const int n = 10_000_000;
        var rng = new Random(20251117);

        var moments = new RunningStatistics.Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(rng.NextDouble());
        }

        const double mean = (0.0 + 1.0 - double.Epsilon) / 2.0;
        var variance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;
        const double skewness = 0.0;
        const double excessKurtosis = -6.0 / 5.0;

        Utils.AssertIsApproximate(mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(variance, moments.Variance, 0.01);
        Utils.AssertIsApproximate(skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.01);
    }

    // Fits 10M samples of the distribution and compares the statistic against its known properties.
    [Theory]
    [InlineData(2.0, 1.0)]
    [InlineData(2.0, 10.0)]
    public void NormalSample_MomentsMatchTheory(double mean, double stdDev)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, stdDev);

        var moments = new RunningStatistics.Moments();

        var rng = new Random(20251115);
        for (var i = 0; i < n; i += 2)
        {
            var u = rng.NextDouble();
            var v = rng.NextDouble();
            if (u < 1e-300) u = 1e-300;
            var r = Math.Sqrt(-2.0 * Math.Log(u));
            moments.Fit(mean + stdDev * r * Math.Cos(2.0 * Math.PI * v));
            moments.Fit(mean + stdDev * r * Math.Sin(2.0 * Math.PI * v));
        }

        const double excessKurtosis = 0.0;

        Utils.AssertIsApproximate(dist.Mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(dist.StdDev, moments.StandardDeviation, 0.01);
        Utils.AssertIsApproximate(dist.Skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.01);
    }

    // Fits 10M samples of the distribution and compares the statistic against its known properties.
    [Fact]
    public void LogNormalSample_MomentsMatchTheory()
    {
        const int n = 10_000_000;
        const double mu = 0.0;
        const double sd = 0.5;
        const double s2 = sd * sd;
        var dist = new LogNormal(mu, sd);

        var moments = new RunningStatistics.Moments();

        var rng = new Random(20251116);
        for (var i = 0; i < n; i += 2)
        {
            var u = rng.NextDouble();
            var v = rng.NextDouble();
            if (u < 1e-300) u = 1e-300;
            var r = Math.Sqrt(-2.0 * Math.Log(u));
            moments.Fit(Math.Exp(mu + sd * r * Math.Cos(2.0 * Math.PI * v)));
            moments.Fit(Math.Exp(mu + sd * r * Math.Sin(2.0 * Math.PI * v)));
        }
            
        var excessKurtosis = Math.Exp(4.0 * s2) + 2.0 * Math.Exp(3.0 * s2) + 3.0 * Math.Exp(2.0 * s2) - 6;

        Utils.AssertIsApproximate(dist.Mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(dist.Variance, moments.Variance, 0.01);
        Utils.AssertIsApproximate(dist.Skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.1);
    }

    // PERMANENT merge-algebra gates (keep). Merge associativity: (a∪b)∪c and a∪(b∪c) built
    // with fresh Clone() (CloneEmpty + Merge) receivers over three seeded KappaStream chunks
    // (n=100, spread 1, κ = |mean|/spread = 10^0..10^8). Every getter of every grouping must
    // match the two-pass PropertyHelpers reference on a∪b∪c, and the groupings must agree
    // with each other, within: relTol = 1e-8 + 1e4·κ·ε on Mean/Variance/StandardDeviation;
    // absTol = 1e-12 + 1e3·κ·ε on Skewness/Kurtosis/ExcessKurtosis (the 1e-12 floor widens
    // the planned floorless 1e3·κ·ε at low κ: at κ=1 that margin gave only ~45x headroom
    // over the observed 4.9E-15, the floor restores ~250x). Probe worst observed (merge
    // errors scale ~linearly in κ, ≈1-3·κ·ε — not κ²): at κ=1e8 Variance rel 2.31E-8, StdDev
    // rel 1.15E-8, Skewness abs 6.31E-8, Kurtosis/Excess abs 4.69E-8 → headroom ~350-470x
    // (abs) and ~1e4x (rel); cross-grouping differences are far smaller.
    [Fact]
    public void Gate_MergeAssociativityMoments()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;
            var absTol = 1e-12 + 1e3 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 20251118 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 40251121 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 70251123 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Moments();
            var b = new RunningStatistics.Moments();
            var c = new RunningStatistics.Moments();
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

            AssertMomentsAgainstReference("(a∪b)∪c", reference, g1, relTol, absTol);
            AssertMomentsAgainstReference("a∪(b∪c)", reference, g2, relTol, absTol);
            AssertMomentsGroupingsAgree("(a∪b)∪c vs a∪(b∪c)", g1, g2, relTol, absTol);
        }
    }

    // Same merge-algebra contract, commutative direction: a∪b∪c vs c∪b∪a over the same
    // three seeded κ-chunks, fresh Clone() receivers, same κ range (10^0..10^8) and margins
    // relTol = 1e-8 + 1e4·κ·ε / absTol = 1e-12 + 1e3·κ·ε as Gate_MergeAssociativityMoments.
    [Fact]
    public void Gate_MergeCommutativityMoments()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;
            var absTol = 1e-12 + 1e3 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 90251128 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 13251217 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 56251219 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Moments();
            var b = new RunningStatistics.Moments();
            var c = new RunningStatistics.Moments();
            for (var i = 0; i < n; i++) a.Fit(chunkA[i]);
            for (var i = 0; i < n; i++) b.Fit(chunkB[i]);
            for (var i = 0; i < n; i++) c.Fit(chunkC[i]);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var g3 = c.Clone();
            g3.Merge(b);
            g3.Merge(a);

            AssertMomentsAgainstReference("a∪b∪c", reference, g1, relTol, absTol);
            AssertMomentsAgainstReference("c∪b∪a", reference, g3, relTol, absTol);
            AssertMomentsGroupingsAgree("a∪b∪c vs c∪b∪a", g1, g3, relTol, absTol);
        }
    }

    private static void AssertMomentsAgainstReference(string tag, MomentsReference reference, RunningStatistics.Moments merged, double relTol, double absTol)
    {
        Assert.True(PropertyHelpers.Within(reference.Mean, merged.Mean, relTol), PropertyHelpers.Describe($"{tag} Mean vs reference", reference.Mean, merged.Mean));
        Assert.True(PropertyHelpers.Within(reference.Variance, merged.Variance, relTol), PropertyHelpers.Describe($"{tag} Variance vs reference", reference.Variance, merged.Variance));
        Assert.True(PropertyHelpers.Within(reference.StandardDeviation, merged.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation vs reference", reference.StandardDeviation, merged.StandardDeviation));
        Assert.True(PropertyHelpers.WithinAbsolute(reference.Skewness, merged.Skewness, absTol), PropertyHelpers.Describe($"{tag} Skewness vs reference", reference.Skewness, merged.Skewness));
        Assert.True(PropertyHelpers.WithinAbsolute(reference.Kurtosis, merged.Kurtosis, absTol), PropertyHelpers.Describe($"{tag} Kurtosis vs reference", reference.Kurtosis, merged.Kurtosis));
        Assert.True(PropertyHelpers.WithinAbsolute(reference.ExcessKurtosis, merged.ExcessKurtosis, absTol), PropertyHelpers.Describe($"{tag} ExcessKurtosis vs reference", reference.ExcessKurtosis, merged.ExcessKurtosis));
    }

    private static void AssertMomentsGroupingsAgree(string tag, RunningStatistics.Moments left, RunningStatistics.Moments right, double relTol, double absTol)
    {
        Assert.True(PropertyHelpers.Within(left.Mean, right.Mean, relTol), PropertyHelpers.Describe($"{tag} Mean", left.Mean, right.Mean));
        Assert.True(PropertyHelpers.Within(left.Variance, right.Variance, relTol), PropertyHelpers.Describe($"{tag} Variance", left.Variance, right.Variance));
        Assert.True(PropertyHelpers.Within(left.StandardDeviation, right.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation", left.StandardDeviation, right.StandardDeviation));
        Assert.True(PropertyHelpers.WithinAbsolute(left.Skewness, right.Skewness, absTol), PropertyHelpers.Describe($"{tag} Skewness", left.Skewness, right.Skewness));
        Assert.True(PropertyHelpers.WithinAbsolute(left.Kurtosis, right.Kurtosis, absTol), PropertyHelpers.Describe($"{tag} Kurtosis", left.Kurtosis, right.Kurtosis));
        Assert.True(PropertyHelpers.WithinAbsolute(left.ExcessKurtosis, right.ExcessKurtosis, absTol), PropertyHelpers.Describe($"{tag} ExcessKurtosis", left.ExcessKurtosis, right.ExcessKurtosis));
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}