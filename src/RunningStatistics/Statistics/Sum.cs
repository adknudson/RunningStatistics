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

    public double Mean()
    {
        return Value / Nobs;
    }

    public override string ToString() => $"{typeof(Sum)} Nobs={Nobs} | Σ={Value}";

    public static explicit operator double(Sum sum) => sum.Value;
}

