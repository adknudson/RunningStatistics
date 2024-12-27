using System;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RunningStatistics;

/// <summary>
/// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
/// </summary>
public sealed class Extrema : RunningStatisticBase<double, Extrema>
{
    private long _nobs;


    public double Min { get; private set; } = double.PositiveInfinity;

    public double Max { get; private set; } = double.NegativeInfinity;

    public long MinCount { get; private set; }

    public long MaxCount { get; private set; }

    public double Range => Max - Min;


    protected override long GetNobs() => _nobs;

    public override void Fit(double value)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("Value must not be NaN.", nameof(value));
        }
        
        UncheckedFit(value, 1);
    }

    public void Fit(double value, long count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count), count, "Count must be non-negative.");
        }

        if (double.IsNaN(value))
        {
            throw new ArgumentException("Value must not be NaN.", nameof(value));
        }
        
        UncheckedFit(value, count);
    }

    /// <summary>
    /// Fit the value without checking if the count is non-negative.
    /// </summary>
    private void UncheckedFit(double value, long count)
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
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        MinCount = 0;
        MaxCount = 0;
        _nobs = 0;
    }

    public override Extrema CloneEmpty() => new();

    public override Extrema Clone()
    {
        return new Extrema
        {
            Min = Min,
            Max = Max,
            MinCount = MinCount,
            MaxCount = MaxCount,
            _nobs = Nobs
        };
    }

    public override void Merge(Extrema extrema)
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
    
    public override string ToString() => $"{typeof(Extrema)} Nobs={Nobs} | Min={Min:F2}, Max={Max:F2}, MinCount={MinCount}, MaxCount={MaxCount}";
}