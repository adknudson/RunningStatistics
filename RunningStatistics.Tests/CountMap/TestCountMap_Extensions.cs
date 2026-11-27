using System;
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

    // The Moments limit: the corrected variance needs two observations, the uncorrected
    // one needs one. Below the limit double kinds answer NaN, and the decimal and
    // generic kinds throw, since those types have no NaN to return.
    [Fact]
    public void Variance_AreNaNWhenFewerThanTwoObservations()
    {
        var empty = new CountMap<double>();
        Assert.True(double.IsNaN(empty.Variance()));
        Assert.True(double.IsNaN(empty.Variance(false)));
        Assert.True(double.IsNaN(empty.StandardDeviation()));

        var single = new CountMap<double>();
        single.Fit(5, 1);
        Assert.True(double.IsNaN(single.Variance()));
        Assert.Equal(0.0, single.Variance(false));
    }

    [Fact]
    public void Variance_ThrowsWhenFewerThanTwoObservationsForDecimalAndMyNum()
    {
        var singleDec = new CountMap<decimal>();
        singleDec.Fit(5, 1);
        Assert.Throws<DivideByZeroException>(() => singleDec.Variance());

        var singleMy = new CountMap<MyNum>();
        singleMy.Fit(new MyNum(5), 1);
        Assert.Throws<DivideByZeroException>(() => singleMy.Variance());
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

    // Sample skewness answers from three observations up, and sample kurtosis from four
    // up; below those limits both return NaN, matching the Moments convention.
    [Fact]
    public void Skewness_Nbelow3IsNaN()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 1);
        countMap.Fit(2, 1);

        Assert.Equal(double.NaN, countMap.Skewness());

        countMap.Fit(4, 1);
        Assert.True(!double.IsNaN(countMap.Skewness()));
    }

    [Fact]
    public void Kurtosis_Nbelow4IsNaN()
    {
        var countMap = new CountMap<int>();
        countMap.Fit(1, 1);
        countMap.Fit(2, 1);
        countMap.Fit(4, 1);

        Assert.Equal(double.NaN, countMap.Kurtosis());

        countMap.Fit(8, 1);
        Assert.True(!double.IsNaN(countMap.Kurtosis()));
    }

    // Zero spread: Moments guards m2 <= 0; the extensions guard the variance
    // argument, so constant maps and hand-passed zero variances answer NaN.
    [Fact]
    public void SkewnessAndKurtosis_AreNaNForConstantData()
    {
        var constant = new CountMap<double>();
        constant.Fit(5, 4);

        Assert.True(double.IsNaN(constant.Skewness()));
        Assert.True(double.IsNaN(constant.Kurtosis()));

        var spread = new CountMap<double>();
        spread.Fit(1, 2);
        spread.Fit(2, 3);
        spread.Fit(3, 1);
        Assert.True(double.IsNaN(spread.Skewness(spread.Mean(), 0.0)));
        Assert.True(double.IsNaN(spread.Kurtosis(spread.Mean(), 0.0)));
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

    // The decimal overloads reuse the int data, so the expected values match the
    // int facts to the same three digits; variance alone stays a decimal.
    [Fact]
    public void Variance_ReturnsCorrectVarianceForDecimal()
    {
        var countMap = new CountMap<decimal>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.567m, countMap.Variance(), 3);
        Assert.Equal(0.567m, countMap.Variance(countMap.Mean()), 3);
    }

    [Fact]
    public void StdDev_ReturnsCorrectStandardDeviationForDecimal()
    {
        var countMap = new CountMap<decimal>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.753, countMap.StandardDeviation(), 3);
        Assert.Equal(0.753, countMap.StandardDeviation(countMap.Mean()), 3);
    }

    [Fact]
    public void Skewness_ReturnsCorrectSkewnessForDecimal()
    {
        var countMap = new CountMap<decimal>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(0.228, countMap.Skewness(), 3);
    }

    [Fact]
    public void Kurtosis_ReturnsCorrectKurtosisForDecimal()
    {
        var countMap = new CountMap<decimal>();
        countMap.Fit(1, 2);
        countMap.Fit(2, 3);
        countMap.Fit(3, 1);

        Assert.Equal(2.107, countMap.Kurtosis(), 3);
    }

    [Fact]
    public void ExcessKurtosis_ReturnsCorrectExcessKurtosisForDecimal()
    {
        var countMap = new CountMap<decimal>();
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

    // The generic variance keeps the result in the observation type, so the
    // assertion reads the MyNum's decimal value, like the mean fact above.
    [Fact]
    public void Variance_ReturnsCorrectVarianceForMyNum()
    {
        var countMap = new CountMap<MyNum>();
        countMap.Fit(new MyNum(1), 2);
        countMap.Fit(new MyNum(2), 3);
        countMap.Fit(new MyNum(3), 1);

        Assert.IsType<MyNum>(countMap.Variance());
        Assert.Equal(0.567m, countMap.Variance().Value, 3);
        Assert.Equal(0.567m, countMap.Variance(countMap.Mean()).Value, 3);
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

    // Tie rule: when p lands exactly on a cumulative frequency, the scan
    // returns the key at which the frequency is reached, on either parity
    // of the unique-key count. The odd-key assertions fail on main, where
    // the odd branch accepted only a frequency strictly greater than p.
    [Fact]
    public void Quantile_ReturnsFirstKeyReachingCumulativeFrequency()
    {
        var evenKeys = new CountMap<int>();
        evenKeys.Fit(1, 1);
        evenKeys.Fit(2, 1);

        Assert.Equal(1, evenKeys.Quantile(0.5));

        var oddKeys = new CountMap<int>();
        oddKeys.Fit(1, 1);
        oddKeys.Fit(2, 1);
        oddKeys.Fit(3, 1);

        Assert.Equal(1, oddKeys.Quantile(1.0 / 3));
        Assert.Equal(2, oddKeys.Quantile(2.0 / 3));
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
        
        // with an even number of observations, the median is the first observation at which the
        // cumulative proportion of the sample reaches one half
        countMap.Fit(4, 1);
        Assert.Equal(2, countMap.Median());
    }
}