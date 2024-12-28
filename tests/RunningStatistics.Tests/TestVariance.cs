using Xunit;

namespace RunningStatistics.Tests;

public class TestVariance
{
    [Fact]
    public void EmptyVarianceReturnsNan()
    {
        Variance v = new();
        Assert.Equal(0, v.Nobs);
        Assert.Equal(double.NaN, v.Value);
        Assert.Equal(double.NaN, v.StandardDeviation);
    }

    [Fact]
    public void SingleFiniteObservation()
    {
        Variance v = new();
        v.Fit(10);
        Assert.Equal(1, v.Nobs);
        Assert.Equal(0, v.Value);
        Assert.Equal(0, v.StandardDeviation);
    }
    
    [Fact]
    public void SingleInfiniteObservation()
    {
        Variance v = new();
        v.Fit(double.PositiveInfinity);
        Assert.Equal(1, v.Nobs);
        Assert.Equal(double.NaN, v.Value);
        Assert.Equal(double.NaN, v.StandardDeviation);
    }

    [Fact]
    public void MergingVariances()
    {
        Variance a = new(), b = new();
        a.Fit(10);
        b.Fit(20);
        var c = Variance.Merge(a, b);
        a.Merge(b);
        
        Assert.Equal(2, a.Nobs);
        Assert.Equal(1, b.Nobs);
        Assert.Equal(2, c.Nobs);
        
        Assert.Equal(a.Value, c.Value, 2);
    }

    [Fact]
    public void ResetVariance()
    {
        Variance a = new();
        a.Fit(1);
        a.Fit(2);
        a.Fit(3);
        
        a.Reset();
        
        Assert.Equal(0, a.Nobs);
        Assert.Equal(double.NaN, a.Value);
    }
}