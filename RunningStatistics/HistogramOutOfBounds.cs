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
    private long Lower { get; set; }

    private long Upper { get; set; }
        
    public (long, long) Counts => (Lower, Upper);

    private long Nobs => Lower + Upper;

        
        
    internal void Reset()
    {
        Lower = 0;
        Upper = 0;
    }

    internal void Update(OutOfBoundsSide side, long count)
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

    internal void Merge(HistogramOutOfBounds other)
    {
        Lower += other.Lower;
        Upper += other.Upper;
    }

    internal HistogramOutOfBounds Clone()
    {
        return new HistogramOutOfBounds
        {
            Lower = Lower,
            Upper = Upper
        };
    }

    public override string ToString()
    {
        return $"{typeof(HistogramOutOfBounds)} Nobs={Nobs} | LowerCount={Lower}, UpperCount={Upper}";
    }
}