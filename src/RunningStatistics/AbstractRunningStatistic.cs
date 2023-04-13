using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The common interface for all statistics that can fit observations of type <see cref="TObs"/>
/// </summary>
public abstract class AbstractRunningStatistic<TObs, TSelf> where TSelf : AbstractRunningStatistic<TObs, TSelf>
{
    private long _nobs;

    /// <summary>
    /// The number of observations that have been fitted.
    /// </summary>
    public long Nobs
    {
        get => GetNobs();
        protected set => _nobs = value;
    }

    /// <summary>
    /// The default method for retrieving the number of observations.
    /// </summary>
    protected virtual long GetNobs() => _nobs;

    /// <summary>
    /// Fit a single observation. 
    /// </summary>
    public abstract void Fit(TObs value);
    
    /// <summary>
    /// Fit a list of observations.
    /// </summary>
    public virtual void Fit(IEnumerable<TObs> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    /// <summary>
    /// Reset the running statistic to its initial state.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Create a copy of the running statistic with the same internal parameters but with zero observations.
    /// </summary>
    public abstract TSelf CloneEmpty();

    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    /// <returns></returns>
    public abstract TSelf Clone();

    /// <summary>
    /// Merge the values from another running statistic.
    /// </summary>
    /// <param name="other"></param>
    public abstract void Merge(TSelf other);

    /// <summary>
    /// Merge one or more running statistics together into a new instance. If only a source statistic is supplied, then
    /// this is equivalent to creating a clone of the source statistic.
    /// </summary>
    /// <param name="sourceStatistic">The running statistic which acts as the base for the returned statistic.</param>
    /// <param name="stats">Extra statistics to be merged.</param>
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
