using System;
using System.Globalization;
using System.Numerics;

namespace RunningStatistics.Tests;

internal readonly struct MyNum(decimal value) :
    IComparable<MyNum>,
    IComparisonOperators<MyNum, MyNum, bool>,
    IEquatable<MyNum>,
    IAdditiveIdentity<MyNum, MyNum>,
    IAdditionOperators<MyNum, MyNum, MyNum>,
    ISubtractionOperators<MyNum, MyNum, MyNum>,
    IMultiplicativeIdentity<MyNum, MyNum>,
    IMultiplyOperators<MyNum, MyNum, MyNum>,
    IMultiplyOperators<MyNum, long, MyNum>,
    IDivisionOperators<MyNum, MyNum, MyNum>,
    IDivisionOperators<MyNum, int, MyNum>,
    IDivisionOperators<MyNum, long, MyNum>,
    IUnaryPlusOperators<MyNum, MyNum>,
    IUnaryNegationOperators<MyNum, MyNum>,
    IMinMaxValue<MyNum>
{
    public decimal Value { get; } = value;

    public static MyNum AdditiveIdentity { get; } = new(0);
    public static MyNum MultiplicativeIdentity { get; } = new(1);
    public static MyNum MaxValue { get; } = new(decimal.MaxValue);
    public static MyNum MinValue { get; } = new(decimal.MinValue);
    

    public int CompareTo(MyNum other) => Value.CompareTo(other.Value);
    public static bool operator >(MyNum left, MyNum right) => left.CompareTo(right) > 0;
    public static bool operator >=(MyNum left, MyNum right) => left.CompareTo(right) >= 0;
    public static bool operator <(MyNum left, MyNum right) => left.CompareTo(right) < 0;
    public static bool operator <=(MyNum left, MyNum right) => left.CompareTo(right) <= 0;
    
    
    public bool Equals(MyNum other) => Value == other.Value;
    public override bool Equals(object obj) => obj is MyNum other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(MyNum left, MyNum right) => left.Equals(right);
    public static bool operator !=(MyNum left, MyNum right) => !left.Equals(right);


    public static MyNum operator +(MyNum left, MyNum right) => new(left.Value + right.Value);
    public static MyNum operator +(MyNum value) => new(value.Value);
    public static MyNum operator -(MyNum left, MyNum right) => new(left.Value - right.Value);
    public static MyNum operator -(MyNum value) => new(-value.Value);
    public static MyNum operator *(MyNum left, MyNum right) => new(left.Value * right.Value);
    public static MyNum operator *(MyNum left, long right) => new(left.Value * right);
    public static MyNum operator /(MyNum left, MyNum right) => new(left.Value / right.Value);
    public static MyNum operator /(MyNum left, int right) => new(left.Value / right);
    public static MyNum operator /(MyNum left, long right) => new(left.Value / right);


    public static explicit operator MyNum(int value) => new(value);
    public static explicit operator MyNum(long value) => new(value);
    public static explicit operator MyNum(double value) => new((decimal) value);
    public static explicit operator MyNum(decimal value) => new(value);
    
    
    public string ToString(string format, IFormatProvider provider = null) => Value.ToString(format, provider);
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
