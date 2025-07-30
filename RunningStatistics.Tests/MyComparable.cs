using System;
using System.Collections.Generic;

namespace RunningStatistics.Tests;

public class MyComparable(decimal value) : IComparable<MyComparable>
{
    private readonly decimal _value = value;
    
    public int CompareTo(MyComparable other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return -1;
        return _value.CompareTo(other._value);
    }
    public static bool operator <(MyComparable left, MyComparable right) => Comparer<MyComparable>.Default.Compare(left, right) < 0;
    public static bool operator >(MyComparable left, MyComparable right) => Comparer<MyComparable>.Default.Compare(left, right) > 0;
    public static bool operator <=(MyComparable left, MyComparable right) => Comparer<MyComparable>.Default.Compare(left, right) <= 0;
    public static bool operator >=(MyComparable left, MyComparable right) => Comparer<MyComparable>.Default.Compare(left, right) >= 0;

    public override string ToString() => $"{nameof(MyComparable)}({_value})";
}