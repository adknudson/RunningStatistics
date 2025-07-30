using System;

namespace RunningStatistics;

/// <summary>
/// Represents a running statistic that tracks the minimum and maximum values observed for a generic type.
/// </summary>
/// <typeparam name="TObs">The type of the observations.
/// Must implement <see cref="IComparable{TObs}"/>.</typeparam>
public sealed class Extrema<TObs> : RunningStatisticBase<TObs, Extrema<TObs>>
    where TObs : IComparable<TObs>
{
    private TObs? _min, _max;
    private long _nobs;

    /// <summary>
    /// Gets the minimum value observed.
    /// </summary>
    public TObs Min => _nobs > 0
        ? _min!
        : throw new InvalidOperationException("Nobs must be greater than 0");

    /// <summary>
    /// Gets the maximum value observed.
    /// </summary>
    public TObs Max => _nobs > 0
        ? _max!
        : throw new InvalidOperationException("Nobs must be greater than 0");

    /// <summary>
    /// Gets the count of observations that have the minimum value.
    /// </summary>
    public long MinCount { get; private set; }

    /// <summary>
    /// Gets the count of observations that have the maximum value.
    /// </summary>
    public long MaxCount { get; private set; }


    protected override long GetNobs() => _nobs;

    public override void Fit(TObs value) => UncheckedFit(value, 1);

    public override void Fit(TObs value, long count)
    {
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }
    
    private void UncheckedFit(TObs value, long count)
    {
        if (_nobs == 0)
        {
            _min = _max = value;
        }
        
        _nobs += count;
        
        if (value.CompareTo(_min!) < 0)
        {
            _min = value;
            MinCount = 0;
        }
        else if (value.CompareTo(_max!) > 0)
        {
            _max = value;
            MaxCount = 0;
        }
        
        if (value.CompareTo(_min!) == 0)
        {
            MinCount += count;
        }
        
        if (value.CompareTo(_max!) == 0)
        {
            MaxCount += count;
        }
    }

    public override void Reset()
    {
        _min = default;
        _max = default;
        MinCount = 0;
        MaxCount = 0;
        _nobs = 0;
    }

    public override Extrema<TObs> CloneEmpty() => new();
    
    public override void Merge(Extrema<TObs> other)
    {
        // If both are empty, do nothing
        if (other.Nobs == 0 && Nobs == 0) return;
        
        // if this is empty, copy the other
        if (Nobs == 0)
        {
            _min = other.Min;
            _max = other.Max;
            MinCount = other.MinCount;
            MaxCount = other.MaxCount;
            _nobs = other.Nobs;
            return;
        }
        
        // if the other is empty, do nothing
        if (other.Nobs == 0) return;
        
        // if both are non-empty, merge
        
        if (Min.CompareTo(other.Min) == 0)
        {
            MinCount += other.MinCount;
        }
        else if (other.Min.CompareTo(Min) < 0)
        {
            _min = other.Min;
            MinCount = other.MinCount;
        }
        
        if (Max.CompareTo(other.Max) == 0)
        {
            MaxCount += other.MaxCount;
        }
        else if (other.Max.CompareTo(Max) > 0)
        {
            _max = other.Max;
            MaxCount = other.MaxCount;
        }
        
        _nobs += other.Nobs;
    }

    public override string ToString()
    {
        return base.ToString() + $" | Min={_min} (n={MinCount:N0}), Max={_max} (n={MaxCount:N0})";
    }
}
