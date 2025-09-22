using Xunit;

namespace RunningStatistics.Tests.CountMap;

public partial class TestCountMap
{
    [Fact]
    public void Sum_ReturnsCorrectSum()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(11, countMap.Sum());
    }

    [Fact]
    public void Nobs_ReturnsCorrectNobs()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        
        Assert.Equal(5, countMap.Nobs(o => o < 3));
    }

    [Fact]
    public void Mean_ReturnsCorrectMean()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(1.833, countMap.Mean(), 3);
    }

    [Fact]
    public void Variance_ReturnsBiasCorrectedVariance()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.567, countMap.Variance(), 3);
    }

    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviation_Int()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.753, countMap.StandardDeviation(), 3);
    }
    
    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviation_Long()
    {
        var countMap = new CountMap<long>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.753, countMap.StandardDeviation(), 3);
    }
    
    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviation_Double()
    {
        var countMap = new CountMap<double>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.753, countMap.StandardDeviation(), 3);
    }
    
    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviationWithMean_Int()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        var mean = countMap.Mean();

        Assert.Equal(0.753, countMap.StandardDeviation(mean), 3);
    }
    
    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviationWithMean_Long()
    {
        var countMap = new CountMap<long>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        var mean = countMap.Mean();

        Assert.Equal(0.753, countMap.StandardDeviation(mean), 3);
    }
    
    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviationWithMean_Double()
    {
        var countMap = new CountMap<double>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        var mean = countMap.Mean();

        Assert.Equal(0.753, countMap.StandardDeviation(mean), 3);
    }

    [Fact]
    public void Skewness_ReturnsCorrectSkewness()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.228, countMap.Skewness(), 3);
    }

    [Fact]
    public void Kurtosis_ReturnsCorrectKurtosis()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(2.107, countMap.Kurtosis(), 3);
    }

    [Fact]
    public void ExcessKurtosis_ReturnsCorrectExcessKurtosis()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(-0.893, countMap.ExcessKurtosis(), 3);
    }
    
    [Fact]
    public void MinKey_ReturnsMinimumKeyForMyNum()
    {
        var countMap = new CountMap<MyNum>();
        countMap.Fit(new MyNum(1), 2);
        countMap.Fit(new MyNum(2), 3);
        countMap.Fit(new MyNum(3), 1);

        Assert.Equal(new MyNum(1), countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ReturnsMaximumKeyForMyNum()
    {
        var countMap = new CountMap<MyNum>();
        countMap.Fit(new MyNum(1), 2);
        countMap.Fit(new MyNum(2), 3);
        countMap.Fit(new MyNum(3), 1);

        Assert.Equal(new MyNum(3), countMap.MaxKey());
    }

    [Fact]
    public void Sum_ReturnsCorrectSumForMyNum()
    {
        var countMap = new CountMap<MyNum>();
        countMap.Fit(new MyNum(1), 2);
        countMap.Fit(new MyNum(2), 3);
        countMap.Fit(new MyNum(3), 1);

        Assert.Equal(new MyNum(11), countMap.Sum());
    }

    [Fact]
    public void Mean_ReturnsCorrectMeanForMyNum()
    {
        var countMap = new CountMap<MyNum>();
        countMap.Fit(new MyNum(1), 2);
        countMap.Fit(new MyNum(2), 3);
        countMap.Fit(new MyNum(3), 1);

        Assert.Equal(1.833m, countMap.Mean().Value, 3);
        Assert.IsType<MyNum>(countMap.Mean());
    }
    
    [Fact]
    public void Mode_ReturnsObservationWithHighestCount()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(2, countMap.Mode());
    }
    
    [Fact]
    public void Quantile_ReturnsCorrectValues_EvenNumberOfUniqueObs()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 2);
        countMap.Fit(3, 1);
        countMap.Fit(4, 2);
        
        Assert.Equal(1, countMap.Quantile(0.0));
        Assert.Equal(1, countMap.Quantile(0.1));
        Assert.Equal(1, countMap.Quantile(0.2));
        Assert.Equal(2, countMap.Quantile(0.3));
        Assert.Equal(2, countMap.Quantile(0.4));
        Assert.Equal(2, countMap.Quantile(0.5));
        Assert.Equal(3, countMap.Quantile(0.6));
        Assert.Equal(3, countMap.Quantile(0.7));
        Assert.Equal(4, countMap.Quantile(0.8));
        Assert.Equal(4, countMap.Quantile(0.9));
        Assert.Equal(4, countMap.Quantile(1.0));
    }

    [Fact]
    public void Quantile_ReturnsCorrectValues_OddNumberOfUniqueObs()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);
        
        Assert.Equal(1, countMap.Quantile(0.00));
        Assert.Equal(1, countMap.Quantile(0.10));
        Assert.Equal(1, countMap.Quantile(0.25));
        Assert.Equal(2, countMap.Quantile(0.35));
        Assert.Equal(2, countMap.Quantile(0.50));
        Assert.Equal(2, countMap.Quantile(0.75));
        Assert.Equal(3, countMap.Quantile(0.85));
        Assert.Equal(3, countMap.Quantile(0.90));
        Assert.Equal(3, countMap.Quantile(0.95));
        Assert.Equal(3, countMap.Quantile(1.00));
    }
    
    [Fact]
    public void Median_ReturnsCorrectMedianObservation()
    {
        // odd number of observations
        var countMap = new CountMap<int>();
        countMap.Fit(1, 1);
        countMap.Fit(2, 1);
        countMap.Fit(3, 1);

        Assert.Equal(2, countMap.Median());
        
        // even number of observations returns first observation where PDF >= 0.5
        countMap.Fit(4, 1);
        Assert.Equal(2, countMap.Median());
    }
}