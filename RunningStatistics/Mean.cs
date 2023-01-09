using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean, stored as a <see cref="double"/>.
/// </summary>
public class Mean : IRunningStatistic<double, double, Mean>
{
    private double _value;


    
    public Mean()
    {
        Nobs = 0;
        Value = 0;
    }

    
    
    public long Nobs { get; private set; }

    public double Value
    {
        get => Nobs == 0 ? double.NaN : _value;
        private set => _value = value;
    }

    
    
    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Nobs += ys.Count;
        Value = Utils.Smooth(Value, ys.Average(), (double) ys.Count / Nobs);
    }

    public void Fit(double value)
    {
        Nobs++;
        Value = Utils.Smooth(Value, value, 1.0 / Nobs);
    }

    public void Merge(Mean mean)
    {
        Nobs += mean.Nobs;
        Value = Nobs == 0 ? 0 : Utils.Smooth(Value, mean.Value, (double) mean.Nobs / Nobs);
    }

    public static Mean Merge(Mean first, Mean second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public void Reset()
    {
        Nobs = 0;
        Value = 0;
    }

    public Mean CloneEmpty()
    {
        return new Mean();
    }

    public Mean Clone()
    {
        return new Mean()
        {
            Nobs = Nobs,
            Value = Value,
        };
    }
    

    
    public override string ToString() => $"{typeof(Mean)}(μ={Value}, n={Nobs})";
}