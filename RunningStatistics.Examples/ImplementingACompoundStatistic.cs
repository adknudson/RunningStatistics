namespace RunningStatistics.Examples;

/// <summary>
/// Example of a compound statistic that contains multiple other statistics.
/// </summary>
public class CompoundStatistic : RunningStatisticBase<double, CompoundStatistic>
{
    public Mean Mean { get; } = new();
    
    public EmpiricalCdf Cdf { get; } = new();
    
    public CountMap<double> CountMap { get; } = new();
    
    
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

    public override CompoundStatistic CloneEmpty() => new();

    public override void Merge(CompoundStatistic other)
    {
        Mean.Merge(other.Mean);
        Cdf.Merge(other.Cdf);
        CountMap.Merge(other.CountMap);
    }

    public override string ToString()
    {
        return base.ToString() + " | (Mean, CDF, CountMap)";
    }
}