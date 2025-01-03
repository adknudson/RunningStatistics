using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Variance;

public partial class TestVariance()
    : AbstractRunningStatsTest<double, RunningStatistics.Variance>(
        () => Random.Shared.NextDouble(),
        () => new RunningStatistics.Variance())
{
    [Fact]
    public void Fit_GuardsAgainstNaN()
    {
        var v = new RunningStatistics.Variance();
        Assert.Throws<ArgumentException>(() => v.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => v.Fit(double.NaN, 2));
        Assert.Throws<ArgumentException>(() => v.Fit([1.0, double.NaN, 2.0]));
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.NaN, 2),
                new(2.0, 3)
            };

            v.Fit(keyValuePairs);
        });
    }
    
    [Fact]
    public void Fit_GuardsAgainstNegativeCount()
    {
        var v = new RunningStatistics.Variance();
        Assert.Throws<ArgumentOutOfRangeException>(() => v.Fit(1.0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(2.0, -2)
            };

            v.Fit(keyValuePairs);
        });
    }

    [Fact]
    public void Fit_GuardsAgainstInfiniteValues()
    {
        var v = new RunningStatistics.Variance();
        Assert.Throws<ArgumentException>(() => v.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => v.Fit(double.NegativeInfinity));
        
        Assert.Throws<ArgumentException>(() => v.Fit(double.PositiveInfinity, 2));
        Assert.Throws<ArgumentException>(() => v.Fit(double.NegativeInfinity, 2));

        Assert.Throws<ArgumentException>(() => v.Fit([1.0, double.PositiveInfinity, 2.0]));
        Assert.Throws<ArgumentException>(() => v.Fit([1.0, double.NegativeInfinity, 2.0]));
        
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.PositiveInfinity, 2),
                new(2.0, 3)
            };

            v.Fit(keyValuePairs);
        });
        
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.NegativeInfinity, 2),
                new(2.0, 3)
            };

            v.Fit(keyValuePairs);
        });
    }
    
    [Fact]
    public void Fit_ZeroCountDoesNotChangeState()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0);
        v.Fit(2.0);
        v.Fit(3.0, 0);
        
        Assert.Equal(2, v.Nobs);
        Assert.Equal(0.5, v.Value);
    }

    [Fact]
    public void Merge_DoesNotChangeOtherState()
    {
        var v1 = new RunningStatistics.Variance();
        v1.Fit(1.0);
        v1.Fit(2.0);
        
        var v2 = new RunningStatistics.Variance();
        v2.Fit(3.0);
        v2.Fit(4.0);
        
        // pre-conditions
        Assert.Equal(2, v2.Nobs);
        Assert.Equal(0.5, v2.Value);
        
        v1.Merge(v2);
        
        // post-conditions
        Assert.Equal(4, v1.Nobs);
        Assert.Equal(1.6666666666666667, v1.Value, 10);
        Assert.Equal(2, v2.Nobs);
        Assert.Equal(0.5, v2.Value);
    }
    
    [Fact]
    public void Merge_WithEmptyOther()
    {
        var v1 = new RunningStatistics.Variance();
        v1.Fit(1.0);
        v1.Fit(2.0);
        
        var v2 = new RunningStatistics.Variance();
        v1.Merge(v2);
        
        Assert.Equal(2, v1.Nobs);
        Assert.Equal(0.5, v1.Value);
    }
    
    [Fact]
    public void Merge_WithNonEmptyOther()
    {
        var v1 = new RunningStatistics.Variance();
        v1.Fit(1.0);
        v1.Fit(2.0);
        
        var v2 = new RunningStatistics.Variance();
        v2.Fit(3.0);
        v2.Fit(4.0);
        
        v1.Merge(v2);
        
        Assert.Equal(4, v1.Nobs);
        Assert.Equal(1.6666666666666667, v1.Value, 10);
    }
    
    [Fact]
    public void MergeEmpty_WithEmptyOther()
    {
        var v1 = new RunningStatistics.Variance();
        var v2 = new RunningStatistics.Variance();
        
        v1.Merge(v2);
        
        Assert.Equal(0, v1.Nobs);
        Assert.Equal(double.NaN, v1.Value);
    }
}