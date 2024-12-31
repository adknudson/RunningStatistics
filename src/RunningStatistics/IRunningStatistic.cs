using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The common interface for all statistics that can fit observations of type <see cref="TObs"/>
/// </summary>
/// <typeparam name="TObs">The type of observation being fit</typeparam>
public interface IRunningStatistic<TObs>
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
    /// Fit a single observation with an associated count.
    /// </summary>
    public void Fit(TObs value, long count);

    /// <summary>
    /// Fit a list of observations.
    /// </summary>
    public void Fit(IEnumerable<TObs> values);
    
    /// <summary>
    /// Fit a list of observations with associated counts.
    /// </summary>
    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs);

    /// <summary>
    /// Reset the running statistic to its initial state.
    /// </summary>
    public void Reset();
    
    /// <summary>
    /// Create a copy of the running statistic with the same internal parameters but with zero observations.
    /// </summary>
    /// <returns>An empty copy with type <see cref="IRunningStatistic{TObs}"/></returns>
    public IRunningStatistic<TObs> CloneEmpty();
    
    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    /// <returns>A copy with type <see cref="IRunningStatistic{TObs}"/></returns>
    public IRunningStatistic<TObs> Clone();

    /// <summary>
    /// Merge the values from another running statistic without the guarantee of the types being the same.
    /// </summary>
    public void UnsafeMerge(IRunningStatistic<TObs> other);
}


/// <summary>
/// A stronger version of the running statistic interface that allows for more specific type information.
/// </summary>
/// <typeparam name="TObs">The type of observation being fit</typeparam>
/// <typeparam name="TSelf">The concrete type of the running statistic</typeparam>
public interface IRunningStatistic<TObs, TSelf> : IRunningStatistic<TObs> 
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    /// <summary>
    /// Create a newly initialized copy of the running statistic.
    /// </summary>
    /// <returns>An empty copy with the same concrete type.</returns>
    public new TSelf CloneEmpty();

    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    /// <returns>A copy with the same concrete type.</returns>
    public new TSelf Clone();
    
    /// <summary>
    /// Merge the values from another running statistic of the same concrete type.
    /// </summary>
    public void Merge(TSelf other);
}
