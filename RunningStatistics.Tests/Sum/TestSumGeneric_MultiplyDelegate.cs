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
        // long: IMultiplyOperators<long, long, long> � exact match, no conversion needed
        Assert.NotNull(Sum<long>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForDouble()
    {
        // double: IMultiplyOperators<double, double, double> � count is widened long?double
        Assert.NotNull(Sum<double>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForDecimal()
    {
        // decimal: IMultiplyOperators<decimal, decimal, decimal> � count is widened long?decimal
        Assert.NotNull(Sum<decimal>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForFloat()
    {
        // float: IMultiplyOperators<float, float, float> � count is narrowed long?float
        // (precision loss accepted for counts > ~16.7M)
        Assert.NotNull(Sum<float>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNotNull_ForMyNum()
    {
        // MyNum explicitly implements IMultiplyOperators<MyNum, long, MyNum>
        Assert.NotNull(Sum<MyNum>.MultiplyDelegate);
    }

    [Fact]
    public void MultiplyDelegate_IsNull_ForMyAddOnlyNumType()
    {
        // MyAddOnlyNum only implements IAdditionOperators � no multiply available
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
        // float has a 24-bit mantissa, so integers are only exactly representable up to 2^24.
        // Beyond that, adding 1f to an accumulator that is already >= 2^24 has no effect �
        // the addition rounds back to the same value. This means the loop path saturates.
        //
        // At count = 2^25:
        //   Delegate path: 1f * (float)(2^25) = 1f * 33554432f = 33554432f  (multiply is exact here)
        //   Loop path:     saturates at 2^24 = 16777216f, because once the sum reaches 2^24,
        //                  each subsequent +1f rounds back to 2^24 and contributes nothing.
        //
        // Note: at count = 2^24 + 1 both paths give the same wrong answer (2^24), because
        // (float)(2^24+1) also rounds to 2^24, so there is no divergence there.
        const float value = 1f;
        const long count = 1L << 25; // 2^25 = 33,554,432

        var delegatePath = new Sum<float>();
        delegatePath.Fit(value, count);

        var loopPath = new Sum<float>();
        for (var i = 0L; i < count; i++) loopPath.Fit(value);

        Assert.NotEqual(loopPath.Value, delegatePath.Value);
        Assert.Equal(1L << 25, delegatePath.Value);  // multiply: 1f * 2^25f = 2^25
        Assert.Equal(1L << 24, loopPath.Value);      // loop: saturates at 2^24
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

