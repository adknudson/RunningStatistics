namespace RunningStatistics.Examples;

/// <summary>
/// Example of a complex statistic that contains multiple other statistics.
/// </summary>
public class MyComplexStatistic : RunningStatisticBase<double, MyComplexStatistic>
{
    private Mean Mean { get; } = new();
    
    private EmpiricalCdf Cdf { get; } = new();
    
    private CountMap<double> CountMap { get; } = new();
    
    
    protected override long GetNobs() => Mean.Nobs;

    public override void Fit(double value, long count)
    {
        Mean.Fit(value, count);
        Cdf.Fit(value, count);
        CountMap.Fit(value, count);
    }

    public override void Reset()
    {
        Mean.Reset();
        Cdf.Reset();
        CountMap.Reset();
    }

    public override MyComplexStatistic CloneEmpty() => new();

    public override void Merge(MyComplexStatistic other)
    {
        Mean.Merge(other.Mean);
        Cdf.Merge(other.Cdf);
        CountMap.Merge(other.CountMap);
    }

    protected override string GetStatsString()
    {
        return $"Mean: {Mean}, Cdf: {Cdf}, CountMap: {CountMap}";
    }
}