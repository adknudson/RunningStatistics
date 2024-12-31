using System.Numerics;

namespace RunningStatistics;

public sealed class Sum<TObs> : RunningStatisticBase<TObs, Sum<TObs>> 
    where TObs : IAdditionOperators<TObs, TObs, TObs>, IAdditiveIdentity<TObs, TObs>
{
    private long _nobs;
    
    
    public TObs Value { get; private set; } = TObs.AdditiveIdentity;


    protected override long GetNobs() => _nobs;

    public override void Fit(TObs value)
    {
        _nobs++;
        Value += value;
    }

    public override void Fit(TObs value, long count)
    {
        Require.NonNegative(count);
        
        for (var i = 0; i < count; i++)
        {
            Fit(value);
        }
    }

    public override void Reset()
    {
        _nobs = 0;
        Value = TObs.AdditiveIdentity;
    }

    public override Sum<TObs> CloneEmpty() => new();
    
    public override void Merge(Sum<TObs> sum)
    {
        _nobs += sum.Nobs;
        Value += sum.Value;
    }

    public static explicit operator TObs(Sum<TObs> sum) => sum.Value;

    protected override string GetStatsString() => $"Σ={Value}";
}