using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Mean;

public partial class TestMean
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => mean.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, mean.Nobs);

        var other = new RunningStatistics.Mean();
        other.Fit(2.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => mean.Merge(other));
        Assert.Equal(long.MaxValue / 2 + 1, mean.Nobs);
    }

    [Fact]
    public void Mean_ReturnsNaN_WhenNoNumbersAdded()
    {
        var mean = new RunningStatistics.Mean();
        Assert.Equal(double.NaN, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenSingleNumberAdded()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(5);
        Assert.Equal(5, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenMultipleNumbersAdded()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(new List<double> { 5, 10, 15 });
        Assert.Equal(10, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenNegativeNumbersAdded()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(new List<double> { -5, -10, -15 });
        Assert.Equal(-10, mean.Value);
    }

    [Fact]
    public void Mean_ReturnsCorrectValue_WhenMixedNumbersAdded()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(new List<double> { -5, 10, 15 });
        Assert.Equal(6.67, mean.Value, 2);
    }

    [Fact]
    public void Mean_ResetsCorrectly()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(new List<double> { 5, 10, 15 });
        mean.Reset();
        Assert.Equal(double.NaN, mean.Value);
    }

    [Fact]
    public void Mean_ClonesCorrectly()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit(new List<double> { 5, 10, 15 });
        var clone = mean.Clone();
        Assert.Equal(mean.Value, clone.Value);
        Assert.Equal(mean.Nobs, clone.Nobs);
    }

    [Fact]
    public void Mean_ClonesEmptyCorrectly()
    {
        var mean = new RunningStatistics.Mean();
        var emptyClone = mean.CloneEmpty();
        Assert.Equal(double.NaN, emptyClone.Value);
        Assert.Equal(0, emptyClone.Nobs);
    }

    [Fact]
    public void Mean_MergesCorrectly()
    {
        var mean1 = new RunningStatistics.Mean();
        mean1.Fit(new List<double> { 5, 10, 15 });
        var mean2 = new RunningStatistics.Mean();
        mean2.Fit(new List<double> { 20, 25, 30 });
        mean1.Merge(mean2);
        Assert.Equal(17.5, mean1.Value, 1);
    }
    
    [Fact]
    public void MergeEmpty_DoesNotPropagateNaNs()
    {
        var a = new RunningStatistics.Mean();
        a.Fit(3.14159);
        a.Fit(2.71828);
        var m = a.Value;

        var b = new RunningStatistics.Mean();
        Assert.Equal(double.NaN, b.Value);
        
        a.Merge(b);
        Assert.Equal(a.Value, m);
    }

    [Fact]
    public void Merge_ThrowsOnNonFiniteObservation()
    {
        var a = new RunningStatistics.Mean();
        Assert.Throws<ArgumentException>(() => a.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => a.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => a.Fit(double.NegativeInfinity));
    }

    // The batch path for collections of doubles folds the whole batch in one step. It averages
    // the values and blends with weight count over count. That result must agree with the
    // incremental path of repeated single fits. The observation counts must be exactly equal,
    // and the mean must match within the usual 1e-8 relative tolerance. The data comes from a
    // seeded generator only. The fact never uses unseeded sampling.

    [Fact]
    public void MeanPaths_BatchEqualsLoop()
    {
        var rng = new Random(20251114);
        var values = new List<double>();
        for (var i = 0; i < 3000; i++)
        {
            values.Add(rng.NextDouble());
        }

        var batch = new RunningStatistics.Mean();
        batch.Fit(values);

        var loop = new RunningStatistics.Mean();
        foreach (var v in values)
        {
            loop.Fit(v);
        }

        Assert.Equal(batch.Nobs, loop.Nobs);
        Utils.AssertIsApproximate(loop.Value, batch.Value, 0);
    }

    [Fact]
    public void MeanPaths_BatchAfterBatchEqualsLoop()
    {
        var rng = new Random(20251114);

        var firstBatch = new List<double>();
        for (var i = 0; i < 7; i++)
        {
            firstBatch.Add(rng.NextDouble());
        }

        // Smaller than the current Nobs (7) at the moment it is fitted.
        var secondBatch = new List<double>();
        for (var i = 0; i < 3; i++)
        {
            secondBatch.Add(rng.NextDouble());
        }

        // Far larger than the current Nobs (10) at the moment it is fitted.
        var thirdBatch = new List<double>();
        for (var i = 0; i < 3000; i++)
        {
            thirdBatch.Add(rng.NextDouble());
        }

        var stat = new RunningStatistics.Mean();
        stat.Fit(firstBatch);
        stat.Fit(secondBatch);
        stat.Fit(thirdBatch);

        var loop = new RunningStatistics.Mean();
        foreach (var v in firstBatch)
        {
            loop.Fit(v);
        }
        foreach (var v in secondBatch)
        {
            loop.Fit(v);
        }
        foreach (var v in thirdBatch)
        {
            loop.Fit(v);
        }

        Assert.Equal(3010, stat.Nobs);
        Assert.Equal(stat.Nobs, loop.Nobs);
        Utils.AssertIsApproximate(loop.Value, stat.Value, 0);
    }

    [Fact]
    public void MeanPaths_MergeEqualsBatchAllAndLoop()
    {
        var rng = new Random(20251114);
        var head = new List<double>();
        var tail = new List<double>();
        for (var i = 0; i < 300; i++)
        {
            head.Add(rng.NextDouble());
        }
        for (var i = 0; i < 300; i++)
        {
            tail.Add(rng.NextDouble());
        }
        var allValues = new List<double>(head);
        foreach (var v in tail)
        {
            allValues.Add(v);
        }

        var left = new RunningStatistics.Mean();
        left.Fit(head);
        var right = new RunningStatistics.Mean();
        right.Fit(tail);
        left.Merge(right);

        var batchAll = new RunningStatistics.Mean();
        batchAll.Fit(allValues);

        var loop = new RunningStatistics.Mean();
        foreach (var v in allValues)
        {
            loop.Fit(v);
        }

        Assert.Equal(600, left.Nobs);
        Assert.Equal(600, batchAll.Nobs);
        Assert.Equal(600, loop.Nobs);

        Utils.AssertIsApproximate(batchAll.Value, left.Value, 0);
        Utils.AssertIsApproximate(loop.Value, left.Value, 0);
        Utils.AssertIsApproximate(loop.Value, batchAll.Value, 0);
    }

    [Fact]
    public void MeanPaths_BatchFromEmptyEqualsBatchMean()
    {
        var rng = new Random(20251114);
        var values = new List<double>();
        for (var i = 0; i < 250; i++)
        {
            values.Add(rng.NextDouble());
        }

        var stat = new RunningStatistics.Mean();
        stat.Fit(values);

        // First batch from empty: Nobs == batch size exactly.
        Assert.Equal(values.Count, stat.Nobs);

        var sum = 0.0;
        foreach (var v in values)
        {
            sum += v;
        }
        var batchMean = sum / values.Count;

        // The first batch blends into an empty statistic with weight one, which leaves exactly the
        // plain average of the values. The statistic's value is therefore the library's own
        // average of the collection. The simple running sum computed here can differ from that
        // average only in the order of the additions. That is why the check is tolerant.
        Utils.AssertIsApproximate(batchMean, stat.Value, 0);
    }

    [Fact]
    public void MeanPaths_FitEmptyIterableIsNoOp()
    {
        var empty = new RunningStatistics.Mean();
        empty.Fit(new List<double>());
        Assert.Equal(0, empty.Nobs);
        Assert.Equal(double.NaN, empty.Value);

        var filled = new RunningStatistics.Mean();
        filled.Fit(2.5, 4);
        filled.Fit(new List<double>());
        Assert.Equal(4, filled.Nobs);
        Assert.Equal(2.5, filled.Value);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}