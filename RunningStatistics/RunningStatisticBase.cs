using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// The base class for all running statistics. This class provides default implementations when possible.
/// </summary>
public abstract class RunningStatisticBase<TObs, TSelf> : IRunningStatistic<TObs, TSelf>
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    public long Nobs => GetNobs();


    protected abstract long GetNobs();
    
    public virtual void Fit(TObs value)
    {
        Fit(value, 1);
    }

    public abstract void Fit(TObs value, long count);
    
    public virtual void Fit(IEnumerable<TObs> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }
    
    public virtual void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        foreach (var kvp in keyValuePairs)
        {
            Fit(kvp.Key, kvp.Value);
        }
    }
    
    public abstract void Reset();

    IRunningStatistic IRunningStatistic.CloneEmpty() => CloneEmpty();

    IRunningStatistic IRunningStatistic.Clone() => Clone();

    IRunningStatistic<TObs> IRunningStatistic<TObs>.CloneEmpty() => CloneEmpty();

    IRunningStatistic<TObs> IRunningStatistic<TObs>.Clone() => Clone();
    
    public abstract TSelf CloneEmpty();

    public virtual TSelf Clone()
    {
        var newStat = CloneEmpty();
        newStat.UnsafeMerge(this);
        return newStat;
    }

    public virtual void UnsafeMerge(IRunningStatistic other)
    {
        Require.TypeToBe<TSelf>(other, out var typedOther);
        Merge(typedOther);
    }

    public virtual void UnsafeMerge(IRunningStatistic<TObs> other)
    {
        Require.TypeToBe<TSelf>(other, out var typedOther);
        Merge(typedOther);
    }
    
    public abstract void Merge(TSelf other);

    /// <summary>
    /// Merge one or more running statistics together into a new instance. If only a source statistic
    /// is supplied, then this is equivalent to creating a clone of the source statistic.
    /// </summary>
    /// <param name="sourceStatistic">The running statistic which acts as the base for the returned statistic.</param>
    /// <param name="stats">Other statistics to be merged.</param>
    /// <returns>A new instance of a running statistic.</returns>
    public static TSelf Merge(TSelf sourceStatistic, params TSelf[] stats)
    {
        var newStat = sourceStatistic.Clone();
        
        foreach (var stat in stats)
        {
            newStat.Merge(stat);
        }

        return newStat;
    }

    public override string ToString() => $"{TypeWithObs()}(Nobs={Nobs:N0})";

    private static string TypeWithObs() => $"{typeof(TSelf).Name.Split('`').First()}{{{typeof(TObs).Name}}}";
}
