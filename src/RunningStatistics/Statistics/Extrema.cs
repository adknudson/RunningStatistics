// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RunningStatistics;

public sealed class Extrema : RunningStatisticBase<double, Extrema>
{
    private long _nobs;
    
    
    public double Min { get; private set; } = double.PositiveInfinity;

    public double Max { get; private set; } = double.NegativeInfinity;

    public long MinCount { get; private set; }

    public long MaxCount { get; private set; }


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

    public override void Merge(Extrema other)
    {
        if (Min == other.Min)
        {
            MinCount += other.MinCount;
        }
        else if (other.Min < Min)
        {
            Min = other.Min;
            MinCount = other.MinCount;
        }
        
        if (Max == other.Max)
        {
            MaxCount += other.MaxCount;
        }
        else if (other.Max > Max)
        {
            Max = other.Max;
            MaxCount = other.MaxCount;
        }
        
        _nobs += other.Nobs;
    }

    protected override string GetStatsString()
    {
        return $"Min={Min} (n={MinCount:N0}), Max={Max} (n={MaxCount:N0})"; 
    }
}