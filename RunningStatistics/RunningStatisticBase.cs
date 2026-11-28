using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// The base class for all running statistics. This class provides default implementations when possible.
/// </summary>
public abstract class RunningStatisticBase<TObs, TSelf> : IRunningStatistic<TObs, TSelf>
    where TSelf : RunningStatisticBase<TObs, TSelf>
{
    public abstract long Nobs { get; }

    public abstract void Fit(TObs value);

    public abstract void UncheckedFit(TObs value);

    public abstract void Fit(TObs value, long count);
    
    public abstract void UncheckedFit(TObs value, long count);

    public virtual void Fit(IEnumerable<TObs> values)
    {
        var stat = CloneEmpty();
        foreach (var value in values)
        {
            stat.Fit(value);
        }
        Merge(stat);
    }

    public virtual void UncheckedFit(IEnumerable<TObs> values)
    {
        var stat = CloneEmpty();
        foreach (var value in values)
        {
            stat.UncheckedFit(value);
        }
        Merge(stat);
    }

    public virtual void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        var stat = CloneEmpty();
        foreach (var (x, k) in keyValuePairs)
        {
            stat.Fit(x, k);
        }
        Merge(stat);
    }

    public virtual void UncheckedFit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        var stat = CloneEmpty();
        foreach (var (x, k) in keyValuePairs)
        {
            stat.UncheckedFit(x, k);
        }
        Merge(stat);
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
        newStat.Merge((TSelf)(IRunningStatistic<TObs, TSelf>)this);
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
    /// Merge one or more running statistics together into a new instance. If you supply only a source
    /// statistic, the method returns a clone of that statistic.
    /// </summary>
    /// <param name="sourceStatistic">The running statistic which acts as the base for the returned statistic.</param>
    /// <param name="stats">Other statistics to merge into the new instance.</param>
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
