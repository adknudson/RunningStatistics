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
    /// <remarks>
    /// A generic observation type has no NaN to hand back, so an empty statistic reports its
    /// missing extreme by throwing, the counterpart of the NaN readouts on numeric statistics.
    /// Use <see cref="TryGetMin{TObs}(out TObs)"/> when a non-throwing read is needed.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when no observations have been fitted.</exception>
    public TObs Min => _nobs > 0
        ? _min!
        : throw new InvalidOperationException("No observations fitted yet. Use TryGetMin.");

    /// <summary>
    /// Gets the maximum value observed.
    /// </summary>
    /// <remarks>
    /// A generic observation type has no NaN to hand back, so an empty statistic reports its
    /// missing extreme by throwing, the counterpart of the NaN readouts on numeric statistics.
    /// Use <see cref="TryGetMax{TObs}(out TObs)"/> when a non-throwing read is needed.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when no observations have been fitted.</exception>
    public TObs Max => _nobs > 0
        ? _max!
        : throw new InvalidOperationException("No observations fitted yet. Use TryGetMax.");

    /// <summary>
    /// Stores the minimum value observed in the given reference and reports whether one exists.
    /// </summary>
    /// <param name="value">Receives the minimum value, or the default of the observation type when nothing was fitted.</param>
    /// <returns>True when at least one observation was fitted, false when the statistic is empty.</returns>
    public bool TryGetMin(out TObs value)
    {
        if (_nobs == 0)
        {
            // Every return must assign the out parameter. A caller who reads the false
            // return never uses this default, so a null or zero value is safe here.
            value = default!;
            return false;
        }

        value = _min!;
        return true;
    }

    /// <summary>
    /// Stores the maximum value observed in the given reference and reports whether one exists.
    /// </summary>
    /// <param name="value">Receives the maximum value, or the default of the observation type when nothing was fitted.</param>
    /// <returns>True when at least one observation was fitted, false when the statistic is empty.</returns>
    public bool TryGetMax(out TObs value)
    {
        if (_nobs == 0)
        {
            value = default!;
            return false;
        }

        value = _max!;
        return true;
    }

    /// <summary>
    /// Gets the count of observations that have the minimum value.
    /// </summary>
    public long MinCount { get; private set; }

    /// <summary>
    /// Gets the count of observations that have the maximum value.
    /// </summary>
    public long MaxCount { get; private set; }


    protected override long GetNobs() => _nobs;

    public override void Fit(TObs value)
    {
        Require.NotNull(value);
        UncheckedFit(value, 1);
    }

    public override void Fit(TObs value, long count)
    {
        Require.NotNull(value);
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
        
        checked { _nobs += count; }
        
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
        
        checked { _nobs += other.Nobs; }
        
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
    }

    public override string ToString()
    {
        return base.ToString() + $" | Min={_min} (n={MinCount:N0}), Max={_max} (n={MaxCount:N0})";
    }
}
