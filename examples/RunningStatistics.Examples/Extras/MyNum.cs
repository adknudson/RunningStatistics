using System.Globalization;
using System.Numerics;

namespace RunningStatistics.Examples.Extras;

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
    private readonly decimal _value;
    
    
    public MyNum(decimal value)
    {
        _value = value;
    }

    
    public static MyNum AdditiveIdentity { get; } = new(0);
    public static MyNum Zero { get; } = new(0);
    public static MyNum MultiplicativeIdentity { get; } = new(1);
    public static MyNum One { get; } = new(1);
    public static MyNum MaxValue { get; } = new(decimal.MaxValue);
    public static MyNum MinValue { get; } = new(decimal.MinValue);
    

    public int CompareTo(MyNum other) => _value.CompareTo(other._value);
    public static bool operator >(MyNum left, MyNum right) => left.CompareTo(right) > 0;
    public static bool operator >=(MyNum left, MyNum right) => left.CompareTo(right) >= 0;
    public static bool operator <(MyNum left, MyNum right) => left.CompareTo(right) < 0;
    public static bool operator <=(MyNum left, MyNum right) => left.CompareTo(right) <= 0;
    
    
    public TypeCode GetTypeCode() => TypeCode.Decimal;
    bool IConvertible.ToBoolean(IFormatProvider provider) => Convert.ToBoolean(_value, provider);
    byte IConvertible.ToByte(IFormatProvider provider) => Convert.ToByte(_value, provider);
    char IConvertible.ToChar(IFormatProvider provider) => Convert.ToChar(_value, provider);
    DateTime IConvertible.ToDateTime(IFormatProvider provider) => Convert.ToDateTime(_value, provider);
    decimal IConvertible.ToDecimal(IFormatProvider provider) => Convert.ToDecimal(_value, provider);
    double IConvertible.ToDouble(IFormatProvider provider) => Convert.ToDouble(_value, provider);
    short IConvertible.ToInt16(IFormatProvider provider) => Convert.ToInt16(_value, provider);
    int IConvertible.ToInt32(IFormatProvider provider) => Convert.ToInt32(_value, provider);
    long IConvertible.ToInt64(IFormatProvider provider) => Convert.ToInt64(_value, provider);
    sbyte IConvertible.ToSByte(IFormatProvider provider) => Convert.ToSByte(_value, provider);
    float IConvertible.ToSingle(IFormatProvider provider) => Convert.ToSingle(_value, provider);
    string IConvertible.ToString(IFormatProvider provider) => Convert.ToString(_value, provider);
    ushort IConvertible.ToUInt16(IFormatProvider provider) => Convert.ToUInt16(_value, provider);
    uint IConvertible.ToUInt32(IFormatProvider provider) => Convert.ToUInt32(_value, provider);
    ulong IConvertible.ToUInt64(IFormatProvider provider) => Convert.ToUInt64(_value, provider);
    object IConvertible.ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(_value, conversionType, provider);

    
    public bool Equals(MyNum other) => _value == other._value;
    public override bool Equals(object obj) => obj is MyNum other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(MyNum left, MyNum right) => left.Equals(right);
    public static bool operator !=(MyNum left, MyNum right) => !left.Equals(right);
    
    
    public string ToString(string format, IFormatProvider provider = null) => _value.ToString(format, provider);

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
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
    
    
    public static MyNum operator +(MyNum left, MyNum right) => new(left._value + right._value);
    public static MyNum operator +(MyNum value) => new(value._value);
    public static MyNum operator -(MyNum left, MyNum right) => new(left._value - right._value);
    public static MyNum operator -(MyNum value) => new(-value._value);
    public static MyNum operator *(MyNum left, MyNum right) => new(left._value * right._value);
    public static MyNum operator *(MyNum left, long right) => new(left._value * right);
    public static MyNum operator /(MyNum left, MyNum right) => new(left._value / right._value);
    public static MyNum operator /(MyNum left, int right) => new(left._value / right);
    public static MyNum operator /(MyNum left, long right) => new(left._value / right);


    public static explicit operator MyNum(int value) => new(value);
    public static explicit operator MyNum(long value) => new(value);
    public static explicit operator MyNum(double value) => new((decimal) value);
    public static explicit operator MyNum(decimal value) => new(value);
}