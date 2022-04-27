using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean, stored as a <see cref="double"/>.
/// </summary>
public class Mean : IRunningStatistic<double, Mean>
{
    private double _value;

    
    
    public Mean()
    {
        Count = 0;
        Value = 0;
    }

    private Mean(Mean other)
    {
        Count = other.Count;
        Value = other.Value;
    }
    
    
    
    public long Count { get; private set; }

    public double Value
    {
        get => Count == 0 ? double.NaN : _value;
        private set => _value = value;
    }
    
    
    
    public void Merge(Mean other)
    {
        Count += other.Count;
        Value = Count == 0 ? 0 : Utils.Smooth(Value, other.Value, (double) other.Count / Count);
    }

    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Count += ys.Count;
        Value = Utils.Smooth(Value, ys.Average(), (double)ys.Count / Count);
    }

    public void Fit(double value)
    {
        Count++;
        Value = Utils.Smooth(Value, value, 1.0 / Count);
    }

    public void Reset()
    {
        Count = 0;
        Value = 0;
    }

    public override string ToString() => $"{typeof(Mean)}(μ={Value}, n={Count})";
    
    private static Mean Merge(Mean a, Mean b)
    {
        Mean merged = new(a);
        merged.Merge(b);
        return merged;
    }
    
    public static explicit operator double(Mean value) => value.Value;
}