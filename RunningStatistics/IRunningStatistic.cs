using System.Collections.Generic;
using System.IO;

namespace RunningStatistics;

public interface IRunningStatistic<TObs>
{
    /// <summary>
    /// The number of observations that have been fit.
    /// </summary>
    public long Count { get; }

    /// <summary>
    /// Fit multiple observations.
    /// </summary>
    /// <param name="values"></param>
    public void Fit(IEnumerable<TObs> values);

    /// <summary>
    /// Fit a single observation.
    /// </summary>
    public void Fit(TObs value);

    /// <summary>
    /// Reset the running statistic to its empty state.
    /// </summary>
    public void Reset();
    
    /// <summary>
    /// Merge a running statistic with another instance of itself. If the type is different, then no merging occurs.
    /// </summary>
    /// <param name="other"></param>
    public void Merge(IRunningStatistic<TObs> other);

    /// <summary>
    /// Print the fitted values to a stream in a human-readable format.
    /// </summary>
    /// <param name="stream"></param>
    public void Print(StreamWriter stream);
}