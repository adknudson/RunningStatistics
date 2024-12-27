using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests;

public class TestBeta
{
    [Fact]
    public void ConstructorInitializesParametersCorrectly()
    {
        Beta beta = new(5, 10);
        Assert.Equal(5, beta.NumSuccesses);
        Assert.Equal(10, beta.NumFailures);
    }
    
    [Fact]
    public void PropertiesReturnExpectedValuesAfterFit()
    {
        Beta beta = new();
        beta.Fit(5, 10);
        Assert.Equal(5, beta.NumSuccesses);
        Assert.Equal(10, beta.NumFailures);
    }
    
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
    public void FitMultipleValues()
    {
        Beta beta = new();
        var values = new List<bool> { true, false, true, true, false };
        beta.Fit(values);
        Assert.Equal(3, beta.NumSuccesses);
        Assert.Equal(2, beta.NumFailures);
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
    public void MergeNonEmptyDistributions()
    {
        Beta a = new(5, 10);
        Beta b = new(10, 5);
        a.Merge(b);
        
        Assert.Equal(15, a.NumSuccesses);
        Assert.Equal(15, a.NumFailures);
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
    public void PdfEdgeCases()
    {
        Beta beta = new(100, 50);
        
        // Values less than 0
        Assert.Equal(0.0, beta.Pdf(-0.5));
        Assert.Equal(0.0, beta.Pdf(-0.0000001));

        // Values greater than 1
        Assert.Equal(0.0, beta.Pdf(1.5));
        Assert.Equal(0.0, beta.Pdf(1.0000001));

        // Values very close to 0
        Assert.Equal(0.0, beta.Pdf(0.0000001), 10);

        // Values very close to 1
        Assert.Equal(0.0, beta.Pdf(0.9999999), 10);

        // Exact 0
        Assert.Equal(0.0, beta.Pdf(0.0));

        // Exact 1
        Assert.Equal(0.0, beta.Pdf(1.0));
    }
    
    [Fact]
    public void PdfHandlesSpecialCases()
    {
        // Case when _a == 0 and _b == 0
        var beta = new Beta(0, 0);
        Assert.Equal(double.PositiveInfinity, beta.Pdf(0));
        Assert.Equal(double.PositiveInfinity, beta.Pdf(1));
        Assert.Equal(0.0, beta.Pdf(0.5));

        // Case when _a == 0
        beta = new Beta(0, 10);
        Assert.Equal(double.PositiveInfinity, beta.Pdf(0));
        Assert.Equal(0.0, beta.Pdf(0.5));
        Assert.Equal(0.0, beta.Pdf(1));

        // Case when _b == 0
        beta = new Beta(10, 0);
        Assert.Equal(0.0, beta.Pdf(0));
        Assert.Equal(0.0, beta.Pdf(0.5));
        Assert.Equal(double.PositiveInfinity, beta.Pdf(1));

        // Case when _a == 1 and _b == 1
        beta = new Beta(1, 1);
        Assert.Equal(1.0, beta.Pdf(0));
        Assert.Equal(1.0, beta.Pdf(0.5));
        Assert.Equal(1.0, beta.Pdf(1));
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
    public void CdfEdgeCases()
    {
        Beta beta = new(100, 50);

        // Values less than 0
        Assert.Equal(0.0, beta.Cdf(-0.5));
        Assert.Equal(0.0, beta.Cdf(-0.0000001));

        // Values greater than 1
        Assert.Equal(1.0, beta.Cdf(1.5));
        Assert.Equal(1.0, beta.Cdf(1.0000001));

        // Values very close to 0
        Assert.Equal(0.0, beta.Cdf(0.0000001), 10);

        // Values very close to 1
        Assert.Equal(1.0, beta.Cdf(0.9999999), 10);

        // Exact 0
        Assert.Equal(0.0, beta.Cdf(0.0));

        // Exact 1
        Assert.Equal(1.0, beta.Cdf(1.0));
    }

    [Fact]
    public void CdfHandlesSpecialCases()
    {
        // Case when _a == 0 and _b == 0
        var beta = new Beta(0, 0);
        Assert.Throws<ArgumentException>(() => beta.Cdf(0));
        Assert.Throws<ArgumentException>(() => beta.Cdf(1));
        Assert.Throws<ArgumentException>(() => beta.Cdf(0.5));

        // Case when _a == 0
        beta = new Beta(0, 10);
        Assert.Equal(0.0, beta.Cdf(-0.0000001));
        Assert.Equal(1.0, beta.Cdf(0));
        Assert.Equal(1.0, beta.Cdf(0.5));
        Assert.Equal(1.0, beta.Cdf(1));

        // Case when _b == 0
        beta = new Beta(10, 0);
        Assert.Equal(0.0, beta.Cdf(0));
        Assert.Equal(0.0, beta.Cdf(0.5));
        Assert.Equal(0.0, beta.Cdf(0.9999999));
        Assert.Equal(1.0, beta.Cdf(1));

        // Case when _a == 1 and _b == 1, CDF is equal to x
        beta = new Beta(1, 1);
        Random random = new();
        for (var i = 0; i < 100; i++)
        {
            var x = random.NextDouble();
            Assert.Equal(x, beta.Cdf(x));
        }
    }

    [Fact]
    public void QuantileWorks()
    {
        var beta = new Beta(100, 50);
        var mBeta = new MathNet.Numerics.Distributions.Beta(100, 50);
        
        // Normal range
        Assert.Equal(0, beta.Quantile(0));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.00), beta.Quantile(0.00));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.25), beta.Quantile(0.25));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.50), beta.Quantile(0.50));
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.75), beta.Quantile(0.75));
        Assert.Equal(mBeta.InverseCumulativeDistribution(1.00), beta.Quantile(1.00));
        Assert.Equal(1, beta.Quantile(1));
        
        // Almost 0
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.0000001), beta.Quantile(0.0000001));
        
        // Almost 1
        Assert.Equal(mBeta.InverseCumulativeDistribution(0.9999999), beta.Quantile(0.9999999));
    }
    
    [Fact]
    public void QuantileEdgeCases()
    {
        var beta = new Beta(100, 50);

        // Values less than 0
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(-0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(-0.0000001));

        // Values greater than 1
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(1.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(1.0000001));
    }
}