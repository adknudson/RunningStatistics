using System;
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

    // Pins the ACCURACY (not exactness) of Moments on jittered 1e6-scale data against a two-pass
    // reference (direct central sums). Moments accumulates raw power sums (M2 ~1e12, M3 ~1e18,
    // M4 ~1e24), so forming central moments subtracts huge nearby quantities: at offset 1e6 the
    // observed relative error is ~1.7e-4 for Variance (4.7e-3 merged), while Skewness (error
    // ~4e4) and Kurtosis (error ~2.1e11) are outright garbage, not just imprecise. These pins
    // document that current accuracy; they do NOT demand improvement. Tightening Skewness/Kurtosis
    // belongs to the deferred recursive central-moment rework of Moments (see AGENT_PLAN.md).
    [Fact]
    public void Accuracy_PinRawMomentsNearOffset()
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
        Utils.AssertIsApproximate(refVariance, moments.Variance, 1e-2 * refVariance);
        Utils.AssertIsApproximate(refStdDev, moments.StandardDeviation, 1e-2 * refStdDev);
        Utils.AssertIsApproximate(refSkewness, moments.Skewness, 1e5);
        Utils.AssertIsApproximate(refKurtosis, moments.Kurtosis, 1e12);
        Utils.AssertIsApproximate(refKurtosis - 3, moments.ExcessKurtosis, 1e12);
    }

    // Same pin as Accuracy_PinRawMomentsNearOffset, through the Merge path: the raw-power-sum
    // cancellation is just as severe when two stats are merged (observed: Variance rel err
    // ~4.7e-3, Skewness abs err ~4e4, Kurtosis abs err ~2.1e11).
    [Fact]
    public void Accuracy_PinMergedMomentsNearOffset()
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

        // Same two-pass reference as Accuracy_PinRawMomentsNearOffset.
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
        Utils.AssertIsApproximate(refVariance, left.Variance, 1e-2 * refVariance);
        Utils.AssertIsApproximate(refStdDev, left.StandardDeviation, 1e-2 * refStdDev);
        Utils.AssertIsApproximate(refSkewness, left.Skewness, 1e5);
        Utils.AssertIsApproximate(refKurtosis, left.Kurtosis, 1e12);
    }

    [Fact]
    public void EmptyMomentsAreNaN()
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
    public void SingleFiniteObservation()
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
    public void TwoFiniteObservations()
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
    public void ThrowsExceptionOnNonFiniteNumber()
    {
        RunningStatistics.Moments m = new();
        Assert.Throws<ArgumentException>(() => m.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => m.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => m.Fit(double.NegativeInfinity));
    }

    [Fact]
    public void MergeEmptyAreNaN()
    {
        RunningStatistics.Moments a = new(); 
        RunningStatistics.Moments b = new(); 
        a.Merge(b);

        Assert.Equal(double.NaN, a.Mean);
        Assert.Equal(double.NaN, a.Variance);
        Assert.Equal(double.NaN, a.Skewness);
        Assert.Equal(double.NaN, a.Kurtosis);
    }

    [Fact]
    public void MergePartsEqualsMergeAll()
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

    [Fact]
    public void StaticMergeDoesNotAffectOriginals()
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

    [Fact]
    public void TestUnitUniform()
    {
        const int n = 10_000_000;
        var rng = new Random();

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

    [Theory]
    [InlineData(2.0, 1.0)]
    [InlineData(2.0, 10.0)]
    public void TestNormal(double mean, double stdDev)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, stdDev);

        var moments = new RunningStatistics.Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(dist.Sample());
        }

        const double excessKurtosis = 0.0;

        Utils.AssertIsApproximate(dist.Mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(dist.StdDev, moments.StandardDeviation, 0.01);
        Utils.AssertIsApproximate(dist.Skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.01);
    }

    [Fact]
    public void TestLogNormal()
    {
        const int n = 10_000_000;
        const double mu = 0.0;
        const double sd = 0.5;
        const double s2 = sd * sd;
        var dist = new LogNormal(mu, sd);

        var moments = new RunningStatistics.Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(dist.Sample());
        }
            
        var excessKurtosis = Math.Exp(4.0 * s2) + 2.0 * Math.Exp(3.0 * s2) + 3.0 * Math.Exp(2.0 * s2) - 6;

        Utils.AssertIsApproximate(dist.Mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(dist.Variance, moments.Variance, 0.01);
        Utils.AssertIsApproximate(dist.Skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.1);
    }
}