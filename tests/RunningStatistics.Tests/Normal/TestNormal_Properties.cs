using Xunit;

namespace RunningStatistics.Tests.Normal;

public partial class TestNormal
{
    [Fact]
    public void Reset_PropertiesAreCorrect()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0);
        normal.Fit(2.0);
        
        // pre-conditions
        Assert.Equal(2, normal.Nobs);
        Assert.Equal(1.5, normal.Mean);
        Assert.Equal(0.5, normal.Variance);
        
        normal.Reset();
        
        // post-conditions
        Assert.Equal(0, normal.Nobs);
        Assert.Equal(double.NaN, normal.Mean);
        Assert.Equal(double.NaN, normal.Variance);
    }
    
    [Fact]
    public void EmptyNormal_PropertiesAreCorrect()
    {
        var normal = new RunningStatistics.Normal();
        Assert.Equal(0, normal.Nobs);
        Assert.Equal(double.NaN, normal.Mean);
        Assert.Equal(double.NaN, normal.Variance);
    }
    
    [Fact]
    public void FitSingleObservation_PropertiesAreCorrect()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0);
        
        Assert.Equal(1, normal.Nobs);
        Assert.Equal(1.0, normal.Mean);
        Assert.Equal(0.0, normal.Variance);
    }
    
    [Fact]
    public void FitMultipleObservations_PropertiesAreCorrect()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit([1.0, 2.0, 3.0]);
        
        Assert.Equal(3, normal.Nobs);
        Assert.Equal(2.0, normal.Mean);
        Assert.Equal(1.0, normal.Variance, 10);
    }
}