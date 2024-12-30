using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics.UnsafeStatistics;

/// <summary>
/// Track the univariate mean. Same as <see cref="Mean"/>, but does no checks to ensure that the observation is a valid
/// value (i.e. finite).
/// </summary>
public sealed class UnsafeMean : RunningStatisticBase<double, UnsafeMean>
{
    private long _nobs;
    

    public double Value { get; private set; }


    protected override long GetNobs()
    {
        return _nobs;
    }

    public override void Fit(double value)
    {
        Fit(value, 1);
    }

    public void Fit(double value, long count)
    {
        _nobs += count;
        Value = Utils.Smooth(Value, value, (double)count / Nobs);
    }

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        _nobs += ys.Count;
        Value = Utils.Smooth(Value, ys.Average(), (double)ys.Count / Nobs);
    }

    public void Fit(IList<double> values)
    {
        _nobs += values.Count;
        Value = Utils.Smooth(Value, values.Average(), (double)values.Count / Nobs);
    }
    
    public override void Reset()
    {
        _nobs = 0;
        Value = 0;
    }

    public override UnsafeMean CloneEmpty()
    {
        return new UnsafeMean();
    }

    public override UnsafeMean Clone()
    {
        return new UnsafeMean
        {
            _nobs = Nobs,
            Value = Value
        };
    }

    public override void Merge(UnsafeMean other)
    {
        _nobs += other.Nobs;
        Value = Nobs == 0 ? 0 : Utils.Smooth(Value, other.Value, (double)other.Nobs / Nobs);
    }
    
    public static explicit operator double(UnsafeMean mean) => mean.Value;
    
    public override string ToString() => $"{typeof(UnsafeMean)} Nobs={Nobs} | μ={Value}";
}