using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Normal;

public partial class TestNormal()
    : AbstractRunningStatsTest<double, RunningStatistics.Normal>(
        () => MathNet.Numerics.Distributions.Normal.Sample(0, 1),
        () => new RunningStatistics.Normal())
{
    [Fact]
    public void Fit_GuardsAgainstNaN()
    {
        var normal = new RunningStatistics.Normal();
        Assert.Throws<ArgumentException>(() => normal.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => normal.Fit(double.NaN, 2));
        Assert.Throws<ArgumentException>(() => normal.Fit([1.0, double.NaN, 2.0]));
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.NaN, 2),
                new(2.0, 3)
            };

            normal.Fit(keyValuePairs);
        });
    }

    [Fact]
    public void Fit_GuardsAgainstNegativeCount()
    {
        var normal = new RunningStatistics.Normal();
        Assert.Throws<ArgumentOutOfRangeException>(() => normal.Fit(1.0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(2.0, -2)
            };

            normal.Fit(keyValuePairs);
        });
    }

    [Fact]
    public void Fit_GuardsAgainstInfiniteValues()
    {
        var normal = new RunningStatistics.Normal();
        Assert.Throws<ArgumentException>(() => normal.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => normal.Fit(double.NegativeInfinity));
        
        Assert.Throws<ArgumentException>(() => normal.Fit(double.PositiveInfinity, 2));
        Assert.Throws<ArgumentException>(() => normal.Fit(double.NegativeInfinity, 2));

        Assert.Throws<ArgumentException>(() => normal.Fit([1.0, double.PositiveInfinity, 2.0]));
        Assert.Throws<ArgumentException>(() => normal.Fit([1.0, double.NegativeInfinity, 2.0]));
        
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.PositiveInfinity, 2),
                new(2.0, 3)
            };

            normal.Fit(keyValuePairs);
        });
        
        Assert.Throws<ArgumentException>(() =>
        {
            var keyValuePairs = new List<KeyValuePair<double, long>>
            {
                new(1.0, 1),
                new(double.NegativeInfinity, 2),
                new(2.0, 3)
            };

            normal.Fit(keyValuePairs);
        });
    }
    
    [Fact]
    public void Fit_ZeroCountDoesNotChangeState()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0);
        normal.Fit(2.0);
        normal.Fit(3.0, 0);
        
        Assert.Equal(2, normal.Nobs);
        Assert.Equal(1.5, normal.Mean);
        Assert.Equal(0.5, normal.Variance);
    }

    [Fact]
    public void MergeWithEmptyOther()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0);
        normal.Fit(2.0);
        
        var other = new RunningStatistics.Normal();
        normal.Merge(other);
        
        Assert.Equal(2, normal.Nobs);
        Assert.Equal(1.5, normal.Mean);
        Assert.Equal(0.5, normal.Variance);
    }

    [Fact]
    public void Merge_WithNonEmptyOther()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0);
        normal.Fit(2.0);
        
        var other = new RunningStatistics.Normal();
        other.Fit(3.0);
        other.Fit(4.0);
        
        normal.Merge(other);
        
        Assert.Equal(4, normal.Nobs);
        Assert.Equal(2.5, normal.Mean);
        Assert.Equal(1.6666666666666667, normal.Variance, 10);
    }
    
    [Fact]
    public void Merge_DoesNotChangeOtherState()
    {
        var normal = new RunningStatistics.Normal();
        normal.Fit(1.0);
        normal.Fit(2.0);
        
        var other = new RunningStatistics.Normal();
        other.Fit(3.0);
        other.Fit(4.0);
        
        // pre-conditions
        Assert.Equal(2, other.Nobs);
        Assert.Equal(3.5, other.Mean);
        Assert.Equal(0.5, other.Variance);
        
        normal.Merge(other);
        
        // post-conditions
        Assert.Equal(4, normal.Nobs);
        Assert.Equal(2.5, normal.Mean);
        Assert.Equal(1.6666666666666667, normal.Variance, 10);
        Assert.Equal(3.5, other.Mean);
        Assert.Equal(0.5, other.Variance);
    }

    [Fact]
    public void MergeEmpty_WithEmptyOther()
    {
        var normal = new RunningStatistics.Normal();
        var other = new RunningStatistics.Normal();
        
        normal.Merge(other);
        
        Assert.Equal(0, normal.Nobs);
        Assert.Equal(double.NaN, normal.Mean);
        Assert.Equal(double.NaN, normal.Variance);
    }
}