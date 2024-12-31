using RunningStatistics.UncheckedStatistics;

namespace RunningStatistics;

/// <summary>
/// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
/// </summary>
public sealed class Extrema : RunningStatisticBase<double, Extrema>
{
    private readonly UncheckedExtrema _extrema = new();

    
    public double Min => _extrema.Min;

    public double Max => _extrema.Max;

    public long MinCount => _extrema.MinCount;

    public long MaxCount => _extrema.MaxCount;
    

    protected override long GetNobs() => _extrema.Nobs;
    
    public override void Fit(double value, long count)
    {
        Require.NotNaN(value);
        Require.NonNegative(count);
        if (count == 0) return;
        _extrema.Fit(value, count);
    }

    public override void Reset() => _extrema.Reset();

    public override Extrema CloneEmpty() => new();

    public override void Merge(Extrema other) => _extrema.Merge(other._extrema);

    protected override string GetStatsString()
    {
        return $"Min={Min} (n={MinCount:N0}), Max={Max} (n={MaxCount:N0})"; 
    }
}