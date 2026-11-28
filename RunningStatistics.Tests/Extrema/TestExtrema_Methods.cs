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


    // The fact merges the data as two stats, one for the first half and one for the second
    // half. Their merge must match a stat fitted with all of the data at once.
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

    // The static Type.Merge(a, b) returns a new stat. a and b keep their own state.
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

        Assert.Equal(2.0, extrema.Range);
    }

    // Fit only rejects NaN, so it accepts infinities as real observations. A fitted
    // infinity becomes the minimum or the maximum, and batched counts accumulate for
    // it just like for any other value.
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

    // When a stream sees both infinities, the minimum ends at negative infinity and the
    // maximum ends at positive infinity. Comparisons order every finite value between
    // them. The range of such a mixed state is positive infinity. The code still rejects
    // NaN. The order in which the infinities arrive does not matter.
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
        Assert.Equal(double.PositiveInfinity, extrema.Range);

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

    // A stream of one single infinity repeated many times has equal minimum and maximum. The
    // range therefore subtracts that infinity from itself, and IEEE rules make that NaN. This
    // is plain arithmetic on honest values, not a guard failing.
    [Fact]
    public void Range_InfiniteFlatStreamIsNaN()
    {
        var plus = new RunningStatistics.Extrema();
        plus.Fit(double.PositiveInfinity, 3);
        Assert.Equal(double.NaN, plus.Range);

        var minus = new RunningStatistics.Extrema();
        minus.Fit(double.NegativeInfinity, 2);
        Assert.Equal(double.NaN, minus.Range);
    }

    // Comparisons treat negative zero and positive zero as equal. The zero fitted first
    // therefore stays both the minimum and the maximum. A second zero of the other sign
    // only raises the occurrence counts. The test uses Assert.Equal, which compares doubles
    // bit for bit and can tell the two zeros apart. The sign helper methods make the
    // pinned bit-level identity explicit and readable.
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

    // When both statistics hold one of the two signed zeros, the merge keeps the zero of
    // the receiver. Comparisons call the two zeros equal, so only the counts merge. The
    // surviving zero therefore carries the receiver's sign.
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

    // A weighted fit of a count above one lands exactly on the minimum count, the maximum
    // count and the total count. The result does not depend on the order in which the
    // distinct values are fitted.
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

    // A zero count returns before the code touches any state. The statistic therefore stays
    // empty if it was empty, and it keeps its existing observations if it had any.
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

    // Merging statistics that weighted fits filled keeps the true extremes of all the
    // observations involved, and their occurrence counts. The outcome is the same whichever
    // statistic merges into which, and it matches a sequential fit of the same observations.
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

    // The two Extrema types now follow one documented convention split by observation type
    // (issue #12). The facts pin both sides after that standardization.
    //
    // Numeric side: the double Extrema reads NaN from Min and Max when empty, because a
    // double statistic can name an undefined readout with NaN. Its Range() comes out as
    // NaN, because it subtracts those NaN accessors. ToString renders both extremes as
    // "NaN (n=0)".
    //
    // Generic side: no sentinel value could stand in for a missing extreme of an arbitrary
    // type, so the generic Extrema throws from Min and Max when empty. Each message names
    // the empty state and points at the non-throwing read that answers it, so a caller
    // seeing the throw knows what to call next. The assertions below pin that wording, so a
    // rewording shows up as a red gate rather than as a silent drift. TryGetMin and TryGetMax
    // give callers a non-throwing read; their facts live in the generic test files. The range
    // extension subtracts the two accessors, so it propagates the same throw. That extension
    // only exists for types that support subtraction, such as MyNum but not MyComparable.
    // accessors, so it propagates the same throw. That extension only exists for types
    // that support subtraction, such as MyNum but not MyComparable.
    //
    // ToString of the generic type reads the raw private fields instead of the throwing
    // accessors. For a reference type the fields are still null, so they print as empty text.
    // For a value type the fields hold plain default objects, which leak a spurious zero
    // extreme into the text. The count fields read the same zeros on both types. Each
    // contract here is individually clean. Only the reporting shape disagrees.

    [Fact]
    public void EmptyStateAccessors_DoubleNaNVsGenericThrow()
    {
        var d = new RunningStatistics.Extrema();
        Assert.Equal(0, d.Nobs);
        Assert.Equal(double.NaN, d.Min);
        Assert.Equal(double.NaN, d.Max);
        Assert.Equal(0, d.MinCount);
        Assert.Equal(0, d.MaxCount);

        var comparable = new Extrema<MyComparable>();
        Assert.Equal(0, comparable.Nobs);
        Assert.Equal(0, comparable.MinCount);
        Assert.Equal(0, comparable.MaxCount);
        var minThrow = Assert.Throws<InvalidOperationException>(() => comparable.Min);
        Assert.Equal("No observations fitted yet. Use TryGetMin.", minThrow.Message);
        var maxThrow = Assert.Throws<InvalidOperationException>(() => comparable.Max);
        Assert.Equal("No observations fitted yet. Use TryGetMax.", maxThrow.Message);

        var num = new Extrema<MyNum>();
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
        Assert.Equal(double.NaN, d.Range);

        // MyNum supports subtraction, so the Range() extension applies and the accessor
        // throw propagates through it. Extrema of MyComparable has no Range() at all,
        // because MyComparable does not support subtraction.
        var num = new Extrema<MyNum>();
        Assert.Throws<InvalidOperationException>(() => num.Range);
    }

    [Fact]
    public void EmptyStateToString_DoubleNaNVsGenericFieldLeak()
    {
        var d = new RunningStatistics.Extrema();
        Assert.Contains("Min=NaN (n=0), Max=NaN (n=0)", d.ToString());

        // The generic ToString reads the private fields rather than the throwing accessors. The
        // empty state therefore renders per the shape of the type. MyComparable is a reference
        // type whose fields are still null, so it prints empty text without throwing, as the
        // lines below show. Asking the accessors directly would throw.
        var comparable = new Extrema<MyComparable>();
        Assert.Contains("Min= (n=0), Max= (n=0)", comparable.ToString());
        Assert.Throws<InvalidOperationException>(() => comparable.Min);

        // MyNum is a value type, so its fields hold plain default objects. ToString therefore
        // leaks a spurious "0" for each extreme. The empty state then has three shapes. It
        // reads NaN for the double, empty text for a reference type, and the default value for
        // a value type. The accessors still throw, so this leaked value is only visible
        // through ToString, as the lines below show.
        var num = new Extrema<MyNum>();
        Assert.Contains("Min=0 (n=0), Max=0 (n=0)", num.ToString());
        Assert.Throws<InvalidOperationException>(() => num.Min);
    }

    [Fact]
    public void EmptyStateFitZeroCount_KeepsGenericEmptyState()
    {
        var comparable = new Extrema<MyComparable>();
        comparable.Fit(new MyComparable(5.0m), 0);
        Assert.Equal(0, comparable.Nobs);
        Assert.Equal(0, comparable.MinCount);
        Assert.Equal(0, comparable.MaxCount);
        Assert.Throws<InvalidOperationException>(() => comparable.Min);
        Assert.Throws<InvalidOperationException>(() => comparable.Max);

        var num = new Extrema<MyNum>();
        num.Fit(new MyNum(5.0m), 0);
        Assert.Equal(0, num.Nobs);
        Assert.Throws<InvalidOperationException>(() => num.Min);
        
        // On a NON-empty generic statistic, Fit(value, 0) leaves state untouched.
        var filled = new Extrema<MyComparable>();
        filled.Fit(new MyComparable(3.0m), 2);
        filled.Fit(new MyComparable(8.0m), 0);
        Assert.Equal(2, filled.Nobs);
        Assert.Equal(new MyComparable(3.0m), filled.Min);
        Assert.Equal(new MyComparable(3.0m), filled.Max);
        Assert.Equal(2, filled.MinCount);
        Assert.Equal(2, filled.MaxCount);
    }

    // Merging two empty statistics keeps both types empty, and the two agree on the state
    // itself. Counts stay zero, and no values move in either direction. The two differ only
    // in how they report emptiness. The double reports NaN, and the generic throws. The
    // generic empty-merge cases are also pinned in the generic test files. This file keeps
    // them side by side for contrast.
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

        var g = new Extrema<MyComparable>();
        g.Merge(new Extrema<MyComparable>());
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

        var gSource = new Extrema<MyComparable>();
        gSource.Fit(new MyComparable(2.0m));
        gSource.Fit(new MyComparable(5.0m));
        var g = new Extrema<MyComparable>();
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

        var g = new Extrema<MyComparable>();
        g.Fit(new MyComparable(4.0m), 3);
        g.Merge(new Extrema<MyComparable>());
        Assert.Equal(3, g.Nobs);
        Assert.Equal(new MyComparable(4.0m), g.Min);
        Assert.Equal(new MyComparable(4.0m), g.Max);
        Assert.Equal(3, g.MinCount);
        Assert.Equal(3, g.MaxCount);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.

    /// <summary>
    /// Tells an ordinary zero from a negative zero. Equals cannot tell the two apart.
    /// One divided by the zero sends negative zero to negative infinity. This check uses
    /// that difference.
    /// </summary>
    private static bool IsNegativeZero(double value) => value == 0.0 && 1.0 / value < 0.0;

    /// <summary>
    /// Matches the check for the ordinary positive zero, not the negative zero. It divides
    /// one by the zero and applies the same division trick.
    /// </summary>
    private static bool IsPositiveZero(double value) => value == 0.0 && 1.0 / value > 0.0;
}