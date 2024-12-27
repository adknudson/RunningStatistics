using System;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests;

public class TestEmpiricalCdf
{
    [Fact]
    public void ConstructorThrowsOnInvalidBins()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmpiricalCdf(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmpiricalCdf(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmpiricalCdf(1));
    }
    
    [Fact]
    public void FitThrowsOnNaN()
    {
        EmpiricalCdf o = new(20);
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Fit(double.NaN));
    }
    
    [Fact]
    public void FitThrowsOnInfinity()
    {
        EmpiricalCdf o = new(20);
        Assert.Throws<ArgumentOutOfRangeException>(() => o.Fit(double.PositiveInfinity));
    }
    
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
    public void ResetWorksAsExpected()
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
    public void CdfEdgeCases()
    {
        var ecdf = new EmpiricalCdf(20);
        var rng = new Random();
        
        for (var _ = 0; _ < 1000; _++)
        {
            ecdf.Fit(rng.NextDouble());
        }
        
        Assert.Equal(0, ecdf.Cdf(ecdf.Min - 1));
        Assert.Equal(0, ecdf.Cdf(ecdf.Min));
        Assert.Equal(1, ecdf.Cdf(ecdf.Max));
        Assert.Equal(1, ecdf.Cdf(ecdf.Max + 1));
    }
    
    [Fact]
    public void QuantileEdgeCases()
    {
        var ecdf = new EmpiricalCdf(20);
        var rng = new Random();
        
        for (var _ = 0; _ < 1000; _++)
        {
            ecdf.Fit(rng.NextDouble());
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => ecdf.Quantile(-0.1));
        Assert.Equal(ecdf.Min, ecdf.Quantile(0));
        Assert.Equal(ecdf.Max, ecdf.Quantile(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ecdf.Quantile(1.1));
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

        var p = 0.0;
        while (p <= 1.0)
        {
            Utils.AssertIsApproximate(dist.CumulativeDistribution(p), o.Cdf(p), 0.005);
            Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(p), o.Quantile(p), 0.005);
            p += 0.001;
        }
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
        
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), o.Quantile(0.01), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.10), o.Quantile(0.10), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.25), o.Quantile(0.25), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), o.Quantile(0.50), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.75), o.Quantile(0.75), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.90), o.Quantile(0.90), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), o.Quantile(0.99), 0.25);
        
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-2), o.Cdf(-2), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-1), o.Cdf(-1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 0), o.Cdf( 0), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 1), o.Cdf( 1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 2), o.Cdf( 2), 0.25);
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

        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), o.Quantile(0.01), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.10), o.Quantile(0.10), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.25), o.Quantile(0.25), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), o.Quantile(0.50), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.75), o.Quantile(0.75), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.90), o.Quantile(0.90), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), o.Quantile(0.99), 0.25);
            
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.7), o.Cdf(0.7), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.8), o.Cdf(0.8), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.9), o.Cdf(0.9), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.0), o.Cdf(1.0), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.1), o.Cdf(1.1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.2), o.Cdf(1.2), 0.25);
    }
}