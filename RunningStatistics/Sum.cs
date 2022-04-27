using System.Collections.Generic;
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

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Sum sum) return;
        
        Count += sum.Count;
        Value += sum.Value;
    }

    public void Reset()
    {
        Count = 0;
        Value = 0;
    }

    public override string ToString() => $"{typeof(Sum)}(Σ={Value}, n={Count})";
    
    public static explicit operator double(Sum value) => value.Value;
}