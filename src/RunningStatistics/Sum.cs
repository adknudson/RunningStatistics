using System.Numerics;

namespace RunningStatistics;

public sealed class Sum : AbstractRunningStatistic<double, Sum>
{
    public double Value { get; private set; }

    
    
    public override void Fit(double value)
    {
        Nobs++;
        Value += value;
    }

    public override void Reset()
    {
        Nobs = 0;
        Value = 0;
    }

    public override Sum CloneEmpty() => new();

    public override Sum Clone()
    {
        return new Sum
        {
            Nobs = Nobs,
            Value = Value
        };
    }
    public override void Merge(Sum sum)
    {
        Nobs += sum.Nobs;
        Value += sum.Value;
    }

    public override string ToString() => $"{typeof(Sum)} Nobs={Nobs} | Σ={Value}";

    public static explicit operator double(Sum sum) => sum.Value;
    
    public double Mean() => Value / Nobs;
}


#if NET7_0_OR_GREATER

/// <summary>
/// Keep track of the univariate sum.
/// </summary>
/// <typeparam name="TObs">Any generic number type.</typeparam>
public sealed class Sum<TObs> : AbstractRunningStatistic<TObs, Sum<TObs>> where TObs : INumber<TObs>
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

    public override Sum<TObs> CloneEmpty() => new();

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

#endif
