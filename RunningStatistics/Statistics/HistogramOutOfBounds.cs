using System;

namespace RunningStatistics;

/// <summary>
/// A container for keeping track of observations that are outside of the <see cref="Histogram"/> bins.
/// </summary>
internal struct HistogramOutOfBounds
{
    /// <summary>
    /// Gets the count of observations below the lower bound.
    /// </summary>
    public long Lower { get; private set; }

    /// <summary>
    /// Gets the count of observations above the upper bound.
    /// </summary>
    public long Upper { get; private set; }
    
    /// <summary>
    /// Resets the counts of out-of-bounds observations to zero.
    /// </summary>
    public void Reset()
    {
        Lower = 0;
        Upper = 0;
    }

    /// <summary>
    /// Adds the specified count to the appropriate out-of-bounds side.
    /// </summary>
    /// <param name="lower">If true, the lower side is incremented. If false, the upper side is incremented.</param>
    /// <param name="count">The number of observations to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified side is not valid.</exception>
    public void Fit(bool lower, long count)
    {
        if (lower)
        {
            Lower += count;
        }
        else
        {
            Upper += count;
        }
    }

    /// <summary>
    /// Merges the counts of out-of-bounds observations from another <see cref="HistogramOutOfBounds"/> instance.
    /// </summary>
    /// <param name="other">The other <see cref="HistogramOutOfBounds"/> instance to merge.</param>
    public void Merge(HistogramOutOfBounds other)
    {
        Lower += other.Lower;
        Upper += other.Upper;
    }
}