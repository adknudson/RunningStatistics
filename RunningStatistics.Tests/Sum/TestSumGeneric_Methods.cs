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

    // Commit 24: null iterable Fit contract — Sum<TObs> has no iterable Fit override,
    // so the call resolves to the base RunningStatisticBase.Fit(IEnumerable<TObs>).
    [Fact]
    public void Fit_RejectsNullIterable()
    {
        var sum = new Sum<TObs>();
        Assert.Throws<ArgumentNullException>(() => sum.Fit((IEnumerable<TObs>?)null));
        Assert.Equal(0L, sum.Nobs);
    }
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

    // --- Commit 12 pins: the DELEGATE path (MyNum implements IMultiplyOperators<MyNum,long,MyNum>,
    // so Fit(value,count) computes value * count in O(1) via the compiled multiply delegate).
    // MyNum is decimal-backed so exact-integer products must come out exact.

    [Fact] public void DelegatePath_ExactCountProduct()
    {
        // Same count/value shape as the fallback pins: 1_000_003 is odd with mixed bits
        // (19,18,17,16,14,9,6,1,0 set). The delegate's value * count must equal the exact
        // mathematical product: 5 * 1_000_003 = 5_000_015, exact in the decimal backing.
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
        // count = 1 on the delegate path must equal one Fit(value): value * 1 is exact.
        var sum = new Sum<MyNum>();
        sum.Fit(From(42), 1L);
        Assert.Equal(1L, sum.Nobs);
        Assert.Equal(42m, sum.Value.Value);
    }

    [Fact] public void CrossAgreement_DelegateEqualsFallback()
    {
        // Pin: the delegate path (MyNum, via IMultiplyOperators<MyNum,long,MyNum>) and the
        // O(log count) doubling fallback (MyBigAddOnlyNum long-backed / MyAddOnlyNum int-backed)
        // must AGREE EXACTLY for identical exact-integer observations — every product below is
        // exactly representable in all three backings. Compares the underlying .Value (decimal
        // vs (decimal)long/(decimal)int) and Nobs. Counts cover a power of two (2^20 — the
        // fallback adds a single doubled block), an odd mixed-bit count, and count = 1.
        // (Not pinned bit-exact for floating-like types — associativity caveat, commit 11.)
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
        // 1_000_003 is odd with an interesting bit pattern (bits 19,18,17,16,14,9,6,1,0 set):
        // the O(log count) doubling fallback must produce exactly count * value for the
        // integer-backed helper. int backing: 3 * 1_000_003 = 3_000_009 fits exactly.
        const long count = 1_000_003L;
        var sum = new Sum<MyAddOnlyNum>();
        sum.Fit(From(3), count);
        Assert.Equal(count, sum.Nobs);
        Assert.Equal(3L * count, (long)sum.Value.Value);
    }
}

public class TestSumGenericMethods_MyBigAddOnlyNum : TestSumGenericMethods<MyBigAddOnlyNum>
{
    // MyBigAddOnlyNum has no Mean() extension — only Sum/Value tests from the base apply.
    // long-backed (unlike int-backed MyAddOnlyNum) so count * value is exactly representable
    // even at count = long.MaxValue with value = 1.
    protected override MyBigAddOnlyNum From(int value) => new(value);

    [Fact] public void FallbackFit_HugeCountCompletes()
    {
        // The fallback (no IMultiplyOperators delegate) must handle huge counts without
        // iterating count times: this completes fast with the exact value count * value
        // (v = 1, long-backed so 1 * long.MaxValue is exact) and Nobs == long.MaxValue.
        var sum = new Sum<MyBigAddOnlyNum>();
        sum.Fit(From(1), long.MaxValue);
        Assert.Equal(long.MaxValue, sum.Nobs);
        Assert.Equal(long.MaxValue, sum.Value.Value);
    }

    // --- Commit 12 pins: the FALLBACK path (no IMultiplyOperators -> O(log count) doubling).
    // (MyAddOnlyNum's exact moderate-count product is already pinned by
    // FallbackFit_ModerateCount_ExactSum; the cross-agreement test adds it at -11 * odd.)

    [Fact] public void FallbackPath_ExactCountProduct()
    {
        // Same count/value as DelegatePath_ExactCountProduct: the doubling fallback must
        // produce the exact product 5 * 1_000_003 = 5_000_015 in the long backing.
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

