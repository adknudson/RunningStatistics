using System;
using Xunit;

namespace RunningStatistics.Tests.Beta;

public partial class TestBeta
{
    [Fact]
    public void Pdf_ReturnsCorrectValues()
    {
        RunningStatistics.Beta beta = new(2, 5);

        Assert.Equal(0.0, beta.Pdf(-0.5));
        
        Assert.Equal(0.000000000, beta.Pdf(0.00), 10);
        Assert.Equal(2.373046875, beta.Pdf(0.25), 10);
        Assert.Equal(0.937500000, beta.Pdf(0.50), 10);
        Assert.Equal(0.087890625, beta.Pdf(0.75), 10);
        Assert.Equal(0.000000000, beta.Pdf(1.00), 10);
        
        Assert.Equal(0.0, beta.Pdf(1.5));
    }
    
    [Fact]
    public void Pdf_EdgeCases()
    {
        RunningStatistics.Beta beta = new(100, 50);
        
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
    public void Pdf_HandlesSpecialCases()
    {
        // Case when _a == 0 and _b == 0
        var beta = new RunningStatistics.Beta(0, 0);
        Assert.Equal(double.PositiveInfinity, beta.Pdf(0));
        Assert.Equal(double.PositiveInfinity, beta.Pdf(1));
        Assert.Equal(0.0, beta.Pdf(0.5));

        // Case when _a == 0
        beta = new RunningStatistics.Beta(0, 10);
        Assert.Equal(double.PositiveInfinity, beta.Pdf(0));
        Assert.Equal(0.0, beta.Pdf(0.5));
        Assert.Equal(0.0, beta.Pdf(1));

        // Case when _b == 0
        beta = new RunningStatistics.Beta(10, 0);
        Assert.Equal(0.0, beta.Pdf(0));
        Assert.Equal(0.0, beta.Pdf(0.5));
        Assert.Equal(double.PositiveInfinity, beta.Pdf(1));

        // Case when _a == 1 and _b == 1
        beta = new RunningStatistics.Beta(1, 1);
        Assert.Equal(1.0, beta.Pdf(0));
        Assert.Equal(1.0, beta.Pdf(0.5));
        Assert.Equal(1.0, beta.Pdf(1));
    }
    
    [Fact]
    public void Cdf_ReturnsCorrectValues()
    {
        RunningStatistics.Beta beta = new(2, 5);

        Assert.Equal(0.0, beta.Cdf(-0.5));
        
        Assert.Equal(0.000000000000, beta.Cdf(0.00), 10);
        Assert.Equal(0.466064453125, beta.Cdf(0.25), 10);
        Assert.Equal(0.890625000000, beta.Cdf(0.50), 10);
        Assert.Equal(0.995361328125, beta.Cdf(0.75), 10);
        Assert.Equal(1.000000000000, beta.Cdf(1.00), 10);
        
        Assert.Equal(1.0, beta.Cdf(1.5));
    }
    
    [Fact]
    public void Cdf_EdgeCases()
    {
        RunningStatistics.Beta beta = new(100, 50);

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
    public void Cdf_HandlesSpecialCases()
    {
        // Case when _a == 0 and _b == 0
        var beta = new RunningStatistics.Beta(0, 0);
        Assert.Throws<ArgumentException>(() => beta.Cdf(0));
        Assert.Throws<ArgumentException>(() => beta.Cdf(1));
        Assert.Throws<ArgumentException>(() => beta.Cdf(0.5));

        // Case when _a == 0
        beta = new RunningStatistics.Beta(0, 10);
        Assert.Equal(0.0, beta.Cdf(-0.0000001));
        Assert.Equal(1.0, beta.Cdf(0));
        Assert.Equal(1.0, beta.Cdf(0.5));
        Assert.Equal(1.0, beta.Cdf(1));

        // Case when _b == 0
        beta = new RunningStatistics.Beta(10, 0);
        Assert.Equal(0.0, beta.Cdf(0));
        Assert.Equal(0.0, beta.Cdf(0.5));
        Assert.Equal(0.0, beta.Cdf(0.9999999));
        Assert.Equal(1.0, beta.Cdf(1));

        // Case when _a == 1 and _b == 1, CDF is equal to x
        beta = new RunningStatistics.Beta(1, 1);
        Random random = new();
        for (var i = 0; i < 100; i++)
        {
            var x = random.NextDouble();
            Assert.Equal(x, beta.Cdf(x));
        }
    }

    [Fact]
    public void Quantile_ReturnsCorrectValues()
    {
        var beta = new RunningStatistics.Beta(2, 5);
        
        // Normal range
        Assert.Equal(0.0, beta.Quantile(0.00));
        Assert.Equal(0.16116291679032646, beta.Quantile(0.25), 10);
        Assert.Equal(0.2644499832956599, beta.Quantile(0.50), 10);
        Assert.Equal(0.3894794852007243, beta.Quantile(0.75), 10);
        Assert.Equal(1.0, beta.Quantile(1.00));
        
        // Almost 0
        Utils.AssertIsApproximate(8.165854858475914e-5, beta.Quantile(0.0000001), 1e-4);
        
        // Almost 1
        Utils.AssertIsApproximate(0.972047739259444, beta.Quantile(0.9999999), 1e-4);
    }
    
    [Fact]
    public void QuantileEdgeCases()
    {
        var beta = new RunningStatistics.Beta(100, 50);

        // Values less than 0
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(-0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(-0.0000001));

        // Values greater than 1
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(1.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => beta.Quantile(1.0000001));
    }
}