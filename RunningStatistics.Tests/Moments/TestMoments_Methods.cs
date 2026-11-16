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

    // Null iterable Fit contract — Moments has no iterable Fit override;
    // the call resolves to the base RunningStatisticBase.Fit(IEnumerable<double>).
    [Fact]
    public void Fit_RejectsNullIterable()
    {
        var moments = new RunningStatistics.Moments();
        Assert.Throws<ArgumentNullException>(() => moments.Fit((IEnumerable<double>?)null));
        Assert.Equal(0, moments.Nobs);
    }
}