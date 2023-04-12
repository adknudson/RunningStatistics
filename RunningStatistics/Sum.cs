using System.Collections.Generic;
using System.Numerics;

namespace RunningStatistics;

/// <summary>
/// Keep track of the univariate sum.
/// </summary>
/// <typeparam name="TObs">Any generic number type.</typeparam>
public class Sum<TObs> : IRunningStatistic<TObs, Sum<TObs>> where TObs : INumber<TObs>
{
    public long Nobs { get; private set; }

    public TObs Value { get; private set; } = TObs.Zero;

    
    
    public void Fit(IEnumerable<TObs> values)
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
        return new Sum<TObs>
        {
            Nobs = Nobs,
            Value = Value
        };
    }

    public override string ToString() => $"{typeof(Sum<TObs>)} Nobs={Nobs} | Σ={Value}";

    public static explicit operator TObs(Sum<TObs> sum) => sum.Value;
}
