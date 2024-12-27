#if NET7_0_OR_GREATER

using System;
using System.Numerics;

namespace RunningStatistics;


public sealed class Extrema<TObs> : RunningStatisticBase<TObs, Extrema<TObs>>
    where TObs : IMinMaxValue<TObs>, IComparisonOperators<TObs, TObs, bool>
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
        
        if (value < Min)
        {
            Min = value;
            MinCount = 0;
        } 
        else if (value > Max)
        {
            Max = value;
            MaxCount = 0;
        }
        
        // value is in the range [Min, Max]

        if (value == Min)
        {
            MinCount += count;
        }
        
        if (value == Max)
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
        if (Min == extrema.Min)
        {
            MinCount += extrema.MinCount;
        }
        else if (extrema.Min < Min)
        {
            Min = extrema.Min;
            MinCount = extrema.MinCount;
        }
        
        if (Max == extrema.Max)
        {
            MaxCount += extrema.MaxCount;
        }
        else if (extrema.Max > Max)
        {
            Max = extrema.Max;
            MaxCount = extrema.MaxCount;
        }
        
        _nobs += extrema.Nobs;
    }
}

#endif