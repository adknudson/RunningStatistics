using System;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests;

public class TestEmpiricalCdf
{
    [Fact]
    public void QuantileOfEmptyIsZero()
    {
        EmpiricalCdf o = new(20);
        var rng = new Random();

        for (var i = 0; i < 10; i++)
        {
            o.Fit(rng.NextDouble());
        }

        Assert.Equal(0.0, o.Quantile(0.5));
    }

    [Fact]
    public void MergeEmptyIsZero()
    {
        EmpiricalCdf a = new(20), b = new(20);
        a.Merge(b);

        Assert.Equal(0.0, a.Quantile(0.5));
    }

    [Fact]
    public void MergeDifferentBuffersThrowsError()
    {
        EmpiricalCdf a = new(10), b = new(20);
        Assert.Throws<Exception>(() => a.Merge(b));
    }

    [Fact]
    public void Reset()
    {
        EmpiricalCdf a = new(10);
        var rng = new Random();
            
        for (var i = 0; i < 1000; i++)
        {
            a.Fit(rng.NextDouble());
        }

        Assert.Equal(1000, a.Nobs);
        a.Reset();
        Assert.Equal(0, a.Nobs);
    }

    [Fact]
    public void MergingWhereOneIsEmptyEqualsNonEmptyInstance()
    {
        EmpiricalCdf a = new(20), b = new(20);
        var rng = new Random();

        for (var i = 0; i < 1000; i++)
        {
            b.Fit(rng.NextDouble());
        }

        var aMergeB = EmpiricalCdf.Merge(a, b);
        var bMergeA = EmpiricalCdf.Merge(b, a);
            
        Assert.Equal(b.Min, aMergeB.Min);
        Assert.Equal(b.Max, aMergeB.Max);
        Assert.Equal(b.Quantile(0.25), aMergeB.Quantile(0.25));
        Assert.Equal(b.Quantile(0.75), aMergeB.Quantile(0.75));

            
        Assert.Equal(b.Min, bMergeA.Min);
        Assert.Equal(b.Max, bMergeA.Max);
        Assert.Equal(b.Quantile(0.25), bMergeA.Quantile(0.25));
        Assert.Equal(b.Quantile(0.75), bMergeA.Quantile(0.75));
    }

    [Fact]
    public void MergePartsEqualsMergeAll()
    {
        const int n = 50_000;
        var rng = new Random();

        EmpiricalCdf a = new(), b = new(), c = new();
            
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

        Assert.Equal(a.Quantile(0.1), c.Quantile(0.1), 2);
        Assert.Equal(a.Median, c.Median, 2);
    }

    [Fact]
    public void TestUnitUniform()
    {
        const int n = 10_000_000;

        var dist = new ContinuousUniform();
        EmpiricalCdf o = new();

        for (var i = 0; i < n; i++)
        {
            o.Fit(dist.Sample());
        }

        Assert.Equal(dist.InverseCumulativeDistribution(0.00), o.Quantile(0.00), 2);
        Assert.Equal(dist.InverseCumulativeDistribution(0.25), o.Quantile(0.25), 2);
        Assert.Equal(dist.InverseCumulativeDistribution(0.50), o.Quantile(0.50), 2);
        Assert.Equal(dist.InverseCumulativeDistribution(0.75), o.Quantile(0.75), 2);
        Assert.Equal(dist.InverseCumulativeDistribution(1.00), o.Quantile(1.00), 2);
            
        Assert.Equal(dist.CumulativeDistribution(0.00), o.Cdf(0.00), 2);
        Assert.Equal(dist.CumulativeDistribution(0.25), o.Cdf(0.25), 2);
        Assert.Equal(dist.CumulativeDistribution(0.50), o.Cdf(0.50), 2);
        Assert.Equal(dist.CumulativeDistribution(0.75), o.Cdf(0.75), 2);
        Assert.Equal(dist.CumulativeDistribution(1.00), o.Cdf(1.00), 2);
    }

    [Fact]
    public void TestStandardNormal()
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(0, 1);

        EmpiricalCdf o = new();

        for (var i = 0; i < n; i++)
        {
            o.Fit(dist.Sample());
        }

        var relErr = Utils.RelError(-2.326347874040846, o.Quantile(0.01));
        Assert.Equal(0.0, relErr, 1);

        Assert.Equal(dist.InverseCumulativeDistribution(0.10), o.Quantile(0.10), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.25), o.Quantile(0.25), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.50), o.Quantile(0.50), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.75), o.Quantile(0.75), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.90), o.Quantile(0.90), 1);
        relErr = Utils.RelError(dist.InverseCumulativeDistribution(0.99), o.Quantile(0.99));
        Assert.Equal(0.0, relErr, 1);
            
        Assert.Equal(dist.CumulativeDistribution(-2), o.Cdf(-2), 1);
        Assert.Equal(dist.CumulativeDistribution(-1), o.Cdf(-1), 1);
        Assert.Equal(dist.CumulativeDistribution( 0), o.Cdf( 0), 1);
        Assert.Equal(dist.CumulativeDistribution( 1), o.Cdf( 1), 1);
        Assert.Equal(dist.CumulativeDistribution( 2), o.Cdf( 2), 1);
    }

    [Fact]
    public void TestLogNormal()
    {
        const int n = 10_000_000;
        var dist = new LogNormal(0.0, 0.1);

        EmpiricalCdf o = new();

        for (var i = 0; i < n; i++)
        {
            o.Fit(dist.Sample());
        }

        Assert.Equal(dist.InverseCumulativeDistribution(0.01), o.Quantile(0.01), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.10), o.Quantile(0.10), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.25), o.Quantile(0.25), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.50), o.Quantile(0.50), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.75), o.Quantile(0.75), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.90), o.Quantile(0.90), 1);
        Assert.Equal(dist.InverseCumulativeDistribution(0.99), o.Quantile(0.99), 1);
            
        Assert.Equal(dist.CumulativeDistribution(0.7), o.Cdf(0.7), 1);
        Assert.Equal(dist.CumulativeDistribution(0.8), o.Cdf(0.8), 1);
        Assert.Equal(dist.CumulativeDistribution(0.9), o.Cdf(0.9), 1);
        Assert.Equal(dist.CumulativeDistribution(1.0), o.Cdf(1.0), 1);
        Assert.Equal(dist.CumulativeDistribution(1.1), o.Cdf(1.1), 1);
        Assert.Equal(dist.CumulativeDistribution(1.2), o.Cdf(1.2), 1);
    }
}