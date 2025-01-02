using Xunit;

namespace RunningStatistics.Tests.Beta;

public partial class TestUncheckedBeta
{
    // The UncheckedBeta statistic makes no guarantees about statistical properties when
    // there are fewer than two observations of each type.
    
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