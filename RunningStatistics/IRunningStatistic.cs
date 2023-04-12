using System.Collections.Generic;

namespace RunningStatistics;

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


public interface IRunningStatistic<in TObs, out TValue> : IRunningStatistic<TObs>
{
    /// <summary>
    /// A generic representation of the running statistic.
    /// </summary>
    public TValue Value { get; }
}


public interface IRunningStatistic<in TObs, out TValue, TSelf> : IRunningStatistic<TObs, TValue> where TSelf : IRunningStatistic<TObs, TValue, TSelf>
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
