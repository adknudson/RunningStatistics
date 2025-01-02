using System.Collections.Generic;
using System.Linq;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable MemberCanBePrivate.Global

namespace RunningStatistics.Unchecked;

public sealed class UncheckedBeta : RunningStatisticBase<bool, UncheckedBeta>
{
    private long _a, _b;
    
    
    public UncheckedBeta()
    {
    }
    
    public UncheckedBeta(long successes, long failures)
    {
        _a = successes;
        _b = failures;
    }
    
    
    public long Successes => _a;

    public long Failures => _b;

    public double Mean => (double)_a / (_a + _b);

    public double Median => this.Quantile(0.5);

    public double Mode => (double)(_a - 1) / (_a + _b - 2);
    
    public double Variance => (double) _a * _b / ((_a + _b) * (_a + _b) * (_a + _b + 1));


    protected override long GetNobs() => _a + _b;

    public override void Fit(bool value, long count)
    {
        if (value)
        {
            _a += count;
        }
        else
        {
            _b += count;
        }
    }

    public void Fit(long successes, long failures)
    {
        _a += successes;
        _b += failures;
    }

    public override void Fit(IEnumerable<bool> values)
    {
        var bs = values.ToList();
        var n = bs.Count;
        var s = bs.Count(b => b);
        Fit(s, n - s);
    }

    public override void Reset()
    {
        _a = 0;
        _b = 0;
    }

    public override UncheckedBeta CloneEmpty() => new();

    public override void Merge(UncheckedBeta other)
    {
        _a += other.Successes;
        _b += other.Failures;
    }

    protected override string GetStatsString() => $"α={Successes}, β={Failures}";
}