using Xunit;

namespace RunningStatistics.Tests;

public class TestMean
{
    [Fact]
    public void EmptyMeanReturnsNaN()
    {
        Mean m = new();
        Assert.Equal(0, m.Nobs);
        Assert.Equal(double.NaN, m.Value);
    }

    [Fact]
    public void MergingMeans()
    {
        Mean a = new(), b = new();
        a.Fit(10);
        b.Fit(20);
        var c = Mean.Merge(a, b);
        a.Merge(b);
        
        Assert.Equal(2, a.Nobs);
        Assert.Equal(1, b.Nobs);
        Assert.Equal(2, c.Nobs);
        
        Assert.Equal(15, a.Value, 2);
        Assert.Equal(15, c.Value, 2);
        Assert.Equal(20, b.Value, 2);
    }

    [Fact]
    public void ResetMean()
    {
        Mean a = new();
        a.Fit(1);
        a.Fit(2);
        a.Fit(3);
        
        a.Reset();
        
        Assert.Equal(double.NaN, a.Value);
        Assert.Equal(0, a.Nobs);
    }
}