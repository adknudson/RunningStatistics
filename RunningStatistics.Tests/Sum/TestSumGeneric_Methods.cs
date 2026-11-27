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
// Concrete subclasses, one per supported type
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
    // float has no Mean() extension, so only Sum/Value tests from the base apply.
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

    // SumExtensions has no average for a custom observation type, so these facts write the
    // average the caller would write: the stored total divided by the observation count, in
    // MyNum's own arithmetic. MyNum divides by a long, and it stores decimals, so an empty
    // sum divides zero by zero the decimal way and throws, the same way Sum<decimal> does.
    [Fact] public void Mean_NoNumbers_Throws()
    {
        var sum = new Sum<MyNum>();
        Assert.Throws<DivideByZeroException>(() => sum.Value / sum.Nobs);
    }

    [Fact] public void Mean_SingleNumber()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(42));
        Assert.Equal(new MyNum(42), sum.Value / sum.Nobs);
    }

    [Fact] public void Mean_PositiveNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10)); sum.Fit(new MyNum(20)); sum.Fit(new MyNum(30));
        Assert.Equal(new MyNum(20), sum.Value / sum.Nobs);
    }

    [Fact] public void Mean_NegativeNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(-10)); sum.Fit(new MyNum(-20)); sum.Fit(new MyNum(-30));
        Assert.Equal(new MyNum(-20), sum.Value / sum.Nobs);
    }

    [Fact] public void Mean_MixedNumbers()
    {
        var sum = new Sum<MyNum>();
        sum.Fit(new MyNum(10)); sum.Fit(new MyNum(-20)); sum.Fit(new MyNum(30));
        Assert.Equal(6.67m, (sum.Value / sum.Nobs).Value, 2);
    }

    // This fact covers the multiply-capable path. MyNum knows how to multiply itself by a
    // count, so the weighted fit asks the type for value times count in one step. MyNum
    // stores its numbers as decimals, so a product of whole numbers must come out exact.

    [Fact] public void DelegatePath_ExactCountProduct()
    {
        // The fact uses the same count and value the fallback tests use, so a reader can
        // compare the two paths. The count 1,000,003 is odd with a mixed pattern of set bits.
        // One step of the type's own multiplication must equal the exact product 5,000,015,
        // which the decimal storage holds exactly.
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
        // This fact compares the multiply-capable path of MyNum with the doubling fallback
        // that the add-only types use. MyBigAddOnlyNum stores whole numbers as long, and
        // MyAddOnlyNum stores them as int. Both paths must agree exactly for the same exact
        // whole-number observations, because every product used here is exactly
        // representable in all three storages. The checks compare the stored sums and the
        // observation counts. The counts cover three shapes. One is a power of two, where
        // the fallback adds one doubled block. One is an odd count with a mixed set of
        // bits. One is a count of one. The fact deliberately does not pin the products of
        // floating-point types bit for bit. The grouping of additions would change those
        // results.
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
    // MyAddOnlyNum has no Mean() extension, so only Sum/Value tests from the base apply.
    protected override MyAddOnlyNum From(int value) => new(value);

    [Fact] public void FallbackFit_ModerateCount_ExactSum()
    {
        // The count 1,000,003 is odd with a mixed pattern of set bits. The doubling fallback
        // therefore walks a general route through the bits of the count. It must still
        // produce the exact product of count and value, three million nine. That value sits
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
    // MyBigAddOnlyNum has no average extension, so only the sum and value tests from the base
    // apply here. Unlike MyAddOnlyNum, which stores whole numbers as int, it stores them as
    // longs. A product like the largest long count times a value of one is therefore still
    // exactly representable.
    protected override MyBigAddOnlyNum From(int value) => new(value);

    [Fact] public void FallbackFit_HugeCountCompletes()
    {
        // The fallback must handle huge counts without one iteration per observation. Fitting a
        // value with the largest long count finishes quickly and lands on exactly that count.
        // That count is both the observation count and the stored sum, which the long storage
        // represents exactly.
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
        // The fact uses the same shared guard as the delegate pin. A count of 0 returns before
        // either path runs, so the doubling loop never executes and the state stays at the
        // identities.
        var sum = new Sum<MyBigAddOnlyNum>();
        sum.Fit(From(7), 0L);
        Assert.Equal(0L, sum.Nobs);
        Assert.Equal(0L, sum.Value.Value);
    }
}

