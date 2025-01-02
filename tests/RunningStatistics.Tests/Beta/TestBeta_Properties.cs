using Xunit;

namespace RunningStatistics.Tests.Beta;

public partial class TestBeta
{
    [Fact]
    public void EmptyBeta_StatisticalPropertiesAreNaN()
    {
        var beta = GenerateStatistic();
        
        Assert.Equal(double.NaN, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(double.NaN, beta.Variance);
    }

    [Fact]
    public void SingleTrueObservation_StatisticalPropertiesAreCorrect()
    {
        var beta = GenerateStatistic();
        beta.Fit(true);
        
        Assert.Equal(1, beta.Nobs);
        Assert.Equal(1, beta.Successes);
        Assert.Equal(0, beta.Failures);
        
        Assert.Equal(1.0, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(double.NaN, beta.Variance);
    }
    
    [Fact]
    public void SingleFalseObservation_StatisticalPropertiesAreCorrect()
    {
        var beta = GenerateStatistic();
        beta.Fit(false);
        
        Assert.Equal(1, beta.Nobs);
        Assert.Equal(0, beta.Successes);
        Assert.Equal(1, beta.Failures);
        
        Assert.Equal(0.0, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(double.NaN, beta.Variance);
    }

    [Fact]
    public void SeveralTrueObservations_StatisticalPropertiesAreCorrect()
    {
        var beta = GenerateStatistic();
        beta.Fit(true, 3);
        
        Assert.Equal(3, beta.Nobs);
        Assert.Equal(3, beta.Successes);
        Assert.Equal(0, beta.Failures);
        
        Assert.Equal(1.0, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(double.NaN, beta.Variance);
    }
    
    [Fact]
    public void SeveralFalseObservations_StatisticalPropertiesAreCorrect()
    {
        var beta = GenerateStatistic();
        beta.Fit(false, 3);
        
        Assert.Equal(3, beta.Nobs);
        Assert.Equal(0, beta.Successes);
        Assert.Equal(3, beta.Failures);
        
        Assert.Equal(0.0, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(double.NaN, beta.Variance);
    }
    
    [Fact]
    public void OneTrueOneFalseObservation_StatisticalPropertiesAreCorrect()
    {
        var beta = GenerateStatistic();
        beta.Fit(true);
        beta.Fit(false);
        
        Assert.Equal(2, beta.Nobs);
        Assert.Equal(1, beta.Successes);
        Assert.Equal(1, beta.Failures);
        
        Assert.Equal(0.5, beta.Mean);
        Assert.Equal(double.NaN, beta.Median);
        Assert.Equal(double.NaN, beta.Mode);
        Assert.Equal(0.08333333333333333, beta.Variance, 10);
    }
    
    [Fact]
    public void AtLeastTwoOfEachObservation_StatisticalPropertiesAreCorrect()
    {
        var beta = GenerateStatistic();
        beta.Fit(true, 20);
        beta.Fit(false, 50);
        
        Assert.Equal(70, beta.Nobs);
        Assert.Equal(20, beta.Successes);
        Assert.Equal(50, beta.Failures);
        
        Assert.Equal(0.2857142857142857, beta.Mean, 10);
        Assert.Equal(0.2836634994541317, beta.Median, 10);
        Assert.Equal(0.27941176470588236, beta.Mode, 10);
        Assert.Equal(0.002874389192296637, beta.Variance, 10);
    }
}