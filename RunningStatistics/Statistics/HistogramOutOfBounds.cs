using System;

namespace RunningStatistics;

/// <summary>
/// Which direction an observation is out of bounds.
/// </summary>
internal enum OutOfBoundsSide
{
    Lower, 
    Upper
}

/// <summary>
/// A container for keeping track of observations that are outside of the <see cref="Histogram"/> bins.
/// </summary>
internal struct HistogramOutOfBounds
{
    public long Lower { get; private set; }

    public long Upper { get; private set; }
    
    public void Reset()
    {
        Lower = 0;
        Upper = 0;
    }

    public void Fit(OutOfBoundsSide side, long count)
    {
        switch (side)
        {
            case OutOfBoundsSide.Lower:
                Lower += count;
                return;
            case OutOfBoundsSide.Upper:
                Upper += count;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }
    }

    public void Merge(HistogramOutOfBounds other)
    {
        Lower += other.Lower;
        Upper += other.Upper;
    }
}