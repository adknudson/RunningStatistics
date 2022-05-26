using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the overall sum, stored as a <see cref="double"/>.
/// </summary>
public class Sum : IRunningStatistic<double>
{
    public Sum()
    {
        Value = 0;
        Count = 0;
    }

    public Sum(Sum other)
    {
        Value = other.Value;
        Count = other.Count;
    }
    

    public long Count { get; private set; }
    public double Value { get; private set; }
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

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Sum sum) return;
        
        Count += sum.Count;
        Value += sum.Value;
    }

    public static Sum Merge(Sum a, Sum b)
    {
        var c = new Sum(a);
        c.Merge(b);
        return c;
    }

    public void Reset()
    {
        Count = 0;
        Value = 0;
    }

    public static implicit operator double(Sum value) => value.Value;
    
    public override string ToString() => $"{typeof(Sum)}(Σ={Value}, n={Count})";

    public void Print(StreamWriter stream)
    {
        stream.WriteLine($"{GetType()}(Σ={Value}, n={Count})");
    }
}