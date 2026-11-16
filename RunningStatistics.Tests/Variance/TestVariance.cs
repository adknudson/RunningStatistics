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
    public void BatchFit_AccurateNear1e9()
    {
        var offset = 1e9;
        var rng = new Random(42);
        var values = new List<double>();
        for (var i = 0; i < 100; i++)
        {
            values.Add(offset + rng.NextDouble());
        }
        
        var batch = new RunningStatistics.Variance();
        batch.Fit(values);
        
        var mean = 0.0;
        foreach (var v in values)
        {
            mean += v;
        }
        mean /= values.Count;
        var sumSquaredDeviation = 0.0;
        foreach (var v in values)
        {
            sumSquaredDeviation += (v - mean) * (v - mean);
        }
        var expected = sumSquaredDeviation / (values.Count - 1);
        
        Utils.AssertIsApproximate(expected, batch.Value, 0);
        
        var incremental = new RunningStatistics.Variance();
        foreach (var v in values)
        {
            incremental.Fit(v);
        }
        Utils.AssertIsApproximate(expected, incremental.Value, 0);
    }
    
    [Fact]
    public void ConstantStream_VarianceNeverNegative()
    {
        var v1 = new RunningStatistics.Variance();
        for (var i = 0; i < 20; i++)
        {
            v1.Fit(7.0);
        }
        
        var v2 = new RunningStatistics.Variance();
        for (var i = 0; i < 5; i++)
        {
            v2.Fit(7.0);
        }
        
        var v3 = new RunningStatistics.Variance();
        v3.Fit([7.0, 7.0, 7.0]);
        v2.Merge(v3);
        
        v1.Merge(v2);
        
        Assert.Equal(28, v1.Nobs);
        Assert.Equal(0, v1.Value);
        Assert.Equal(0, v1.StandardDeviation);
    }
    
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var v = new RunningStatistics.Variance();
        v.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => v.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, v.Nobs);
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

    [Fact]
    public void Fit_RejectsNullIterable()
    {
        var variance = new RunningStatistics.Variance();
        Assert.Throws<ArgumentNullException>(() => variance.Fit((IEnumerable<double>?)null));
        Assert.Equal(0, variance.Nobs);
    }
}