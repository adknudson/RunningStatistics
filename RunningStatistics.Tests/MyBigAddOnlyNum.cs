using System;
using System.Numerics;

namespace RunningStatistics.Tests;

/// <summary>
/// A long-backed numeric type that supports only addition and additive identity, so it does not
/// implement the multiply operators. Unlike <see cref="MyAddOnlyNum" />, whose int backing cannot
/// hold products of huge counts, this type's long backing lets the counting fallback path of
/// <see cref="Sum{TObs}" /> be tested at counts up to <c>long.MaxValue</c> with exact results,
/// for example fitting the value one with a count of <c>long.MaxValue</c>.
/// </summary>
public readonly struct MyBigAddOnlyNum(long value) :
    IAdditionOperators<MyBigAddOnlyNum, MyBigAddOnlyNum, MyBigAddOnlyNum>,
    IAdditiveIdentity<MyBigAddOnlyNum, MyBigAddOnlyNum>,
    IEquatable<MyBigAddOnlyNum>
{
    public long Value { get; } = value;

    public static MyBigAddOnlyNum AdditiveIdentity => new(0);

    public static MyBigAddOnlyNum operator +(MyBigAddOnlyNum left, MyBigAddOnlyNum right) => new(left.Value + right.Value);

    public bool Equals(MyBigAddOnlyNum other) => Value == other.Value;
    public override bool Equals(object obj) => obj is MyBigAddOnlyNum other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(MyBigAddOnlyNum left, MyBigAddOnlyNum right) => left.Equals(right);
    public static bool operator !=(MyBigAddOnlyNum left, MyBigAddOnlyNum right) => !left.Equals(right);

    public override string ToString() => Value.ToString();
}
