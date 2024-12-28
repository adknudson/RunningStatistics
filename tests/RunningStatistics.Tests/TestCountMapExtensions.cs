using RunningStatistics.Tests.Extras;
using Xunit;

namespace RunningStatistics.Tests;

public class TestCountMapExtensions
{
    [Fact]
    public void MinKey_ReturnsMinimumKeyForNumerics()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(1, countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ReturnsMaximumKeyForNumerics()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(3, countMap.MaxKey());
    }
    
    [Fact]
    public void MinKey_ReturnsMinimumKeyForStrings()
    {
        var countMap = new CountMap<string>();
        countMap.Fit("a", 2);
        countMap.Fit("b", 3);
        countMap.Fit("c", 1);

        Assert.Equal("a", countMap.MinKey());
    }

    [Fact]
    public void MaxKey_ReturnsMaximumKeyForStrings()
    {
        var countMap = new CountMap<string>();
        countMap.Fit("a", 2);
        countMap.Fit("b", 3);
        countMap.Fit("c", 1);

        Assert.Equal("c", countMap.MaxKey());
    }

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
    public void Mean_ReturnsCorrectMean()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(1.8333333333333333, countMap.Mean(), 10);
    }

    [Fact]
    public void Variance_ReturnsCorrectVariance()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.4722222222222223, countMap.Variance(), 10);
    }

    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviation()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.6871842709362769, countMap.StandardDeviation(), 10);
    }

    [Fact]
    public void Skewness_ReturnsCorrectSkewness()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.22826882356360775, countMap.Skewness(), 10);
    }

    [Fact]
    public void Kurtosis_ReturnsCorrectKurtosis()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(2.1072664359861584, countMap.Kurtosis(), 10);
    }

    [Fact]
    public void ExcessKurtosis_ReturnsCorrectExcessKurtosis()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(-0.8927335640138416, countMap.ExcessKurtosis(), 10);
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

        Assert.Equal(1.8333333333333333m, countMap.Mean().Value, 10);
        Assert.IsType<MyNum>(countMap.Mean());
    }
}