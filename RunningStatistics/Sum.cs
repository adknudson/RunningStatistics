using System.Collections.Generic;
using System.Numerics;

namespace RunningStatistics;

public class Sum<TObs> : IRunningStatistic<TObs, TObs, Sum<TObs>> where TObs : INumber<TObs>
{
    public long Nobs { get; protected set; }

    public TObs Value { get; protected set; } = TObs.Zero;

    
    
    public virtual void Fit(IEnumerable<TObs> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(TObs value)
    {
        Nobs++;
        Value += value;
    }

    public void Reset()
    {
        Nobs = 0;
        Value = TObs.Zero;
    }

    public void Merge(Sum<TObs> sum)
    {
        Nobs += sum.Nobs;
        Value += sum.Value;
    }

    public static Sum<TObs> Merge(Sum<TObs> first, Sum<TObs> second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public Sum<TObs> CloneEmpty()
    {
        return new Sum<TObs>();
    }

    public Sum<TObs> Clone()
    {
        return new Sum<TObs>()
        {
            Nobs = Nobs,
            Value = Value
        };
    }

    public override string ToString() => $"{typeof(Sum<TObs>)}(Σ={Value}, n={Nobs})";
}