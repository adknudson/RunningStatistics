﻿using Xunit;

namespace RunningStatistics.Tests.CountMap;

public partial class TestCountMap
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
    public void StdDev_ReturnsCorrectStandardDeviation()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.753, countMap.StandardDeviation(), 3);
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