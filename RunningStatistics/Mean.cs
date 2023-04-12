using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean, stored as a <see cref="double"/>.
/// </summary>
public sealed class Mean : AbstractRunningStatistic<double, Mean>
{
    private double _value;
    
    
    
    public double Value
    {
        get => Nobs == 0 ? double.NaN : _value;
        private set => _value = value;
    }

    
    
    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Nobs += ys.Count;
        Value = Utils.Smooth(Value, ys.Average(), (double) ys.Count / Nobs);
    }

    public override void Fit(double value)
    {
        Nobs++;
        Value = Utils.Smooth(Value, value, 1.0 / Nobs);
    }

    public override void Reset()
    {
        Nobs = 0;
        Value = 0;
    }

    public override Mean CloneEmpty()
    {
        return new Mean();
    }

    public override Mean Clone()
    {
        return new Mean
        {
            Nobs = Nobs,
            Value = Value
        };
    }
    public override void Merge(Mean mean)
    {
        Nobs += mean.Nobs;
        Value = Nobs == 0 ? 0 : Utils.Smooth(Value, mean.Value, (double) mean.Nobs / Nobs);
    }
    
    public override string ToString() => $"{typeof(Mean)} Nobs={Nobs} | μ={Value}";

    public static explicit operator double(Mean mean) => mean.Value;
}