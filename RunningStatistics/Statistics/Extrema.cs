// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace RunningStatistics;

/// <summary>
/// Represents a running statistic that tracks the minimum and maximum values observed.
/// </summary>
public sealed class Extrema : RunningStatisticBase<double, Extrema>
{
    private double _min = double.PositiveInfinity;
    private double _max = double.NegativeInfinity;
    private long _minCount, _maxCount;
    private long _nobs;

    /// <summary>
    /// Gets the minimum value observed.
    /// </summary>
    public double Min => _nobs > 0 ? _min : double.NaN;

    /// <summary>
    /// Gets the maximum value observed.
    /// </summary>
    public double Max => _nobs > 0 ? _max : double.NaN;

    /// <summary>
    /// Gets the count of observations that have the minimum value.
    /// </summary>
    public long MinCount => _minCount;

    /// <summary>
    /// Gets the count of observations that have the maximum value.
    /// </summary>
    public long MaxCount => _maxCount;


    protected override long GetNobs() => _nobs;
    
    public override void Fit(double value, long count)
    {
        Require.NotNaN(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    private void UncheckedFit(double value, long count)
    {
        if (_nobs == 0)
        {
            _min = _max = value;
        }
        
        _nobs += count;
        
        if (value < _min)
        {
            _min = value;
            _minCount = 0;
        } 
        else if (value > _max)
        {
            _max = value;
            _maxCount = 0;
        }

        if (value == _min)
        {
            _minCount += count;
        }
        
        if (value == _max)
        {
            _maxCount += count;
        }
    }

    public override void Reset()
    {
        _min = double.PositiveInfinity;
        _max = double.NegativeInfinity;
        _minCount = 0;
        _maxCount = 0;
        _nobs = 0;
    }

    public override Extrema CloneEmpty() => new();

    public override void Merge(Extrema other)
    {
        // If both are empty, do nothing
        if (other.Nobs == 0 && Nobs == 0) return;
        
        // if this is empty, copy the other
        if (Nobs == 0)
        {
            _min = other._min;
            _max = other._max;
            _minCount = other._minCount;
            _maxCount = other._maxCount;
            _nobs = other.Nobs;
            return;
        }
        
        // if the other is empty, do nothing
        if (other.Nobs == 0) return;
        
        // if both are non-empty, merge
        
        if (_min == other._min)
        {
            _minCount += other._minCount;
        }
        else if (other._min < _min)
        {
            _min = other._min;
            _minCount = other._minCount;
        }
        
        if (_max == other._max)
        {
            _maxCount += other._maxCount;
        }
        else if (other._max > _max)
        {
            _max = other._max;
            _maxCount = other._maxCount;
        }
        
        _nobs += other.Nobs;
    }

    public override string ToString() => base.ToString() + $" | Min={Min} (n={MinCount:N0}), Max={Max} (n={MaxCount:N0})";
}