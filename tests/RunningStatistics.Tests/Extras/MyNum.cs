using System;
using System.Globalization;
using System.Numerics;

namespace RunningStatistics.Tests.Extras;

public readonly struct MyNum : 
    IComparable<MyNum>, 
    IComparisonOperators<MyNum, MyNum, bool>,
    IConvertible, 
    IEquatable<MyNum>,
    IFormattable,
    IParsable<MyNum>,
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
    public MyNum(decimal value)
    {
        Value = value;
    }


    public decimal Value { get; }

    public static MyNum AdditiveIdentity { get; } = new(0);
    public static MyNum Zero { get; } = new(0);
    public static MyNum MultiplicativeIdentity { get; } = new(1);
    public static MyNum One { get; } = new(1);
    public static MyNum MaxValue { get; } = new(decimal.MaxValue);
    public static MyNum MinValue { get; } = new(decimal.MinValue);
    

    public int CompareTo(MyNum other) => Value.CompareTo(other.Value);
    public static bool operator >(MyNum left, MyNum right) => left.CompareTo(right) > 0;
    public static bool operator >=(MyNum left, MyNum right) => left.CompareTo(right) >= 0;
    public static bool operator <(MyNum left, MyNum right) => left.CompareTo(right) < 0;
    public static bool operator <=(MyNum left, MyNum right) => left.CompareTo(right) <= 0;
    
    
    public TypeCode GetTypeCode() => TypeCode.Decimal;
    bool IConvertible.ToBoolean(IFormatProvider provider) => Convert.ToBoolean(Value, provider);
    byte IConvertible.ToByte(IFormatProvider provider) => Convert.ToByte(Value, provider);
    char IConvertible.ToChar(IFormatProvider provider) => Convert.ToChar(Value, provider);
    DateTime IConvertible.ToDateTime(IFormatProvider provider) => Convert.ToDateTime(Value, provider);
    decimal IConvertible.ToDecimal(IFormatProvider provider) => Convert.ToDecimal(Value, provider);
    double IConvertible.ToDouble(IFormatProvider provider) => Convert.ToDouble(Value, provider);
    short IConvertible.ToInt16(IFormatProvider provider) => Convert.ToInt16(Value, provider);
    int IConvertible.ToInt32(IFormatProvider provider) => Convert.ToInt32(Value, provider);
    long IConvertible.ToInt64(IFormatProvider provider) => Convert.ToInt64(Value, provider);
    sbyte IConvertible.ToSByte(IFormatProvider provider) => Convert.ToSByte(Value, provider);
    float IConvertible.ToSingle(IFormatProvider provider) => Convert.ToSingle(Value, provider);
    string IConvertible.ToString(IFormatProvider provider) => Convert.ToString(Value, provider);
    ushort IConvertible.ToUInt16(IFormatProvider provider) => Convert.ToUInt16(Value, provider);
    uint IConvertible.ToUInt32(IFormatProvider provider) => Convert.ToUInt32(Value, provider);
    ulong IConvertible.ToUInt64(IFormatProvider provider) => Convert.ToUInt64(Value, provider);
    object IConvertible.ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(Value, conversionType, provider);

    
    public bool Equals(MyNum other) => Value == other.Value;
    public override bool Equals(object obj) => obj is MyNum other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(MyNum left, MyNum right) => left.Equals(right);
    public static bool operator !=(MyNum left, MyNum right) => !left.Equals(right);
    
    
    public string ToString(string format, IFormatProvider provider = null) => Value.ToString(format, provider);

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }


    public static MyNum Parse(string s, IFormatProvider provider = null) => new(decimal.Parse(s, provider));

    public static bool TryParse(string s, IFormatProvider provider, out MyNum result)
    {
        if (decimal.TryParse(s, provider, out var value))
        {
            result = new MyNum(value);
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryParse(string s, out MyNum result)
    {
        if (decimal.TryParse(s, out var value))
        {
            result = new MyNum(value);
            return true;
        }

        result = default;
        return false;
    }
    
    
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
}