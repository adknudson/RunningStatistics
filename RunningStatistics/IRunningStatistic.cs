using System.Collections.Generic;

namespace RunningStatistics;

public interface IRunningStatistic
{
    /// <summary>
    /// The number of observations fitted so far.
    /// </summary>
    public long Nobs { get; }

    /// <summary>
    /// Reset the running statistic to its initial state.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Create a copy of the running statistic with the same internal parameters but with zero observations.
    /// </summary>
    /// <returns>An empty copy with type <see cref="IRunningStatistic"/></returns>
    public IRunningStatistic CloneEmpty();

    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    /// <returns>A copy with type <see cref="IRunningStatistic"/></returns>
    public IRunningStatistic Clone();

    /// <summary>
    /// Merge the values from another running statistic. This method performs a runtime type check and
    /// will throw an <see cref="System.InvalidCastException"/> if <paramref name="other"/> cannot be
    /// cast to the concrete type of this instance. For a type-safe alternative, use
    /// <see cref="IRunningStatistic{TObs, TSelf}.Merge"/> when the concrete type is known.
    /// </summary>
    /// <exception cref="System.InvalidCastException">
    /// Thrown when <paramref name="other"/> is not the same concrete type as this instance.
    /// </exception>
    public void UnsafeMerge(IRunningStatistic other);
}

/// <summary>
/// The common interface for all statistics that can fit observations of type <see cref="TObs"/>
/// </summary>
/// <typeparam name="TObs">The type of observation being fit</typeparam>
public interface IRunningStatistic<TObs> : IRunningStatistic
{
    /// <summary>
    /// Fits a single observation.
    /// </summary>
    public void Fit(TObs value);

    /// <summary>
    /// Fits a single observation without without checking that the arguments are valid.
    /// </summary>
    public void UncheckedFit(TObs value);

    /// <summary>
    /// Fits an observed value with an associated count.
    /// </summary>
    public void Fit(TObs value, long count);

    /// <summary>
    /// Fits an observed value with an associated count without checking that the arguments are valid.
    /// </summary>
    public void UncheckedFit(TObs value, long count);

    /// <summary>
    /// Fits a list of observations.
    /// </summary>
    public void Fit(IEnumerable<TObs> values);

    /// <summary>
    /// Fits a list of observations without checking that the arguments or observations are valid.
    /// </summary>
    public void UncheckedFit(IEnumerable<TObs> values);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyValuePairs"></param>
    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyValuePairs"></param>
    public void UncheckedFit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs);

    /// <summary>
    /// Create a copy of the running statistic with the same internal parameters but with zero observations.
    /// </summary>
    /// <returns>An empty copy with type <see cref="IRunningStatistic{TObs}"/></returns>
    public new IRunningStatistic<TObs> CloneEmpty();

    /// <summary>
    /// Create a deep copy of the running statistic.
    /// </summary>
    /// <returns>A copy with type <see cref="IRunningStatistic{TObs}"/></returns>
    public new IRunningStatistic<TObs> Clone();

    /// <summary>
    /// Merge the values from another running statistic of the same observation type. This method
    /// performs a runtime type check and will throw an <see cref="System.InvalidCastException"/> if
    /// <paramref name="other"/> cannot be cast to the concrete type of this instance. For a type-safe
    /// alternative, use <see cref="IRunningStatistic{TObs, TSelf}.Merge"/> when the concrete type is known.
    /// </summary>
    /// <exception cref="System.InvalidCastException">
    /// Thrown when <paramref name="other"/> is not the same concrete type as this instance.
    /// </exception>
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
    /// Create a deep copy of the running statistic. The default implementation in
    /// <see cref="RunningStatisticBase{TObs,TSelf}"/> calls <see cref="CloneEmpty"/> followed by
    /// <see cref="Merge"/>. Implement both of those members correctly for <c>Clone</c>
    /// to behave correctly.
    /// </summary>
    /// <returns>A copy with the same concrete type.</returns>
    public new TSelf Clone();

    /// <summary>
    /// Merge the values from another running statistic of the same concrete type into this instance.
    /// This is the type-safe counterpart to <c>UnsafeMerge</c> and is also used internally by the
    /// default <see cref="Clone"/> implementation.
    /// </summary>
    public void Merge(TSelf other);
}
