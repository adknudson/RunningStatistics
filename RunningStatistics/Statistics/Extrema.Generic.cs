using System;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

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
    private long _minCount, _maxCount;
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
    public long MinCount => _minCount;

    /// <summary>
    /// Gets the count of observations that have the maximum value.
    /// </summary>
    public long MaxCount => _maxCount;
    

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
            _minCount = 0;
        }
        else if (value.CompareTo(_max!) > 0)
        {
            _max = value;
            _maxCount = 0;
        }
        
        if (value.Equals(_min))
        {
            _minCount += count;
        }
        
        if (value.Equals(_max))
        {
            _maxCount += count;
        }
    }

    public override void Reset()
    {
        _min = default;
        _max = default;
        _minCount = 0;
        _maxCount = 0;
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
            _minCount = other._minCount;
            _maxCount = other._maxCount;
            _nobs = other.Nobs;
            return;
        }
        
        // if the other is empty, do nothing
        if (other.Nobs == 0) return;
        
        // if both are non-empty, merge
        
        if (Min.Equals(other.Min))
        {
            _minCount += other._minCount;
        }
        else if (other.Min.CompareTo(Min) < 0)
        {
            _min = other.Min;
            _minCount = other._minCount;
        }
        
        if (Max.Equals(other.Max))
        {
            _maxCount += other._maxCount;
        }
        else if (other.Max.CompareTo(Max) > 0)
        {
            _max = other.Max;
            _maxCount = other._maxCount;
        }
        
        _nobs += other.Nobs;
    }

    protected override string GetStatsString()
    {
        return $"Min={Min} (n={MinCount:N0}), Max={Max} (n={MaxCount:N0})";
    }
}
