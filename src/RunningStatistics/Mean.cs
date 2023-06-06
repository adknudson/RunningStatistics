using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean, stored as a <see cref="double"/>.
/// </summary>
public sealed class Mean : AbstractRunningStatistic<double, Mean>
{
    private double _value;
    
    
    
    public double Value => Nobs == 0 ? double.NaN : _value;



    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Nobs += ys.Count;
        _value = Utils.Smooth(_value, ys.Average(), (double) ys.Count / Nobs);
    }

    public override void Fit(double value)
    {
        Nobs++;
        _value = Utils.Smooth(_value, value, 1.0 / Nobs);
    }

    public override void Reset()
    {
        Nobs = 0;
        _value = 0;
    }

    public override Mean CloneEmpty() => new();

    public override Mean Clone()
    {
        return new Mean
        {
            Nobs = Nobs,
            _value = Value
        };
    }
    public override void Merge(Mean mean)
    {
        Nobs += mean.Nobs;
        _value = Nobs == 0 ? 0 : Utils.Smooth(_value, mean._value, (double) mean.Nobs / Nobs);
    }
    
    public override string ToString() => $"{typeof(Mean)} Nobs={Nobs} | μ={Value}";

    public static explicit operator double(Mean mean) => mean.Value;
}