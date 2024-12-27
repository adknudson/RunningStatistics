using System;
using Xunit;

namespace RunningStatistics.Tests;

public class TestBeta
{
    [Fact]
    public void ParametersMustBeNonNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Beta(10, -1));

        var beta = new Beta();
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Fit(10, -1));
    }
    
    [Fact]
    public void EmptyBetaReturnsNaN()
    {
        Beta beta = new();
        Assert.Equal(double.NaN, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(double.NaN, beta.Variance);
    }

    [Fact]
    public void MergeEmptyReturnsNaN()
    {
        Beta a = new(), b = new();
        a.Merge(b);
        
        Assert.Equal(double.NaN, a.Mean);
        Assert.Equal(double.NaN, a.Median);
        Assert.Equal(double.NaN, a.Mode);
        Assert.Equal(double.NaN, a.Variance);
    }

    [Fact]
    public void PdfWorks()
    {
        Beta beta = new(100, 50);
        var mBeta = new MathNet.Numerics.Distributions.Beta(100, 50);

        Assert.Equal(0.0, beta.Pdf(-0.5));
        
        Assert.Equal(mBeta.Density(0.00), beta.Pdf(0.00));
        Assert.Equal(mBeta.Density(0.25), beta.Pdf(0.25));
        Assert.Equal(mBeta.Density(0.50), beta.Pdf(0.50));
        Assert.Equal(mBeta.Density(0.75), beta.Pdf(0.75));
        Assert.Equal(mBeta.Density(1.00), beta.Pdf(1.00));
        
        Assert.Equal(0.0, beta.Pdf(1.5));
    }
    
    [Fact]
    public void CdfWorks()
    {
        Beta beta = new(100, 50);
        var mBeta = new MathNet.Numerics.Distributions.Beta(100, 50);

        Assert.Equal(0.0, beta.Cdf(-0.5));
        
        Assert.Equal(mBeta.CumulativeDistribution(0.00), beta.Cdf(0.00));
        Assert.Equal(mBeta.CumulativeDistribution(0.25), beta.Cdf(0.25));
        Assert.Equal(mBeta.CumulativeDistribution(0.50), beta.Cdf(0.50));
        Assert.Equal(mBeta.CumulativeDistribution(0.75), beta.Cdf(0.75));
        Assert.Equal(mBeta.CumulativeDistribution(1.00), beta.Cdf(1.00));
        
        Assert.Equal(1.0, beta.Cdf(1.5));
    }

    [Fact]
    public void QuantileWorks()
    {
        Beta beta = new(100, 50);
        var mBeta = new MathNet.Numerics.Distributions.Beta(100, 50);

        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(-0.5));
        
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.00), beta.Quantile(0.00));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.25), beta.Quantile(0.25));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.50), beta.Quantile(0.50));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.75), beta.Quantile(0.75));
        Assert.Equal(mBeta.InverseCumulativeDistribution(1.00), beta.Quantile(1.00));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(1.5));
    }
}