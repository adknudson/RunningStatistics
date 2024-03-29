﻿#if NET7_0_OR_GREATER

using System;
using System.Numerics;

namespace RunningStatistics;


public sealed class Extrema<TObs> : AbstractRunningStatistic<TObs, Extrema<TObs>>
    where TObs : 
    INumberBase<TObs>,
    IMinMaxValue<TObs>, 
    IComparisonOperators<TObs, TObs, bool>
{
    private readonly TObs _tolerance;


    public Extrema()
    {
        _tolerance = TObs.Zero;
    }
    
    public Extrema(TObs tolerance)
    {
        if (tolerance < TObs.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "The tolerance must be non-negative.");
        }
        
        _tolerance = tolerance;
    }


    public TObs Min { get; private set; } = TObs.MaxValue;

    public TObs Max { get; private set; } = TObs.MinValue;
    
    public long MinCount { get; private set; }

    public long MaxCount { get; private set; }

    public TObs Range => Max - Min;
    
    
    public override void Fit(TObs value)
    {
        UncheckedFit(value, 1);
    }
    
    public void Fit(TObs value, long count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative.");
        UncheckedFit(value, count);
    }
    
    /// <summary>
    /// Fit the value without checking if the count is non-negative.
    /// </summary>
    private void UncheckedFit(TObs value, long count)
    {
        if (Nobs == 0) Min = Max = value;

        Nobs += count;
        
        if (value < Min)
        {
            Min = value;
            MinCount = count;
        }

        if (value > Max)
        {
            Max = value;
            MaxCount = count;
        }
        
        if (AbsDiff(value, Min) <= _tolerance) MinCount += count;
        if (AbsDiff(value, Max) <= _tolerance) MaxCount += count;
    }

    public override void Reset()
    {
        Min = TObs.MaxValue;
        Max = TObs.MinValue;
        MinCount = 0;
        MaxCount = 0;
        Nobs = 0;
    }

    public override Extrema<TObs> CloneEmpty() => new(_tolerance);

    public override Extrema<TObs> Clone()
    {
        return new Extrema<TObs>(_tolerance)
        {
            Min = Min,
            Max = Max,
            MinCount = MinCount,
            MaxCount = MaxCount,
            Nobs = Nobs
        };
    }

    public override void Merge(Extrema<TObs> extrema)
    {
        Nobs += extrema.Nobs;

        if (AbsDiff(Min, extrema.Min) <= _tolerance)
        {
            MinCount += extrema.MinCount;
        }
        else if (extrema.Min < Min)
        {
            Min = extrema.Min;
            MinCount = extrema.MinCount;
        }

        if (AbsDiff(Max, extrema.Max) <= _tolerance)
        {
            MaxCount += extrema.MaxCount;
        }
        else if (extrema.Max > Max)
        {
            Max = extrema.Max;
            MaxCount = extrema.MaxCount;
        }
    }

    /// <summary>
    /// Return the absolute difference between two values.
    /// </summary>
    private static TObs AbsDiff(TObs value1, TObs value2)
    {
        return value2 > value1 ? value2 - value1 : value1 - value2;
    }
}

#endif