using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Sum;

public partial class TestSum
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => sum.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, sum.Nobs);
    }

    [Fact]
    public void SumWith_NoNumbers()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Equal(0, sum.Value);
    }

    [Fact]
    public void SumWith_SingleNumber()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(42);
        Assert.Equal(42, sum.Value);
    }
    
    [Fact]
    public void SumOf_PositiveNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(20);
        sum.Fit(30);
        Assert.Equal(60, sum.Value);
    }

    [Fact]
    public void SumOf_NegativeNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(-10);
        sum.Fit(-20);
        sum.Fit(-30);
        Assert.Equal(-60, sum.Value);
    }

    [Fact]
    public void SumOf_MixedNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(-20);
        sum.Fit(30);
        Assert.Equal(20, sum.Value);
    }
    
    [Fact]
    public void Mean_NoNumbers_IsNaN()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Equal(double.NaN, sum.Mean());
    }

    [Fact]
    public void Mean_SingleNumber()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(42);
        Assert.Equal(42, sum.Mean());
    }

    [Fact]
    public void Mean_PositiveNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(20);
        sum.Fit(30);
        Assert.Equal(20, sum.Mean());
    }

    [Fact]
    public void Mean_NegativeNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(-10);
        sum.Fit(-20);
        sum.Fit(-30);
        Assert.Equal(-20, sum.Mean());
    }

    [Fact]
    public void Mean_MixedNumbers()
    {
        var sum = new RunningStatistics.Sum();
        sum.Fit(10);
        sum.Fit(-20);
        sum.Fit(30);
        Assert.Equal(6.67, sum.Mean(), 2);
    }

    // The promise about non-finite values, pinned exactly as the code behaves today
    // without changing the library: fitting first checks that the value is finite
    // (throwing an ArgumentException), and only then that the count is not negative
    // (throwing an ArgumentOutOfRangeException). Both the single-value and the batch
    // paths check their input before touching any state.

    [Fact]
    public void Fit_RejectsInfinityAndNaN()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NegativeInfinity));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN));
        // every rejected call leaves the state at its identity
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
    }

    [Fact]
    public void Fit_GuardOrder_FiniteBeforeZeroCount()
    {
        // The finiteness of the value is checked before anything else, so even a zero
        // count does not let a non-finite value through: unlike the other statistics,
        // the double fit here has no zero-count shortcut placed before the guards.
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.PositiveInfinity, 0));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NegativeInfinity, 0));
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN, 0));
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
        // a finite value with count 0 is accepted and is a state no-op
        var fitted = new RunningStatistics.Sum();
        fitted.Fit(2.5, 4);
        fitted.Fit(5.0, 0);
        Assert.Equal(4, fitted.Nobs);
        Assert.Equal(10.0, fitted.Value);
    }

    [Fact]
    public void Fit_GuardOrder_NaNBeatsNegativeCount()
    {
        // A bad value is rejected before the bad count is even considered, so fitting
        // NaN with a negative count reports the bad-value ArgumentException rather than
        // the bad-count ArgumentOutOfRangeException. Only a finite value with a
        // negative count gets the count error.
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sum.Fit(1.0, -1));
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
    }

    [Fact]
    public void BatchFit_NonFiniteElementThrowsStateUntouched()
    {
        // Fitting a collection of doubles copies it and validates every element before
        // adding anything, so a call that is rejected leaves the statistic exactly as it
        // was: the counts and the sum read the same values the test saved before the
        // call.
        var fresh = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => fresh.Fit(new double[] { 1.0, double.PositiveInfinity, 2.0 }));
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(0.0, fresh.Value);

        var fitted = new RunningStatistics.Sum();
        fitted.Fit(1.5, 3); // sum of three 1.5 observations: Nobs 3, Value 4.5
        var nobsBefore = fitted.Nobs;
        var valueBefore = fitted.Value;
        Assert.Throws<ArgumentException>(() => fitted.Fit(new double[] { 1.0, double.NaN, 2.0 }));
        Assert.Equal(nobsBefore, fitted.Nobs);  // the saved values still match
        Assert.Equal(valueBefore, fitted.Value);
    }

    [Fact]
    public void Merge_HugeFiniteValuesSaturateValueToInfinity()
    {
        // An observation about the current behavior, deliberately documented and not
        // fixed: the running total is a double, so adding huge finite values saturates
        // it to an infinity the way IEEE arithmetic does. Only the total goes
        // non-finite; the observation count stays finite and exact. Merge has no
        // guards of its own, and although Fit rejects non-finite observations, nothing
        // prevents the accumulated total itself from overflowing.
        var pos1 = new RunningStatistics.Sum();
        var pos2 = new RunningStatistics.Sum();
        pos1.Fit(1.7e308);
        pos2.Fit(1.7e308);
        pos1.Merge(pos2);
        Assert.Equal(2, pos1.Nobs);
        Assert.Equal(double.PositiveInfinity, pos1.Value);
        Assert.Equal(double.PositiveInfinity, pos1.Mean());
        Assert.Equal(1, pos2.Nobs);
        Assert.Equal(1.7e308, pos2.Value);

        var neg1 = new RunningStatistics.Sum();
        var neg2 = new RunningStatistics.Sum();
        neg1.Fit(-1.7e308);
        neg2.Fit(-1.7e308);
        neg1.Merge(neg2);
        Assert.Equal(2, neg1.Nobs);
        Assert.Equal(double.NegativeInfinity, neg1.Value);

        // The same saturation happens on a weighted fit of a count above one: value
        // times count passes beyond the largest double and lands on an infinity.
        var fitSaturated = new RunningStatistics.Sum();
        fitSaturated.Fit(1.7e308, 2);
        Assert.Equal(2, fitSaturated.Nobs);
        Assert.Equal(double.PositiveInfinity, fitSaturated.Value);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}