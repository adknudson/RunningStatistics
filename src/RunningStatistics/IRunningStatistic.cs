using System.Collections.Generic;

namespace RunningStatistics;

public interface IRunningStatistic<in TObs>
{
    /// <summary>
    /// The number of observations that have been fitted.
    /// </summary>
    public long Nobs { get; }

    /// <summary>
    /// Fit a single observation. 
    /// </summary>
    public void Fit(TObs value);

    /// <summary>
    /// Fit a list of observations.
    /// </summary>
    public void Fit(IEnumerable<TObs> values);

    /// <summary>
    /// Reset the running statistic to its initial state.
    /// </summary>
    public void Reset();
}

public interface IRunningStatistic<in TObs, TSelf> : IRunningStatistic<TObs> 
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    /// <summary>
    /// Create a newly initialized copy of the running statistic.
    /// </summary>
    public TSelf CloneEmpty();

    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    public TSelf Clone();
    
    /// <summary>
    /// Merge the values from another running statistic.
    /// </summary>
    public void Merge(TSelf other);
}
