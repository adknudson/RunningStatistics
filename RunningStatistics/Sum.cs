using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the overall sum, stored as a <see cref="double"/>.
/// </summary>
public class Sum : IRunningStatistic<double, Sum>
{
    public Sum()
    {
        Value = 0;
        Count = 0;
    }

    private Sum(Sum other)
    {
        Count = other.Count;
        Value = other.Value;
    }
    
    
    
    public long Count { get; private set; }
    private double Value { get; set; }
    public double Mean => Value / Count;


    public void Fit(double value)
    {
        Count += 1;
        Value += value;
    }

    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Count += ys.Count;
        Value += ys.Sum();
    }

    public void Merge(Sum other)
    {
        Count += other.Count;
        Value += other.Value;
    }

    public void Reset()
    {
        Count = 0;
        Value = 0;
    }

    public void Print(StreamWriter stream)
    {
        stream.WriteLine(ToString());
    }

    public override string ToString() => $"{typeof(Sum)}(Σ={Value}, n={Count})";

    private static Sum Merge(Sum a, Sum b)
    {
        var merged = new Sum(a);
        merged.Merge(b);
        return merged;
    }
    public static Sum operator +(Sum a, Sum b) => Merge(a, b);
    public static explicit operator double(Sum value) => value.Value;
}