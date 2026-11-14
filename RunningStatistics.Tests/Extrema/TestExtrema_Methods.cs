using System;
using Xunit;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtrema
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var extrema = new RunningStatistics.Extrema();
        extrema.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => extrema.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, extrema.Nobs);
    }

    [Fact]
    public void ConstructorInitializesProperties()
    {
        RunningStatistics.Extrema extrema = new();

        Assert.Equal(double.NaN, extrema.Min);
        Assert.Equal(double.NaN, extrema.Max);
        Assert.Equal(0, extrema.MinCount);
        Assert.Equal(0, extrema.MaxCount);
        Assert.Equal(0, extrema.Nobs);
    }
    
    [Fact]
    public void FittingSingleValue()
    {
        RunningStatistics.Extrema extrema = new();

        extrema.Fit(10.0);
        Assert.Equal(10.0, extrema.Min);
        Assert.Equal(10.0, extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(1, extrema.Nobs);
    }
    
    [Fact]
    public void FitUpdatesMinAndMaxCorrectly()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);

        Assert.Equal(3.0, extrema.Min);
        Assert.Equal(7.0, extrema.Max);
    }

    [Fact]
    public void FitUpdatesMinCountAndMaxCountCorrectly()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);
        extrema.Fit(7.0);
        extrema.Fit(7.0);

        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
    }

    [Fact]
    public void FitUpdatesNobsCorrectly()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);

        Assert.Equal(3, extrema.Nobs);
    }
    
    [Fact]
    public void FitHandlesNaNValues()
    {
        RunningStatistics.Extrema extrema = new();
        Assert.Throws<ArgumentException>(() => extrema.Fit(double.NaN));
    }

    [Fact]
    public void FitHandlesExtremeValues()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(double.MinValue);
        extrema.Fit(double.MaxValue);
        extrema.Fit(double.MinValue);

        Assert.Equal(double.MinValue, extrema.Min);
        Assert.Equal(double.MaxValue, extrema.Max);
        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(3, extrema.Nobs);
    }
    
    [Fact]
    public void MergeCombinesMinAndMaxCorrectly()
    {
        RunningStatistics.Extrema a = new();
        a.Fit(1.0);
        a.Fit(2.0);

        RunningStatistics.Extrema b = new();
        b.Fit(0.5);
        b.Fit(3.0);

        a.Merge(b);

        Assert.Equal(0.5, a.Min);
        Assert.Equal(3.0, a.Max);
    }

    [Fact]
    public void MergeCombinesMinCountAndMaxCountCorrectly()
    {
        RunningStatistics.Extrema a = new();
        a.Fit(1.0);
        a.Fit(1.0);
        a.Fit(2.0);

        RunningStatistics.Extrema b = new();
        b.Fit(1.0);
        b.Fit(3.0);
        b.Fit(3.0);

        a.Merge(b);

        Assert.Equal(3, a.MinCount);
        Assert.Equal(2, a.MaxCount);
    }

    [Fact]
    public void MergeCombinesNobsCorrectly()
    {
        RunningStatistics.Extrema a = new();
        a.Fit(1.0);
        a.Fit(2.0);

        RunningStatistics.Extrema b = new();
        b.Fit(0.5);
        b.Fit(3.0);

        a.Merge(b);

        Assert.Equal(4, a.Nobs);
    }

    [Fact]
    public void MergeEmpty_PropertiesAreCorrect()
    {
        RunningStatistics.Extrema a = new();
        RunningStatistics.Extrema b = new(); 
        a.Merge(b);

        Assert.Equal(double.NaN, a.Min);
        Assert.Equal(double.NaN, a.Max);
        Assert.Equal(0, a.MinCount);
        Assert.Equal(0, a.MaxCount);
        Assert.Equal(0, a.Nobs);
    }


    [Fact]
    public void MergePartsEqualsMergeAll()
    {
        const int n = 2000;
        var rng = new Random();

        RunningStatistics.Extrema a = new(); RunningStatistics.Extrema b = new(); RunningStatistics.Extrema c = new();

        double v;
        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            a.Fit(v);
            c.Fit(v);
        }

        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            b.Fit(v);
            c.Fit(v);
        }

        a.Merge(b);

        Assert.Equal(a.Min, c.Min);
        Assert.Equal(a.MinCount, c.MinCount);
        Assert.Equal(a.Max, c.Max);
        Assert.Equal(a.MaxCount, c.MaxCount);
    }

    [Fact]
    public void StaticMergeDoesNotAffectOriginals()
    {
        RunningStatistics.Extrema a = new();
        a.Fit([1, 1, 2, 3, 5, 8, 13, 21, 34, 55]);

        RunningStatistics.Extrema b = new();
        b.Fit([-1, -1, -1, 34]);

        var c = RunningStatistics.Extrema.Merge(a, b);
            
        Assert.Equal(1, a.Min);
        Assert.Equal(55, a.Max);
            
        Assert.Equal(-1, b.Min);
        Assert.Equal(34, b.Max);
            
        Assert.Equal(-1, c.Min);
        Assert.Equal(55, c.Max);
    }
    
    [Fact]
    public void ResetMethodResetsProperties()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(1.0);
        extrema.Fit(2.0);
        extrema.Fit(3.0);

        extrema.Reset();

        Assert.Equal(double.NaN, extrema.Min);
        Assert.Equal(double.NaN, extrema.Max);
        Assert.Equal(0, extrema.MinCount);
        Assert.Equal(0, extrema.MaxCount);
        Assert.Equal(0, extrema.Nobs);
    }
    
    [Fact]
    public void RangeMethodReturnsCorrectValue()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(1.0);
        extrema.Fit(3.0);

        Assert.Equal(2.0, extrema.Range());
    }

    // Pins: Fit accepts infinities (only Require.NotNaN guards the value), so +Inf/-Inf become
    // real min/max states with batch counts accumulating; NaN is still rejected.
    [Fact]
    public void Fit_InfinityObservationsAccepted()
    {
        var extrema = new RunningStatistics.Extrema();
        extrema.Fit(double.PositiveInfinity);

        Assert.Equal(double.PositiveInfinity, extrema.Min);
        Assert.Equal(double.PositiveInfinity, extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(1, extrema.Nobs);

        extrema.Fit(double.PositiveInfinity, 2);
        Assert.Equal(3, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
        Assert.Equal(3, extrema.Nobs);

        var negative = new RunningStatistics.Extrema();
        negative.Fit(double.NegativeInfinity, 2);
        Assert.Equal(double.NegativeInfinity, negative.Min);
        Assert.Equal(double.NegativeInfinity, negative.Max);
        Assert.Equal(2, negative.MinCount);
        Assert.Equal(2, negative.MaxCount);
        Assert.Equal(2, negative.Nobs);
    }

    // Pins: a stream seeing both infinities ends with Min=-Inf, Max=+Inf (Inf comparisons order as
    // -Inf < x < +Inf); Range() of the mixed state is +Inf - (-Inf) = +Inf. NaN still rejected.
    [Fact]
    public void Fit_MixedInfinityExtremes()
    {
        var extrema = new RunningStatistics.Extrema();
        extrema.Fit(double.PositiveInfinity);
        extrema.Fit(double.NegativeInfinity);

        Assert.Equal(double.NegativeInfinity, extrema.Min);
        Assert.Equal(double.PositiveInfinity, extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(2, extrema.Nobs);
        Assert.Equal(double.PositiveInfinity, extrema.Range());

        var reversed = new RunningStatistics.Extrema();
        reversed.Fit(double.NegativeInfinity);
        reversed.Fit(double.PositiveInfinity);

        Assert.Equal(double.NegativeInfinity, reversed.Min);
        Assert.Equal(double.PositiveInfinity, reversed.Max);
        Assert.Equal(1, reversed.MinCount);
        Assert.Equal(1, reversed.MaxCount);
        Assert.Equal(2, reversed.Nobs);

        Assert.Throws<ArgumentException>(() => extrema.Fit(double.NaN));
    }

    // Pins: a flat all-Infinity stream has Min == Max == +/-Inf, so Range() = Inf - Inf = NaN
    // (IEEE, honest — not a guard violation).
    [Fact]
    public void Range_InfiniteFlatStreamIsNaN()
    {
        var plus = new RunningStatistics.Extrema();
        plus.Fit(double.PositiveInfinity, 3);
        Assert.Equal(double.NaN, plus.Range());

        var minus = new RunningStatistics.Extrema();
        minus.Fit(double.NegativeInfinity, 2);
        Assert.Equal(double.NaN, minus.Range());
    }

    // Pins signed zeros: IEEE comparisons keep the FIRST-fitted zero as both min and max
    // (-0.0 == 0.0 is true, so the second zero only bumps MinCount/MaxCount). Assert.Equal on
    // doubles compares bitwise (===), which would distinguish -0.0, but the sign helpers make
    // the pinned bit-level identity explicit and readable.
    [Fact]
    public void Fit_SignedZerosKeepFirstFitted()
    {
        var negativeFirst = new RunningStatistics.Extrema();
        negativeFirst.Fit(-0.0);
        negativeFirst.Fit(0.0);

        Assert.True(IsNegativeZero(negativeFirst.Min));
        Assert.True(IsNegativeZero(negativeFirst.Max));
        Assert.Equal(2, negativeFirst.MinCount);
        Assert.Equal(2, negativeFirst.MaxCount);
        Assert.Equal(2, negativeFirst.Nobs);

        var positiveFirst = new RunningStatistics.Extrema();
        positiveFirst.Fit(0.0);
        positiveFirst.Fit(-0.0);

        Assert.True(IsPositiveZero(positiveFirst.Min));
        Assert.True(IsPositiveZero(positiveFirst.Max));
        Assert.Equal(2, positiveFirst.Nobs);
    }

    // Pins: merging zero-carrying stats takes the THIS side's zero (_min == other._min holds for
    // -0.0 vs 0.0, so only counts merge; the survivor is this's sign).
    [Fact]
    public void Merge_SignedZerosKeepThisSidesZero()
    {
        var negative = new RunningStatistics.Extrema();
        negative.Fit(-0.0);
        var positive = new RunningStatistics.Extrema();
        positive.Fit(0.0);

        negative.Merge(positive);
        Assert.True(IsNegativeZero(negative.Min));
        Assert.True(IsNegativeZero(negative.Max));
        Assert.Equal(2, negative.MinCount);
        Assert.Equal(2, negative.MaxCount);
        Assert.Equal(2, negative.Nobs);

        var positiveFirst = new RunningStatistics.Extrema();
        positiveFirst.Fit(0.0);
        var negativeSecond = new RunningStatistics.Extrema();
        negativeSecond.Fit(-0.0);
        positiveFirst.Merge(negativeSecond);

        Assert.True(IsPositiveZero(positiveFirst.Min));
        Assert.True(IsPositiveZero(positiveFirst.Max));
    }

    // Pins Fit(value, count) with count > 1: batch counts land on MinCount/MaxCount/Nobs exactly,
    // independent of the order in which distinct values are fitted.
    [Fact]
    public void FitBatched_MinMaxCountsNobs()
    {
        var extrema = new RunningStatistics.Extrema();
        extrema.Fit(5.0, 3);
        Assert.Equal(5.0, extrema.Min);
        Assert.Equal(5.0, extrema.Max);
        Assert.Equal(3, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
        Assert.Equal(3, extrema.Nobs);

        extrema.Fit(2.0, 4);
        Assert.Equal(2.0, extrema.Min);
        Assert.Equal(5.0, extrema.Max);
        Assert.Equal(4, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
        Assert.Equal(7, extrema.Nobs);

        var reversed = new RunningStatistics.Extrema();
        reversed.Fit(2.0, 4);
        reversed.Fit(5.0, 3);
        Assert.Equal(2.0, reversed.Min);
        Assert.Equal(5.0, reversed.Max);
        Assert.Equal(4, reversed.MinCount);
        Assert.Equal(3, reversed.MaxCount);
        Assert.Equal(7, reversed.Nobs);
    }

    // Pins: count == 0 returns before any state update (empty stays empty; existing state untouched).
    [Fact]
    public void FitBatched_ZeroCountShortCircuit()
    {
        var extrema = new RunningStatistics.Extrema();
        extrema.Fit(5.0, 0);
        Assert.Equal(double.NaN, extrema.Min);
        Assert.Equal(0, extrema.Nobs);

        extrema.Fit(3.0, 2);
        extrema.Fit(9.0, 0);
        Assert.Equal(3.0, extrema.Min);
        Assert.Equal(3.0, extrema.Max);
        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(2, extrema.MaxCount);
        Assert.Equal(2, extrema.Nobs);
    }

    // Pins: Merge of batched stats keeps the honest merged min/max and their occurrence counts,
    // in either merge direction (same as sequential Fit).
    [Fact]
    public void MergeBatched_ValuesMatchSequential()
    {
        var highFirst = new RunningStatistics.Extrema();
        highFirst.Fit(5.0, 3);
        var lowFirst = new RunningStatistics.Extrema();
        lowFirst.Fit(2.0, 4);

        highFirst.Merge(lowFirst);
        Assert.Equal(2.0, highFirst.Min);
        Assert.Equal(5.0, highFirst.Max);
        Assert.Equal(4, highFirst.MinCount);
        Assert.Equal(3, highFirst.MaxCount);
        Assert.Equal(7, highFirst.Nobs);

        var lowStat = new RunningStatistics.Extrema();
        lowStat.Fit(2.0, 4);
        var highStat = new RunningStatistics.Extrema();
        highStat.Fit(5.0, 3);
        lowStat.Merge(highStat);
        Assert.Equal(2.0, lowStat.Min);
        Assert.Equal(5.0, lowStat.Max);
        Assert.Equal(4, lowStat.MinCount);
        Assert.Equal(3, lowStat.MaxCount);
        Assert.Equal(7, lowStat.Nobs);
    }

    // Pins: when both stats share the same extreme value, the occurrence counts SUM on merge.
    [Fact]
    public void MergeBatched_EqualExtremesCountsSum()
    {
        var first = new RunningStatistics.Extrema();
        first.Fit(5.0, 3);
        var second = new RunningStatistics.Extrema();
        second.Fit(5.0, 2);

        first.Merge(second);
        Assert.Equal(5.0, first.Min);
        Assert.Equal(5.0, first.Max);
        Assert.Equal(5, first.MinCount);
        Assert.Equal(5, first.MaxCount);
        Assert.Equal(5, first.Nobs);
    }

    /// <summary>
    /// Bit-level sign check: IEEE division sends -0.0 to -Infinity and +0.0 to +Infinity, whereas
    /// == cannot distinguish -0.0 from +0.0.
    /// </summary>
    private static bool IsNegativeZero(double value) => value == 0.0 && 1.0 / value < 0.0;

    /// <summary>
    /// Counterpart of IsNegativeZero distinguishing +0.0 from -0.0 at the bit level.
    /// </summary>
    private static bool IsPositiveZero(double value) => value == 0.0 && 1.0 / value > 0.0;
}