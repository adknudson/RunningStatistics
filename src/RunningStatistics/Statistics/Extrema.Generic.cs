using System;
using System.Numerics;
// ReSharper disable UnusedMember.Global

namespace RunningStatistics;

public sealed class Extrema<TObs> : RunningStatisticBase<TObs, Extrema<TObs>>
    where TObs : IMinMaxValue<TObs>, IComparable<TObs>
{
    private long _nobs;
    

    public TObs Min { get; private set; } = TObs.MaxValue;

    public TObs Max { get; private set; } = TObs.MinValue;
    
    public long MinCount { get; private set; }

    public long MaxCount { get; private set; }
    

    protected override long GetNobs() => _nobs;

    public override void Fit(TObs value) => UncheckedFit(value, 1);

    public void Fit(TObs value, long count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count), count, "Count must be non-negative.");
        }
        
        UncheckedFit(value, count);
    }
    
    /// <summary>
    /// Fit the value without checking if the count is non-negative.
    /// </summary>
    private void UncheckedFit(TObs value, long count)
    {
        _nobs += count;

        if (_nobs == count)
        {
            Min = Max = value;
        }
        
        if (value.CompareTo(Min) < 0)
        {
            Min = value;
            MinCount = 0;
        } 
        else if (value.CompareTo(Max) > 0)
        {
            Max = value;
            MaxCount = 0;
        }
        
        // value is in the range [Min, Max]

        if (value.Equals(Min))
        {
            MinCount += count;
        }
        
        if (value.Equals(Max))
        {
            MaxCount += count;
        }
    }

    public override void Reset()
    {
        Min = TObs.MaxValue;
        Max = TObs.MinValue;
        MinCount = 0;
        MaxCount = 0;
        _nobs = 0;
    }

    public override Extrema<TObs> CloneEmpty() => new();

    public override Extrema<TObs> Clone()
    {
        return new Extrema<TObs>
        {
            Min = Min,
            Max = Max,
            MinCount = MinCount,
            MaxCount = MaxCount,
            _nobs = Nobs
        };
    }

    public override void Merge(Extrema<TObs> extrema)
    {
        if (Min.Equals(extrema.Min))
        {
            MinCount += extrema.MinCount;
        }
        else if (extrema.Min.CompareTo(Min) < 0)
        {
            Min = extrema.Min;
            MinCount = extrema.MinCount;
        }
        
        if (Max.Equals(extrema.Max))
        {
            MaxCount += extrema.MaxCount;
        }
        else if (extrema.Max.CompareTo(Max) > 0)
        {
            Max = extrema.Max;
            MaxCount = extrema.MaxCount;
        }
        
        _nobs += extrema.Nobs;
    }

    protected override string GetStatsString()
    {
        return $"Min={Min} (n={MinCount:N0}), Max={Max} (n={MaxCount:N0})";
    }
}
