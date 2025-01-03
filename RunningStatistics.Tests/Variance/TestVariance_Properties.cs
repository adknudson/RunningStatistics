using System;
using Xunit;

namespace RunningStatistics.Tests.Variance;

public partial class TestVariance
{
    [Fact]
    public void Reset_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        
        // pre-conditions
        Assert.Equal(2, v.Nobs);
        Assert.Equal(0.5, v.Value);
        
        v.Reset();
        
        // post-conditions
        Assert.Equal(0, v.Nobs);
        Assert.Equal(double.NaN, v.Value);
    }

    [Fact]
    public void CloneEmpty_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        
        var copy = v.CloneEmpty();
        
        Assert.Equal(0, copy.Nobs);
        Assert.Equal(double.NaN, copy.Value);
    }
    
    [Fact]
    public void Clone_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        
        var copy = v.Clone();
        
        Assert.Equal(2, copy.Nobs);
        Assert.Equal(0.5, copy.Value);
    }
    
    [Fact]
    public void EmptyVariance_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        
        Assert.Equal(0, v.Nobs);
        Assert.Equal(double.NaN, v.Value);
        Assert.Equal(double.NaN, v.StandardDeviation);
    }
    
    [Fact]
    public void SingleObservation_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        
        Assert.Equal(1, v.Nobs);
        Assert.Equal(double.NaN, v.Value);
        Assert.Equal(double.NaN, v.StandardDeviation);
    }
    
    [Fact]
    public void TwoObservations_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        
        Assert.Equal(2, v.Nobs);
        Assert.Equal(0.5, v.Value);
        Assert.Equal(Math.Sqrt(0.5), v.StandardDeviation);
    }
    
    [Fact]
    public void MultipleObservations_PropertiesAreCorrect()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        v.Fit(3.0);
        v.Fit(4.0);
        
        Assert.Equal(4, v.Nobs);
        Assert.Equal(1.6666666666666667, v.Value, 10);
        Assert.Equal(Math.Sqrt(1.6666666666666667), v.StandardDeviation, 10);
    }
}