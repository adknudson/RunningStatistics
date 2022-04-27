using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean, stored as a <see cref="double"/>.
/// </summary>
public class Mean : IRunningStatistic<double>
{
    private double _value;


    public Mean()
    {
        Count = 0;
        Value = 0;
    }
    

    public long Count { get; private set; }

    public double Value
    {
        get => Count == 0 ? double.NaN : _value;
        private set => _value = value;
    }


    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Mean mean) return;
        
        Count += mean.Count;
        Value = Count == 0 ? 0 : Utils.Smooth(Value, mean.Value, (double) mean.Count / Count);
    }

    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Count += ys.Count;
        Value = Utils.Smooth(Value, ys.Average(), (double) ys.Count / Count);
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
    
    public static explicit operator double(Mean value) => value.Value;
}