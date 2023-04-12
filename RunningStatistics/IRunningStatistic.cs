using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// The common interface for all statistics that can fit observations of type <see cref="TObs"/>
/// </summary>
public interface IRunningStatistic<in TObs>
{
    /// <summary>
    /// The number of observations that have been fitted.
    /// </summary>
    public long Nobs { get; }
    
    /// <summary>
    /// Fit a list of observations.
    /// </summary>
    public void Fit(IEnumerable<TObs> values);

    /// <summary>
    /// Fit a single observation.
    /// </summary>
    public void Fit(TObs value);
    
    /// <summary>
    /// Reset the running statistic to its initial state.
    /// </summary>
    public void Reset();
}



/// <summary>
/// The common interface for statistics that can fit observations of type <see cref="TObs"/> and adds the clone/merge
/// interface.
/// </summary>
public interface IRunningStatistic<in TObs, TSelf> : IRunningStatistic<TObs> where TSelf : IRunningStatistic<TObs, TSelf>
{
    /// <summary>
    /// Create a copy of the running statistic with the same internal parameters but with zero observations.
    /// </summary>
    public TSelf CloneEmpty();

    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    /// <returns></returns>
    public TSelf Clone();

    /// <summary>
    /// Merge the values from another running statistic.
    /// </summary>
    /// <param name="other"></param>
    public void Merge(TSelf other);

    /// <summary>
    /// Merge two running statistics together into a new instance.
    /// </summary>
    public static abstract TSelf Merge(TSelf first, TSelf second);
}
