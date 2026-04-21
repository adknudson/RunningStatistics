using System;
using System.Numerics;

namespace RunningStatistics.Tests;

/// <summary>
/// A minimal numeric type that supports only addition and additive identity —
/// i.e. it does NOT implement <see cref="IMultiplyOperators{TSelf,TOther,TResult}"/>.
/// Used to exercise the O(count) fallback path in <see cref="Sum{TObs}"/>.
/// </summary>
public readonly struct MyAddOnlyNum(int value) :
    IAdditionOperators<MyAddOnlyNum, MyAddOnlyNum, MyAddOnlyNum>,
    IAdditiveIdentity<MyAddOnlyNum, MyAddOnlyNum>,
    IEquatable<MyAddOnlyNum>
{
    public int Value { get; } = value;

    public static MyAddOnlyNum AdditiveIdentity => new(0);

    public static MyAddOnlyNum operator +(MyAddOnlyNum left, MyAddOnlyNum right) => new(left.Value + right.Value);

    public bool Equals(MyAddOnlyNum other) => Value == other.Value;
    public override bool Equals(object obj) => obj is MyAddOnlyNum other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(MyAddOnlyNum left, MyAddOnlyNum right) => left.Equals(right);
    public static bool operator !=(MyAddOnlyNum left, MyAddOnlyNum right) => !left.Equals(right);

    public override string ToString() => Value.ToString();
}
