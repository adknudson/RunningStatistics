using System.Collections.Generic;

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

    IRunningStatistic<TObs> IRunningStatistic<TObs>.CloneEmpty() => CloneEmpty();

    IRunningStatistic<TObs> IRunningStatistic<TObs>.Clone() => Clone();
    
    public abstract TSelf CloneEmpty();

    public TSelf Clone()
    {
        var newStat = CloneEmpty();
        newStat.UnsafeMerge(this);
        return newStat;
    }
    
    public void UnsafeMerge(IRunningStatistic<TObs> other)
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

    public sealed override string ToString()
    {
        return $"{typeof(TSelf).Name}(Nobs={Nobs:N0}) | {GetStatsString()}";
    }

    protected abstract string GetStatsString();
}
