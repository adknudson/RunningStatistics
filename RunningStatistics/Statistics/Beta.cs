using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable MemberCanBePrivate.Global

namespace RunningStatistics;

public sealed class Beta : RunningStatisticBase<bool, Beta>
{
    private long _a, _b;


    public Beta()
    {
    }
    
    public Beta(long successes, long failures)
    {
        Require.NonNegative(successes);
        Require.NonNegative(failures);
        
        _a = successes;
        _b = failures;
    }


    public long Successes => _a;

    public long Failures => _b;

    public double Mean => Nobs > 0 
        ? (double)_a / (_a + _b) 
        : double.NaN;

    public double Median => _a > 1 && _b > 1 
        ? this.Quantile(0.5)
        : double.NaN;

    public double Mode => _a > 1 && _b > 1
        ? (double)(_a - 1) / (_a + _b - 2)
        : double.NaN;

    public double Variance => _a > 0 && _b > 0
        ? (_a * _b) / (Math.Pow(_a + _b, 2) * (_a + _b + 1))
        : double.NaN;


    protected override long GetNobs() => _a + _b;

    public override void Fit(bool value, long count)
    {
        Require.NonNegative(count);
        
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
        Require.NonNegative(successes);
        Require.NonNegative(failures);

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

    public override Beta CloneEmpty() => new();

    public override void Merge(Beta other)
    {
        checked
        {
            _a += other.Successes;
            _b += other.Failures;
        }
    }
    
    protected override string GetStatsString() => $"α={Successes:N0}, β={Failures:N0}";
}