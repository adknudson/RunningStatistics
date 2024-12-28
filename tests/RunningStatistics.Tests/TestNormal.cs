﻿using Xunit;

namespace RunningStatistics.Tests;

public class TestNormal
{
    [Fact]
    public void ShouldReturnNaN_WhenNoData()
    {
        var x = new Normal();
        Assert.Equal(double.NaN, x.Mean);
        Assert.Equal(double.NaN, x.Variance);
        Assert.Equal(double.NaN, x.StandardDeviation);
    }
    
    [Fact]
    public void ShouldHaveZeroObservations_WhenNoData()
    {
        var x = new Normal();
        Assert.Equal(0, x.Nobs);
    }
    
    [Fact]
    public void SingleFiniteObservation()
    {
        var x = new Normal();
        x.Fit(10);
        Assert.Equal(1, x.Nobs);
        Assert.Equal(10, x.Mean);
        Assert.Equal(0, x.Variance);
        Assert.Equal(0, x.StandardDeviation);
    }
    
    [Fact]
    public void SingleInfiniteObservation()
    {
        var x = new Normal();
        x.Fit(double.PositiveInfinity);
        Assert.Equal(1, x.Nobs);
        Assert.Equal(double.PositiveInfinity, x.Mean);
        Assert.Equal(double.NaN, x.Variance);
        Assert.Equal(double.NaN, x.StandardDeviation);
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(0, 5)]
    [InlineData(0, 10)]
    public void TestPdf(double mean, double std)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, std);
        
        var norm = new Normal();
        
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
    public void TestCdf(double mean, double std)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, std);
        
        var norm = new Normal();
        
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
    public void TestQuantile(double mean, double std, double tol)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, std);
        
        var norm = new Normal();
        
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
}