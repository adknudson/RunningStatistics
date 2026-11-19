using Xunit;

namespace RunningStatistics.Tests.Sum;

public class TestSumGeneric_MultiplyDelegate
{
    // -------------------------------------------------------------------------
    // Delegate presence
    // -------------------------------------------------------------------------

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForLong()
    {
        // long multiplies by long directly: an exact match, no conversion needed
        Assert.NotNull(Sum<long>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForDouble()
    {
        // double multiplies by double, so the whole-number count is widened to double first
        Assert.NotNull(Sum<double>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForDecimal()
    {
        // decimal multiplies by decimal, so the whole-number count is widened to decimal first
        Assert.NotNull(Sum<decimal>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForFloat()
    {
        // float multiplies by float, so the whole-number count is narrowed to float first,
        // accepting precision loss for counts above roughly 16.7 million
        Assert.NotNull(Sum<float>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForMyNum()
    {
        // MyNum declares its own multiply operator taking a whole-number count
        Assert.NotNull(Sum<MyNum>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNull_ForMyAddOnlyNumType()
    {
        // MyAddOnlyNum only offers addition, so no multiply path is available for it
        Assert.Null(Sum<MyAddOnlyNum>.MultiplyDelegate);
    }

    // -------------------------------------------------------------------------
    // Correctness: multiply path (delegate is non-null)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(3L, 1)]
    [InlineData(3L, 5)]
    [InlineData(-7L, 4)]
    [InlineData(0L, 100)]
    public void FitWithCount_Long_DelegatePath_MatchesRepeatedSingleFit(long value, int count)
    {
        var delegatePath = new Sum<long>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<long>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value, delegatePath.Value);
        Assert.Equal(loopPath.Nobs, delegatePath.Nobs);
    }

    [Theory]
    [InlineData(3.0, 1)]
    [InlineData(3.0, 5)]
    [InlineData(-7.5, 4)]
    [InlineData(0.0, 100)]
    public void FitWithCount_Double_DelegatePath_MatchesRepeatedSingleFit(double value, int count)
    {
        var delegatePath = new Sum<double>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<double>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value, delegatePath.Value);
        Assert.Equal(loopPath.Nobs, delegatePath.Nobs);
    }

    [Theory]
    [InlineData(3, 1)]
    [InlineData(3, 5)]
    [InlineData(-7, 4)]
    [InlineData(0, 100)]
    public void FitWithCount_Decimal_DelegatePath_MatchesRepeatedSingleFit(int rawValue, int count)
    {
        var value = (decimal)rawValue;

        var delegatePath = new Sum<decimal>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<decimal>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value, delegatePath.Value);
        Assert.Equal(loopPath.Nobs, delegatePath.Nobs);
    }

    [Theory]
    [InlineData(3f, 1)]
    [InlineData(3f, 5)]
    [InlineData(-7.5f, 4)]
    [InlineData(0f, 100)]
    public void FitWithCount_Float_DelegatePath_MatchesRepeatedSingleFit(float value, int count)
    {
        var delegatePath = new Sum<float>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<float>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value, delegatePath.Value, 3);
        Assert.Equal(loopPath.Nobs, delegatePath.Nobs);
    }

    [Fact]
    public void FitWithCount_Float_PrecisionBreaks_AbovePow2_24()
    {
        // A float keeps twenty-four bits of mantissa, so counting by ones only stays exact up to
        // 2^24. Adding 1f to an accumulator already at or past that point has no effect: the
        // addition rounds back to the same value, so the loop path saturates there.
        //
        // At a count of 2^25:
        //   The delegate path multiplies 1f by the float of that count, 33554432f, and the product
        //   is exact because it is representable.
        //   The loop path saturates at 2^24, the value 16777216f: once the sum reaches it, every
        //   further 1f rounds back and contributes nothing.
        //
        // Note that at a count of 2^24 plus one both paths give the same wrong answer, 2^24,
        // because the float of 2^24 plus one also rounds to 2^24; there is nothing to diverge.
        const float value = 1f;
        const long count = 1L << 25; // 33,554,432 observations

        var delegatePath = new Sum<float>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<float>();
        for (var i = 0L; i < count; i++) loopPath.Fit(value);

        Assert.NotEqual(loopPath.Value, delegatePath.Value);
        Assert.Equal(1L << 25, delegatePath.Value); // the delegate path multiplies exactly
        Assert.Equal(1L << 24, loopPath.Value);      // the loop path saturates short of the exact product
    }

    [Theory]
    [InlineData(7, 1)]
    [InlineData(7, 10)]
    [InlineData(-3, 6)]
    [InlineData(0, 50)]
    public void FitWithCount_Long_MatchesRepeatedSingleFit(long value, int count)
    {
        var delegatePath = new Sum<long>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<long>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value, delegatePath.Value);
        Assert.Equal(loopPath.Nobs, delegatePath.Nobs);
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(5, 8)]
    [InlineData(-2, 3)]
    public void FitWithCount_MyNum_DelegatePath_MatchesRepeatedSingleFit(int rawValue, int count)
    {
        var value = new MyNum(rawValue);

        var delegatePath = new Sum<MyNum>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<MyNum>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value, delegatePath.Value);
        Assert.Equal(loopPath.Nobs, delegatePath.Nobs);
    }

    // -------------------------------------------------------------------------
    // Correctness: fallback loop path (delegate is null)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(4, 1)]
    [InlineData(4, 7)]
    [InlineData(-1, 3)]
    [InlineData(0, 10)]
    public void FitWithCount_MyAddOnlyNum_MatchesRepeatedSingleFit(int rawValue, int count)
    {
        var value = new MyAddOnlyNum(rawValue);

        var fallbackPath = new Sum<MyAddOnlyNum>();
        fallbackPath.Fit(value, count);

        var loopPath = new Sum<MyAddOnlyNum>();
        for (var i = 0; i < count; i++) loopPath.Fit(value);

        Assert.Equal(loopPath.Value.Value, fallbackPath.Value.Value);
        Assert.Equal(loopPath.Nobs, fallbackPath.Nobs);
    }

    // -------------------------------------------------------------------------
    // Nobs accounting
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void FitWithCount_NobsIsCorrect_DelegatePath(long count)
    {
        var sum = new Sum<long>();
        sum.Fit(1L, count);
        Assert.Equal(count, sum.Nobs);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void FitWithCount_NobsIsCorrect_DelegatePath_Double(long count)
    {
        var sum = new Sum<double>();
        sum.Fit(1.0, count);
        Assert.Equal(count, sum.Nobs);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void FitWithCount_NobsIsCorrect_DelegatePath_Decimal(long count)
    {
        var sum = new Sum<decimal>();
        sum.Fit(1m, count);
        Assert.Equal(count, sum.Nobs);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void FitWithCount_NobsIsCorrect_FallbackPath(long count)
    {
        var sum = new Sum<MyAddOnlyNum>();
        sum.Fit(new MyAddOnlyNum(1), count);
        Assert.Equal(count, sum.Nobs);
    }

    // -------------------------------------------------------------------------
    // Edge cases
    // -------------------------------------------------------------------------

    [Fact]
    public void FitWithCount_Zero_DoesNotChangeState_DelegatePath()
    {
        var sum = new Sum<long>();
        sum.Fit(99L, 0);
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0L, sum.Value);
    }

    [Fact]
    public void FitWithCount_Zero_DoesNotChangeState_DelegatePath_Double()
    {
        var sum = new Sum<double>();
        sum.Fit(99.0, 0);
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0.0, sum.Value);
    }

    [Fact]
    public void FitWithCount_Zero_DoesNotChangeState_FallbackPath()
    {
        var sum = new Sum<MyAddOnlyNum>();
        sum.Fit(new MyAddOnlyNum(99), 0);
        Assert.Equal(0, sum.Nobs);
        Assert.Equal(0, sum.Value.Value);
    }
}

