using System.Collections.Generic;

namespace RunningStatistics;

public interface IRunningStatistic<in TObs>
{
    public long Nobs { get; }
    
    public void Fit(IEnumerable<TObs> values);

    public void Fit(TObs value);
    
    public void Reset();
}


public interface IRunningStatistic<in TObs, out TValue> : IRunningStatistic<TObs>
{
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
