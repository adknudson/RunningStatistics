using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace RunningStatistics.Tests.Sum;

/// <summary>
/// Abstract base for testing <see cref="Sum{TObs}"/> value behaviour across numeric types.
/// Subclasses supply a <see cref="From"/> converter so every test runs for their concrete type.
/// Mean tests live in each subclass because the return type and empty-state behaviour vary.
/// </summary>
public abstract class TestSumGenericMethods<TObs>
    where TObs : IAdditionOperators<TObs, TObs, TObs>, IAdditiveIdentity<TObs, TObs>
{
    protected abstract TObs From(int value);

    [Fact] public void SumWith_NoNumbers()
    {
        var sum = new Sum<TObs>();
        Assert.Equal(TObs.AdditiveIdentity, sum.Value);
    }

    [Fact] public void SumWith_SingleNumber()
    {
        var sum = new Sum<TObs>();
        sum.Fit(From(42));
        Assert.Equal(From(42), sum.Value);
    }

    [Fact] public void SumOf_PositiveNumbers()
    {
        var sum = new Sum<TObs>();
        sum.Fit(From(10));
        sum.Fit(From(20));
        sum.Fit(From(30));
        Assert.Equal(From(60), sum.Value);
    }

    [Fact] public void SumOf_NegativeNumbers()
    {
        var sum = new Sum<TObs>();
        sum.Fit(From(-10));
        sum.Fit(From(-20));
        sum.Fit(From(-30));
        Assert.Equal(From(-60), sum.Value);
    }

    [Fact] public void SumOf_MixedNumbers()
    {
        var sum = new Sum<TObs>();
        sum.Fit(From(10));
        sum.Fit(From(-20));
        sum.Fit(From(30));
        Assert.Equal(From(20), sum.Value);
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}

// ---------------------------------------------------------------------------
// Concrete subclasses — one per supported type
// ---------------------------------------------------------------------------

public class TestSumGenericMethods_Int : TestSumGenericMethods<int>
{
    protected override int From(int value) => value;

    [Fact] public void Mean_NoNumbers_IsNaN()
        => Assert.Equal(double.NaN, new Sum<int>().Mean());

    [Fact] public void Mean_SingleNumber()
    {
        var sum = new Sum<int>();
        sum.Fit(42);
        Assert.Equal(42.0, sum.Mean());
    }

    [Fact] public void Mean_PositiveNumbers()
    {
        var sum = new Sum<int>();
        sum.Fit(10); sum.Fit(20); sum.Fit(30);
        Assert.Equal(20.0, sum.Mean());
    }

    [Fact] public void Mean_MixedNumbers()
    {
        var sum = new Sum<int>();
        sum.Fit(10); sum.Fit(-20); sum.Fit(30);
        Assert.Equal(6.67, sum.Mean(), 2);
    }
}

public class TestSumGenericMethods_Long : TestSumGenericMethods<long>
{
    protected override long From(int value) => value;

    [Fact] public void Mean_NoNumbers_IsNaN()
        => Assert.Equal(double.NaN, new Sum<long>().Mean());

    [Fact] public void Mean_SingleNumber()
    {
        var sum = new Sum<long>();
        sum.Fit(42L);
        Assert.Equal(42.0, sum.Mean());
    }

    [Fact] public void Mean_PositiveNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L); sum.Fit(20L); sum.Fit(30L);
        Assert.Equal(20.0, sum.Mean());
    }

    [Fact] public void Mean_MixedNumbers()
    {
        var sum = new Sum<long>();
        sum.Fit(10L); sum.Fit(-20L); sum.Fit(30L);
        Assert.Equal(6.67, sum.Mean(), 2);
    }
}

public class TestSumGenericMethods_Double : TestSumGenericMethods<double>
{
    protected override double From(int value) => value;

    [Fact] public void Mean_NoNumbers_IsNaN()
        => Assert.Equal(double.NaN, new Sum<double>().Mean());

    [Fact] public void Mean_SingleNumber()
    {
        var sum = new Sum<double>();
        sum.Fit(42.0);
        Assert.Equal(42.0, sum.Mean());
    }

    [Fact] public void Mean_PositiveNumbers()
    {
        var sum = new Sum<double>();
        sum.Fit(10.0); sum.Fit(20.0); sum.Fit(30.0);
        Assert.Equal(20.0, sum.Mean());
    }

    [Fact] public void Mean_MixedNumbers()
    {
        var sum = new Sum<double>();
        sum.Fit(10.0); sum.Fit(-20.0); sum.Fit(30.0);
        Assert.Equal(6.67, sum.Mean(), 2);
    }
}

public class TestSumGenericMethods_Float : TestSumGenericMethods<float>
{
    // float has no Mean() extension — only Sum/Value tests from the base apply.
    protected override float From(int value) => value;
}

public class TestSumGenericMethods_Decimal : TestSumGenericMethods<decimal>
{
    protected override decimal From(int value) => value;

    [Fact] public void Mean_NoNumbers_Throws()
        => Assert.Throws<DivideByZeroException>(() => new Sum<decimal>().Mean());

    [Fact] public void Mean_SingleNumber()
    {
        var sum = new Sum<decimal>();
        sum.Fit(42m);
        Assert.Equal(42m, sum.Mean());
    }

    [Fact] public void Mean_PositiveNumbers()
    {
        var sum = new Sum<decimal>();
        sum.Fit(10m); sum.Fit(20m); sum.Fit(30m);
        Assert.Equal(20m, sum.Mean());
    }

    [Fact] public void Mean_MixedNumbers()
    {
        var sum = new Sum<decimal>();
        sum.Fit(10m); sum.Fit(-20m); sum.Fit(30m);
        Assert.Equal(6.67m, sum.Mean(), 2);
    }
}

public class TestSumGenericMethods_MyNum : TestSumGenericMethods<MyNum>
{
    protected override MyNum From(int value) => new(value);

    [Fact] public void Mean_NoNumbers_Throws()
        => Assert.Throws<DivideByZeroException>(() => new Sum<MyNum>().Mean());

    [Fact] public void Mean_SingleNumber()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(42));
        Assert.Equal(new MyNum(42), sum.Mean());
    }

    [Fact] public void Mean_PositiveNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10)); sum.Fit(new MyNum(20)); sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(20), sum.Mean());
    }

    [Fact] public void Mean_NegativeNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(-10)); sum.Fit(new MyNum(-20)); sum.Fit(new MyNum(-30));
        Assert.Equal(new MyNum(-20), sum.Mean());
    }

    [Fact] public void Mean_MixedNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10)); sum.Fit(new MyNum(-20)); sum.Fit(new MyNum(30));
        Assert.Equal(6.67m, sum.Mean().Value, 2);
    }

    // The multiply-capable path: MyNum knows how to multiply itself by a count, so the
    // weighted fit asks the type for value times count in one step. MyNum stores its
    // numbers as decimals, so a product of whole numbers must come out exact.

    [Fact] public void DelegatePath_ExactCountProduct()
    {
        // Deliberately the same count and value the fallback tests use, so the paths can
        // be compared: 1,000,003 is odd with a mixed pattern of set bits, and one step of
        // the type's own multiplication must equal the exact product 5,000,015, which the
        // decimal storage holds exactly.
        const long count = 1_000_003L;
        var sum = new Sum<MyNum>();
        sum.Fit(From(5), count);
        Assert.Equal(count, sum.Nobs);
        Assert.Equal(5m * count, sum.Value.Value);
    }

    [Fact] public void DelegatePath_ZeroCountShortCircuit()
    {
        // Fit(value, 0) short-circuits BEFORE branching on MultiplyDelegate, so the state
        // is untouched on either path: Nobs stays 0, Value stays the additive identity.
        var sum = new Sum<MyNum>();
        sum.Fit(From(5), 0L);
        Assert.Equal(0L, sum.Nobs);
        Assert.Equal(0m, sum.Value.Value);
    }

    [Fact] public void DelegatePath_SingleCount()
    {
        // A count of one on this path must match a single ordinary fit, since multiplying
        // by one leaves the value unchanged.
        var sum = new Sum<MyNum>();
        sum.Fit(From(42), 1L);
        Assert.Equal(1L, sum.Nobs);
        Assert.Equal(42m, sum.Value.Value);
    }

    [Fact] public void CrossAgreement_DelegateEqualsFallback()
    {
        // The multiply-capable path (MyNum) and the doubling fallback used by the types
        // that can only add (MyBigAddOnlyNum stored as long, MyAddOnlyNum stored as int)
        // must agree exactly for the same exact whole-number observations, because every
        // product used here is exactly representable in all three storages. The checks
        // compare the stored sums and the observation counts. The counts are chosen to
        // cover a power of two, where the fallback adds one doubled block, an odd count
        // with a mixed set of bits, and a count of one. Products for floating-point-like
        // types are deliberately not pinned bit for bit here, because the way additions
        // group together would change their results.
        const long powerOfTwo = 1_048_576L;
        const long odd = 1_000_003L;

        var delegateSum = new Sum<MyNum>();
        delegateSum.Fit(From(3), powerOfTwo);
        var bigFallback = new Sum<MyBigAddOnlyNum>();
        bigFallback.Fit(new MyBigAddOnlyNum(3), powerOfTwo);
        Assert.Equal(powerOfTwo, bigFallback.Nobs);
        Assert.Equal(3m * powerOfTwo, delegateSum.Value.Value);
        Assert.Equal(delegateSum.Value.Value, (decimal)bigFallback.Value.Value);

        delegateSum = new Sum<MyNum>();
        delegateSum.Fit(From(-11), odd);
        bigFallback = new Sum<MyBigAddOnlyNum>();
        bigFallback.Fit(new MyBigAddOnlyNum(-11), odd);
        var smallFallback = new Sum<MyAddOnlyNum>();
        smallFallback.Fit(new MyAddOnlyNum(-11), odd); // int backing holds -11_000_033
        Assert.Equal(odd, bigFallback.Nobs);
        Assert.Equal(odd, smallFallback.Nobs);
        Assert.Equal(-11m * odd, delegateSum.Value.Value);
        Assert.Equal(delegateSum.Value.Value, (decimal)bigFallback.Value.Value);
        Assert.Equal(delegateSum.Value.Value, (decimal)smallFallback.Value.Value);

        delegateSum = new Sum<MyNum>();
        delegateSum.Fit(From(7), 1L);
        bigFallback = new Sum<MyBigAddOnlyNum>();
        bigFallback.Fit(new MyBigAddOnlyNum(7), 1L);
        Assert.Equal(1L, bigFallback.Nobs);
        Assert.Equal(delegateSum.Value.Value, (decimal)bigFallback.Value.Value);
    }
}

public class TestSumGenericMethods_MyAddOnlyNum : TestSumGenericMethods<MyAddOnlyNum>
{
    // MyAddOnlyNum has no Mean() extension — only Sum/Value tests from the base apply.
    protected override MyAddOnlyNum From(int value) => new(value);

    [Fact] public void FallbackFit_ModerateCount_ExactSum()
    {
        // The count 1,000,003 is odd with a mixed pattern of set bits, so the doubling
        // fallback walks a general route through the bits of the count and must still
        // produce the exact product of count and value: three million nine, which sits
        // comfortably inside the int-backed helper's storage.
        const long count = 1_000_003L;
        var sum = new Sum<MyAddOnlyNum>();
        sum.Fit(From(3), count);
        Assert.Equal(count, sum.Nobs);
        Assert.Equal(3L * count, (long)sum.Value.Value);
    }
}

public class TestSumGenericMethods_MyBigAddOnlyNum : TestSumGenericMethods<MyBigAddOnlyNum>
{
    // MyBigAddOnlyNum has no average extension, so only the sum and value tests from the
    // base apply here. Unlike the int-stored MyAddOnlyNum, it stores whole numbers as
    // longs, so a product like the largest long count times a value of one is still
    // exactly representable.
    protected override MyBigAddOnlyNum From(int value) => new(value);

    [Fact] public void FallbackFit_HugeCountCompletes()
    {
        // The fallback must handle huge counts without one iteration per observation:
        // fitting one with the largest long count finishes quickly and lands on exactly
        // that count both as the observation count and as the stored sum, which the long
        // storage represents exactly.
        var sum = new Sum<MyBigAddOnlyNum>();
        sum.Fit(From(1), long.MaxValue);
        Assert.Equal(long.MaxValue, sum.Nobs);
        Assert.Equal(long.MaxValue, sum.Value.Value);
    }

    // The doubling fallback path: types that cannot multiply get their weighted fit by
    // doubling partial products, so the work grows with the number of bits in the count.
    // The moderate exact product for MyAddOnlyNum is already pinned by the fact above,
    // and the cross-agreement test repeats it at negative eleven times the odd count.

    [Fact] public void FallbackPath_ExactCountProduct()
    {
        // Deliberately the same count and value as the multiply-capable test above, so
        // the fallback must produce the same exact product, 5,000,015, in long storage.
        const long count = 1_000_003L;
        var sum = new Sum<MyBigAddOnlyNum>();
        sum.Fit(From(5), count);
        Assert.Equal(count, sum.Nobs);
        Assert.Equal(5L * count, sum.Value.Value);
    }

    [Fact] public void FallbackPath_ZeroCountShortCircuit()
    {
        // Same shared guard as the delegate pin: count 0 returns before either path runs,
        // so the doubling loop never executes and the state stays at the identities.
        var sum = new Sum<MyBigAddOnlyNum>();
        sum.Fit(From(7), 0L);
        Assert.Equal(0L, sum.Nobs);
        Assert.Equal(0L, sum.Value.Value);
    }
}

