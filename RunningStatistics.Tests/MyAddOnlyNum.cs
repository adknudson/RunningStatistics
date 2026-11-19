using System;
using System.Numerics;

namespace RunningStatistics.Tests;

/// <summary>
/// A minimal numeric type that supports only addition and additive identity, so it does not
/// implement the multiply operators. It exercises the counting fallback path of
/// <see cref="Sum{TObs}" />, the one used when no multiply by a count is available.
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
