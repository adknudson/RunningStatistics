using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class Sum : RunningStatisticBase<double, Sum>
{
    private long _nobs;
    
    
    public double Value { get; private set; }


    protected override long GetNobs() => _nobs;

    public override void Fit(double value)
    {
        _nobs++;
        Value += value;
    }

    public override void Fit(IEnumerable<double> values)
    {
        var xs = values.ToList();
        _nobs += xs.Count;
        Value += xs.Sum();
    }

    public override void Reset()
    {
        _nobs = 0;
        Value = 0;
    }

    public override Sum CloneEmpty() => new();

    public override Sum Clone()
    {
        return new Sum
        {
            _nobs = Nobs,
            Value = Value
        };
    }
    public override void Merge(Sum sum)
    {
        _nobs += sum.Nobs;
        Value += sum.Value;
    }

    public double Mean() => Value / Nobs;

    public static explicit operator double(Sum sum) => sum.Value;

    public override string ToString() => $"{typeof(Sum)} Nobs={Nobs} | Σ={Value}";
}

