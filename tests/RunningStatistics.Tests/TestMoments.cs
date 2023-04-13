using System;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests;

public class TestMoments
{
    [Fact]
    public void EmptyMomentsAreNaN()
    {
        Moments moments = new();
        var (mean, variance, skewness, kurtosis) = moments;

        Assert.Equal(double.NaN, mean);
        Assert.Equal(double.NaN, variance);
        Assert.Equal(double.NaN, skewness);
        Assert.Equal(double.NaN, kurtosis);
    }

    [Fact]
    public void MergeEmptyAreNaN()
    {
        Moments a = new(); Moments b = new(); a.Merge(b);
        var (mean, variance, skewness, kurtosis) = a;

        Assert.Equal(double.NaN, mean);
        Assert.Equal(double.NaN, variance);
        Assert.Equal(double.NaN, skewness);
        Assert.Equal(double.NaN, kurtosis);
    }

    [Fact]
    public void MergePartsEqualsMergeAll()
    {
        const int n = 2000;
        var rng = new Random();

        Moments a = new(); Moments b = new(); Moments c = new();

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
        Moments a = new(), b = new();
        var rng = new Random();
        for (var i = 0; i < 100; i++)
        {
            a.Fit(rng.NextDouble());
            b.Fit(rng.NextDouble());
        }

        var c = Moments.Merge(a, b);
        Assert.Equal(a.Nobs + b.Nobs, c.Nobs);
            
        c.Fit(rng.NextDouble());
        Assert.Equal(a.Nobs + b.Nobs + 1, c.Nobs);
    }

    [Fact]
    public void TestUnitUniform()
    {
        const int n = 10_000_000;
        var rng = new Random();

        var moments = new Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(rng.NextDouble());
        }

        const double mean = (0.0 + 1.0 - double.Epsilon) / 2.0;
        var variance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;
        const double skewness = 0.0;
        const double excessKurtosis = -6.0 / 5.0;

        Assert.Equal(mean, moments.Mean, 2);
        Assert.Equal(variance, moments.Variance, 2);
        Assert.Equal(skewness, moments.Skewness, 2);
        Assert.Equal(excessKurtosis, moments.ExcessKurtosis, 2);
    }

    [Theory]
    [InlineData(2.0, 1.0)]
    [InlineData(2.0, 10.0)]
    public void TestNormal(double mean, double stdDev)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, stdDev);

        var moments = new Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(dist.Sample());
        }

        const double excessKurtosis = 0.0;

        Assert.Equal(0.0, Utils.RelError(dist.Mean, moments.Mean), 2);
        Assert.Equal(0.0, Utils.RelError(dist.StdDev, Math.Sqrt(moments.Variance)), 2);
        Assert.Equal(dist.Skewness, moments.Skewness, 2);
        Assert.Equal(excessKurtosis, moments.ExcessKurtosis, 2);
    }

    [Fact]
    public void TestLogNormal()
    {
        const int n = 100_000_000;
        const double mu = 0.0;
        const double sd = 0.5;
        const double s2 = sd * sd;
        var dist = new LogNormal(mu, sd);

        var moments = new Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(dist.Sample());
        }
            
        var excessKurtosis = Math.Exp(4.0 * s2) + 2.0 * Math.Exp(3.0 * s2) + 3.0 * Math.Exp(2.0 * s2) - 6;

        Assert.Equal(0.0, Utils.RelError(dist.Mean, moments.Mean), 2);
        Assert.Equal(0.0, Utils.RelError(dist.Variance, moments.Variance), 2);
        Assert.Equal(0.0, Utils.RelError(dist.Skewness, moments.Skewness), 2);
        Assert.Equal(0.0, Utils.RelError(excessKurtosis, moments.ExcessKurtosis), 2);
    }
}