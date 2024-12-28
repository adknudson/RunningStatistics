using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics.UnsafeStatistics;

/// <summary>
/// Track the univariate mean. Same as <see cref="Mean"/>, but does no checks to ensure that the observation is a valid
/// value (i.e. finite).
/// </summary>
public sealed class UnsafeMean : IRunningStatistic<double, UnsafeMean>
{
    public long Nobs { get; private set; }
    
    public double Value { get; private set; }
    
    
    public void Fit(double value)
    {
        Fit(value, 1);
    }

    public void Fit(double value, int count)
    {
        Nobs += count;
        Value = Utils.Smooth(Value, value, (double)count / Nobs);
    }

    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        Nobs += ys.Count;
        Value = Utils.Smooth(Value, ys.Average(), (double)ys.Count / Nobs);
    }

    public void Fit(IList<double> values)
    {
        Nobs += values.Count;
        Value = Utils.Smooth(Value, values.Average(), (double)values.Count / Nobs);
    }
    
    public void Reset()
    {
        Nobs = 0;
        Value = 0;
    }

    public UnsafeMean CloneEmpty()
    {
        return new UnsafeMean();
    }

    public UnsafeMean Clone()
    {
        return new UnsafeMean
        {
            Nobs = Nobs,
            Value = Value
        };
    }

    public void Merge(UnsafeMean other)
    {
        Nobs += other.Nobs;
        Value = Nobs == 0 ? 0 : Utils.Smooth(Value, other.Value, (double)other.Nobs / Nobs);
    }

    IRunningStatistic<double> IRunningStatistic<double>.CloneEmpty()
    {
        return CloneEmpty();
    }

    IRunningStatistic<double> IRunningStatistic<double>.Clone()
    {
        return Clone();
    }

    public void Merge(IRunningStatistic<double> other)
    {
        Merge((UnsafeMean)other);
    }

    public static explicit operator double(UnsafeMean mean) => mean.Value;
    
    public override string ToString() => $"{typeof(UnsafeMean)} Nobs={Nobs} | μ={Value}";
}