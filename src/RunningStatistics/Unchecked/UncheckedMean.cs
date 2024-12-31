using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics.Unchecked;

/// <summary>
/// Track the average value of a stream of numbers.
/// </summary>
public sealed class UncheckedMean : RunningStatisticBase<double, UncheckedMean>
{
    private long _nobs;
    

    public double Value { get; private set; }


    protected override long GetNobs()
    {
        return _nobs;
    }

    public override void Fit(double value, long count)
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

    public override UncheckedMean CloneEmpty()
    {
        return new UncheckedMean();
    }
    
    public override void Merge(UncheckedMean other)
    {
        _nobs += other.Nobs;
        Value = Nobs == 0 ? 0 : Utils.Smooth(Value, other.Value, (double)other.Nobs / Nobs);
    }
    
    public static explicit operator double(UncheckedMean mean) => mean.Value;
    
    protected override string GetStatsString() => $"μ={Value}";
}