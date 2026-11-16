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

    // PIN: the non-finite contract of double Sum, documented exactly as the code
    // behaves today: Fit's first guard is Require.Finite (ArgumentException), the count guard
    // Require.NonNegative (ArgumentOutOfRangeException) comes second, and both the scalar and
    // batch paths validate before touching state. No library change.

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
        // Require.Finite(value) runs BEFORE anything else: even count == 0 does not let a
        // non-finite value through (double Fit has no zero-count short-circuit ahead of the guards).
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
        // Fit(NaN, -1): the Finite guard wins -> ArgumentException, NOT ArgumentOutOfRangeException;
        // a negative count gives ArgumentOutOfRangeException only for a finite value.
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => sum.Fit(double.NaN, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sum.Fit(1.0, -1));
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
    }

    [Fact]
    public void BatchFit_NonFiniteElementThrowsStateUntouched()
    {
        // batch Fit(IEnumerable<double>) copies (values.ToList()) and validates every element
        // (xs.ForEach(Require.Finite)) BEFORE any accumulation: the state before the rejected
        // call is exactly the state after (Nobs/Value pinned before == after).
        var fresh = new RunningStatistics.Sum();
        Assert.Throws<ArgumentException>(() => fresh.Fit(new double[] { 1.0, double.PositiveInfinity, 2.0 }));
        Assert.Equal(0, fresh.Nobs);
        Assert.Equal(0.0, fresh.Value);

        var fitted = new RunningStatistics.Sum();
        fitted.Fit(1.5, 3); // sum of three 1.5 observations: Nobs 3, Value 4.5
        var nobsBefore = fitted.Nobs;
        var valueBefore = fitted.Value;
        Assert.Throws<ArgumentException>(() => fitted.Fit(new double[] { 1.0, double.NaN, 2.0 }));
        Assert.Equal(nobsBefore, fitted.Nobs);  // before == after
        Assert.Equal(valueBefore, fitted.Value);
    }

    [Fact]
    public void Merge_HugeFiniteValuesSaturateValueToInfinity()
    {
        // OBSERVATION, documented and NOT fixed: the Value field is a double, so accumulated
        // finite observations saturate to ±Inf per IEEE (only the *value* goes non-finite; Nobs
        // stays finite and exact). Merge itself has no guards; Fit rejects non-finite
        // observations but cannot stop the accumulated Value from overflowing.
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

        // same IEEE saturation on the Fit path with count > 1: value * count overflows to ±Inf
        var fitSaturated = new RunningStatistics.Sum();
        fitSaturated.Fit(1.7e308, 2);
        Assert.Equal(2, fitSaturated.Nobs);
        Assert.Equal(double.PositiveInfinity, fitSaturated.Value);
    }

    [Fact]
    public void Fit_RejectsNullIterable()
    {
        var sum = new RunningStatistics.Sum();
        Assert.Throws<ArgumentNullException>(() => sum.Fit((IEnumerable<double>)null));
        Assert.Equal(0, sum.Nobs);
    }
}