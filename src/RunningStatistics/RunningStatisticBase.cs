﻿using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The base class for all running statistics. This class provides a default implementation for the
/// generic clone and merge methods. 
/// </summary>
public abstract class RunningStatisticBase<TObs, TSelf> : IRunningStatistic<TObs, TSelf>
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    public long Nobs => GetNobs();


    protected abstract long GetNobs();
    
    public abstract void Fit(TObs value);
    
    public virtual void Fit(IEnumerable<TObs> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }
    
    public abstract void Reset();

    IRunningStatistic<TObs> IRunningStatistic<TObs>.CloneEmpty() => CloneEmpty();

    IRunningStatistic<TObs> IRunningStatistic<TObs>.Clone() => Clone();
    
    public abstract TSelf CloneEmpty();
    
    public abstract TSelf Clone();
    
    public void Merge(IRunningStatistic<TObs> other)
    {
        if (other is TSelf stats)
        {
            Merge(stats);
        }
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
}
