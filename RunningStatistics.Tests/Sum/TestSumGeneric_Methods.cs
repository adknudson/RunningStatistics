using System;
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
}

public class TestSumGenericMethods_MyAddOnlyNum : TestSumGenericMethods<MyAddOnlyNum>
{
    // MyAddOnlyNum has no Mean() extension — only Sum/Value tests from the base apply.
    protected override MyAddOnlyNum From(int value) => new(value);
}

