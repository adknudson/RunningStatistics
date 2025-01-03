using System;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests.EmpiricalCdf;

public partial class TestEmpiricalCdf
{
    [Fact]
    public void ConstructorThrowsOnInvalidBins()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.EmpiricalCdf(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.EmpiricalCdf(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.EmpiricalCdf(1));
    }
    
    [Fact]
    public void FitThrowsOnNaN()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Assert.Throws<ArgumentException>(() => cdf.Fit(double.NaN));
    }
    
    [Fact]
    public void FitThrowsOnInfinity()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Assert.Throws<ArgumentException>(() => cdf.Fit(double.PositiveInfinity));
    }
    
    [Fact]
    public void Quantile_FewerObservationsThanNumberOfBinsThrows()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        var rng = new Random();

        for (var i = 0; i < 10; i++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(0.25));
        Assert.Throws<InvalidOperationException>(() => cdf.Median);
    }

    [Fact]
    public void Quantile_NumObservationsEqualToNumberOfBins()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        var rng = new Random();
        
        for (var i = 0; i < 20; i++)
        {
            cdf.Fit(rng.NextDouble());
        }
        
        Assert.Equal(cdf.Min, cdf.Quantile(0));
        Assert.Equal(cdf.Max, cdf.Quantile(1));
        
        // This shouldn't throw an exception
        cdf.Quantile(0.25);
    }

    [Fact]
    public void MergeDifferentBuffersThrowsException()
    {
        RunningStatistics.EmpiricalCdf a = new(10);
        RunningStatistics.EmpiricalCdf b = new(20);
        Assert.Throws<Exception>(() => a.Merge(b));
    }

    [Fact]
    public void ResetWorksAsExpected()
    {
        RunningStatistics.EmpiricalCdf cdf = new(10);
        Random rng = new();
            
        for (var i = 0; i < 1000; i++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Equal(1000, cdf.Nobs);
        cdf.Reset();
        Assert.Equal(0, cdf.Nobs);
    }

    [Fact]
    public void MergingWhereOneIsEmptyEqualsNonEmptyInstance()
    {
        RunningStatistics.EmpiricalCdf a = new(20), b = new(20);
        Random rng = new();

        for (var i = 0; i < 1000; i++)
        {
            b.Fit(rng.NextDouble());
        }

        var aMergeB = RunningStatistics.EmpiricalCdf.Merge(a, b);
        var bMergeA = RunningStatistics.EmpiricalCdf.Merge(b, a);
            
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
        Random rng = new();

        RunningStatistics.EmpiricalCdf a = new();
        RunningStatistics.EmpiricalCdf b = new();
        RunningStatistics.EmpiricalCdf c = new();

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
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Random rng = new();
        
        for (var _ = 0; _ < 1000; _++)
        {
            cdf.Fit(rng.NextDouble());
        }
        
        Assert.Equal(0, cdf.Cdf(cdf.Min - 1));
        Assert.Equal(0, cdf.Cdf(cdf.Min));
        Assert.Equal(1, cdf.Cdf(cdf.Max));
        Assert.Equal(1, cdf.Cdf(cdf.Max + 1));
    }
    
    [Fact]
    public void QuantileEdgeCases()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Random rng = new();
        
        for (var _ = 0; _ < 1000; _++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => cdf.Quantile(-0.1));
        Assert.Equal(cdf.Min, cdf.Quantile(0));
        Assert.Equal(cdf.Max, cdf.Quantile(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => cdf.Quantile(1.1));
    }

    [Fact]
    public void TestUnitUniform()
    {
        const int n = 10_000_000;

        var dist = new ContinuousUniform();
        RunningStatistics.EmpiricalCdf cdf = new();

        for (var i = 0; i < n; i++)
        {
            cdf.Fit(dist.Sample());
        }

        var p = 0.0;
        while (p <= 1.0)
        {
            Utils.AssertIsApproximate(dist.CumulativeDistribution(p), cdf.Cdf(p), 0.005);
            Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(p), cdf.Quantile(p), 0.005);
            p += 0.001;
        }
    }

    [Fact]
    public void TestStandardNormal()
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(0, 1);

        RunningStatistics.EmpiricalCdf cdf = new();

        for (var i = 0; i < n; i++)
        {
            cdf.Fit(dist.Sample());
        }
        
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), cdf.Quantile(0.01), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.10), cdf.Quantile(0.10), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.25), cdf.Quantile(0.25), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), cdf.Quantile(0.50), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.75), cdf.Quantile(0.75), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.90), cdf.Quantile(0.90), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), cdf.Quantile(0.99), 0.25);
        
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-2), cdf.Cdf(-2), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-1), cdf.Cdf(-1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 0), cdf.Cdf( 0), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 1), cdf.Cdf( 1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 2), cdf.Cdf( 2), 0.25);
    }

    [Fact]
    public void TestLogNormal()
    {
        const int n = 10_000_000;
        var dist = new LogNormal(0.0, 0.1);

        RunningStatistics.EmpiricalCdf cdf = new();

        for (var i = 0; i < n; i++)
        {
            cdf.Fit(dist.Sample());
        }

        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), cdf.Quantile(0.01), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.10), cdf.Quantile(0.10), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.25), cdf.Quantile(0.25), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), cdf.Quantile(0.50), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.75), cdf.Quantile(0.75), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.90), cdf.Quantile(0.90), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), cdf.Quantile(0.99), 0.25);
            
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.7), cdf.Cdf(0.7), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.8), cdf.Cdf(0.8), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.9), cdf.Cdf(0.9), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.0), cdf.Cdf(1.0), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.1), cdf.Cdf(1.1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.2), cdf.Cdf(1.2), 0.25);
    }
}