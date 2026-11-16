using System;
using System.Collections.Generic;
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
    public void Constructor_InitializesPropertiesCorrectly()
    {
        RunningStatistics.Extrema extrema = new();

        Assert.Equal(double.NaN, extrema.Min);
        Assert.Equal(double.NaN, extrema.Max);
        Assert.Equal(0, extrema.MinCount);
        Assert.Equal(0, extrema.MaxCount);
        Assert.Equal(0, extrema.Nobs);
    }
    
    [Fact]
    public void Fit_SingleValueSetsMinAndMax()
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
    public void FitUpdates_MinAndMaxCorrectly()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);

        Assert.Equal(3.0, extrema.Min);
        Assert.Equal(7.0, extrema.Max);
    }

    [Fact]
    public void FitUpdates_MinCountAndMaxCountCorrectly()
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
    public void FitUpdates_NobsCorrectly()
    {
        RunningStatistics.Extrema extrema = new();
        extrema.Fit(5.0);
        extrema.Fit(3.0);
        extrema.Fit(7.0);

        Assert.Equal(3, extrema.Nobs);
    }
    
    [Fact]
    public void Fit_RejectsNaN()
    {
        RunningStatistics.Extrema extrema = new();
        Assert.Throws<ArgumentException>(() => extrema.Fit(double.NaN));
    }

    [Fact]
    public void Fit_ExtremeValues()
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
    public void Merge_CombinesMinAndMaxCorrectly()
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
    public void Merge_CombinesMinCountAndMaxCountCorrectly()
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
    public void Merge_CombinesNobsCorrectly()
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


    // Merging two stats fitted with the first and second halves of the same data must match
    // a stat fitted with all of the data at once.
    [Fact]
    public void Merge_PartsEqualsAll()
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

    // The static Type.Merge(a, b) returns a new stat; a and b keep their own state.
    [Fact]
    public void StaticMerge_DoesNotAffectOriginals()
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
    public void Reset_ClearsState()
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
    public void Range_ReturnsCorrectValue()
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

    // Pin (empty-state asymmetry): the double Extrema and the generic Extrema<TObs> DISAGREE on
    // how an empty statistic (Nobs == 0) exposes its extremes. Double: Min/Max accessors return
    // double.NaN sentinels (no throw); Range() computes NaN - NaN = NaN (unguarded arithmetic on
    // the NaN accessors); ToString renders "Min=NaN (n=0), Max=NaN (n=0)". Generic: the Min/Max
    // accessors THROW InvalidOperationException("Nobs must be greater than 0") — there is no NaN
    // sentinel for an arbitrary comparable TObs — and the NET7 Range extension (available only for
    // TObs : ISubtractionOperators, e.g. MyNum; not for MyComparable) evaluates Max - Min so it
    // propagates the same throw. Yet ToString of the generic type BYPASSES the guarded accessors
    // and interpolates the raw _min/_max fields: null (class TObs) -> EMPTY text; for struct-like
    // TObs the TObs? fields reify as plain structs holding the TObs DEFAULT, so a spurious zero
    // extreme LEAKS (Min=0 for MyNum). MinCount/MaxCount/Nobs are the same identities (0) on both
    // types. All contracts are individually clean; the asymmetry is pinned as-is, no library
    // change.

    [Fact]
    public void EmptyStateAccessors_DoubleNaNVsGenericThrow()
    {
        var d = new RunningStatistics.Extrema();
        Assert.Equal(0, d.Nobs);
        Assert.Equal(double.NaN, d.Min);
        Assert.Equal(double.NaN, d.Max);
        Assert.Equal(0, d.MinCount);
        Assert.Equal(0, d.MaxCount);

        var comparable = new RunningStatistics.Extrema<MyComparable>();
        Assert.Equal(0, comparable.Nobs);
        Assert.Equal(0, comparable.MinCount);
        Assert.Equal(0, comparable.MaxCount);
        var minThrow = Assert.Throws<InvalidOperationException>(() => comparable.Min);
        Assert.Equal("Nobs must be greater than 0", minThrow.Message);
        Assert.Throws<InvalidOperationException>(() => comparable.Max);

        var num = new RunningStatistics.Extrema<MyNum>();
        Assert.Equal(0, num.Nobs);
        Assert.Equal(0, num.MinCount);
        Assert.Equal(0, num.MaxCount);
        Assert.Throws<InvalidOperationException>(() => num.Min);
        Assert.Throws<InvalidOperationException>(() => num.Max);
    }

    [Fact]
    public void EmptyStateRange_DoubleNaNVsGenericThrow()
    {
        var d = new RunningStatistics.Extrema();
        Assert.Equal(double.NaN, d.Range());

        // MyNum : ISubtractionOperators<MyNum,MyNum,MyNum> -> the NET7 Range extension applies:
        // extrema.Max - extrema.Min propagates the accessor throw. Extrema<MyComparable> has NO
        // Range(): MyComparable lacks ISubtractionOperators, so the extension never applies.
        var num = new RunningStatistics.Extrema<MyNum>();
        Assert.Throws<InvalidOperationException>(() => num.Range());
    }

    [Fact]
    public void EmptyStateToString_DoubleNaNVsGenericFieldLeak()
    {
        var d = new RunningStatistics.Extrema();
        Assert.Contains("Min=NaN (n=0), Max=NaN (n=0)", d.ToString());

        // The generic ToString reads the private _min/_max fields, NOT the throwing accessors.
        // The rendering of the empty state depends on the cardinality of TObs:
        // MyComparable is a class -> fields are null -> render as EMPTY text (no throw), even
        // though the Min/Max accessors themselves throw:
        var comparable = new RunningStatistics.Extrema<MyComparable>();
        Assert.Contains("Min= (n=0), Max= (n=0)", comparable.ToString());
        Assert.Throws<InvalidOperationException>(() => comparable.Min);

        // MyNum is struct-like -> the TObs? fields reify as plain MyNum holding the DEFAULT
        // MyNum(0), so ToString LEAKS a spurious "0" extreme (a third empty-state shape:
        // double = NaN, class TObs = empty text, struct TObs = zero default). Accessors still
        // throw, so the leaked value is reachable ONLY through ToString:
        var num = new RunningStatistics.Extrema<MyNum>();
        Assert.Contains("Min=0 (n=0), Max=0 (n=0)", num.ToString());
        Assert.Throws<InvalidOperationException>(() => num.Min);
    }

    [Fact]
    public void EmptyStateFitZeroCount_KeepsGenericEmptyState()
    {
        var comparable = new RunningStatistics.Extrema<MyComparable>();
        comparable.Fit(new MyComparable(5.0m), 0);
        Assert.Equal(0, comparable.Nobs);
        Assert.Equal(0, comparable.MinCount);
        Assert.Equal(0, comparable.MaxCount);
        Assert.Throws<InvalidOperationException>(() => comparable.Min);
        Assert.Throws<InvalidOperationException>(() => comparable.Max);

        var num = new RunningStatistics.Extrema<MyNum>();
        num.Fit(new MyNum(5.0m), 0);
        Assert.Equal(0, num.Nobs);
        Assert.Throws<InvalidOperationException>(() => num.Min);

        // Same guard ORDER as the double type (whose Fit(NaN, 0) throws ArgumentException): the
        // value guard Require.NotNull runs BEFORE the count == 0 short-circuit -> ANE even at 0.
        Assert.Throws<ArgumentNullException>(() => comparable.Fit((MyComparable)null, 0));

        // On a NON-empty generic statistic, Fit(value, 0) leaves state untouched.
        var filled = new RunningStatistics.Extrema<MyComparable>();
        filled.Fit(new MyComparable(3.0m), 2);
        filled.Fit(new MyComparable(8.0m), 0);
        Assert.Equal(2, filled.Nobs);
        Assert.Equal(new MyComparable(3.0m), filled.Min);
        Assert.Equal(new MyComparable(3.0m), filled.Max);
        Assert.Equal(2, filled.MinCount);
        Assert.Equal(2, filled.MaxCount);
    }

    // Pin: empty-merge AGREES between the two types on state (Nobs/counts stay 0, values
    // copied/kept symmetrically) but still differs on how emptiness is EXPOSED (NaN vs throw).
    // (Generic empty-merge cases are also pinned in the generic Methods files; kept here side
    // by side with the double.)
    [Fact]
    public void EmptyStateMerge_EmptyIntoEmpty_StaysEmptyBothTypes()
    {
        var d = new RunningStatistics.Extrema();
        d.Merge(new RunningStatistics.Extrema());
        Assert.Equal(0, d.Nobs);
        Assert.Equal(double.NaN, d.Min);
        Assert.Equal(double.NaN, d.Max);
        Assert.Equal(0, d.MinCount);
        Assert.Equal(0, d.MaxCount);

        var g = new RunningStatistics.Extrema<MyComparable>();
        g.Merge(new RunningStatistics.Extrema<MyComparable>());
        Assert.Equal(0, g.Nobs);
        Assert.Equal(0, g.MinCount);
        Assert.Equal(0, g.MaxCount);
        Assert.Throws<InvalidOperationException>(() => g.Min);
        Assert.Throws<InvalidOperationException>(() => g.Max);
    }

    [Fact]
    public void EmptyStateMerge_EmptyIntoNonEmpty_CopiesOther()
    {
        var dSource = new RunningStatistics.Extrema();
        dSource.Fit(2.0);
        dSource.Fit(5.0);
        var d = new RunningStatistics.Extrema();
        d.Merge(dSource);
        Assert.Equal(2, d.Nobs);
        Assert.Equal(2.0, d.Min);
        Assert.Equal(5.0, d.Max);
        Assert.Equal(1, d.MinCount);
        Assert.Equal(1, d.MaxCount);

        var gSource = new RunningStatistics.Extrema<MyComparable>();
        gSource.Fit(new MyComparable(2.0m));
        gSource.Fit(new MyComparable(5.0m));
        var g = new RunningStatistics.Extrema<MyComparable>();
        g.Merge(gSource);
        Assert.Equal(2, g.Nobs);
        Assert.Equal(new MyComparable(2.0m), g.Min);
        Assert.Equal(new MyComparable(5.0m), g.Max);
        Assert.Equal(1, g.MinCount);
        Assert.Equal(1, g.MaxCount);
    }

    [Fact]
    public void EmptyStateMerge_NonEmptyIntoEmpty_KeepsState()
    {
        var d = new RunningStatistics.Extrema();
        d.Fit(4.0, 3);
        d.Merge(new RunningStatistics.Extrema());
        Assert.Equal(3, d.Nobs);
        Assert.Equal(4.0, d.Min);
        Assert.Equal(4.0, d.Max);
        Assert.Equal(3, d.MinCount);
        Assert.Equal(3, d.MaxCount);

        var g = new RunningStatistics.Extrema<MyComparable>();
        g.Fit(new MyComparable(4.0m), 3);
        g.Merge(new RunningStatistics.Extrema<MyComparable>());
        Assert.Equal(3, g.Nobs);
        Assert.Equal(new MyComparable(4.0m), g.Min);
        Assert.Equal(new MyComparable(4.0m), g.Max);
        Assert.Equal(3, g.MinCount);
        Assert.Equal(3, g.MaxCount);
    }

    // Null iterable Fit contract — Extrema (double) has no iterable Fit override.
    [Fact]
    public void Fit_RejectsNullIterable()
    {
        var extrema = new RunningStatistics.Extrema();
        Assert.Throws<ArgumentNullException>(() => extrema.Fit((IEnumerable<double>)null));
        Assert.Equal(0, extrema.Nobs);
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