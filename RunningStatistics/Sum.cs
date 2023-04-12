using System.Numerics;

namespace RunningStatistics;

/// <summary>
/// Keep track of the univariate sum.
/// </summary>
/// <typeparam name="TObs">Any generic number type.</typeparam>
public class Sum<TObs> : AbstractRunningStatistic<TObs, Sum<TObs>> where TObs : INumber<TObs>
{
    public TObs Value { get; private set; } = TObs.Zero;

    
    
    public override void Fit(TObs value)
    {
        Nobs++;
        Value += value;
    }

    public override void Reset()
    {
        Nobs = 0;
        Value = TObs.Zero;
    }

    public override Sum<TObs> CloneEmpty()
    {
        return new Sum<TObs>();
    }
    
    public override Sum<TObs> Clone()
    {
        return new Sum<TObs>
        {
            Nobs = Nobs,
            Value = Value
        };
    }
    public override void Merge(Sum<TObs> sum)
    {
        Nobs += sum.Nobs;
        Value += sum.Value;
    }

    public override string ToString() => $"{typeof(Sum<TObs>)} Nobs={Nobs} | Σ={Value}";

    public static explicit operator TObs(Sum<TObs> sum) => sum.Value;
}
