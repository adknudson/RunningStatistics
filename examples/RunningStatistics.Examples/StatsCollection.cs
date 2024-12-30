namespace RunningStatistics.Examples;

public class StatsCollection : IRunningStatistic<double, StatsCollection>
{
    private CountMap<double> CountMap { get; init; } = new();
    
    private EmpiricalCdf Ecdf { get; init; } = new();

    public long Nobs => Ecdf.Nobs;
    
    public void Fit(double value)
    {
        CountMap.Fit(value);
        Ecdf.Fit(value);
    }

    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Reset()
    {
        CountMap.Reset();
        Ecdf.Reset();
    }

    IRunningStatistic<double> IRunningStatistic<double>.CloneEmpty() => CloneEmpty();

    IRunningStatistic<double> IRunningStatistic<double>.Clone() => Clone();

    public StatsCollection CloneEmpty()
    {
        return new StatsCollection();
    }

    public StatsCollection Clone()
    {
        return new StatsCollection
        {
            CountMap = CountMap.Clone(),
            Ecdf = Ecdf.Clone()
        };
    }
    
    public void UnsafeMerge(IRunningStatistic<double> other)
    {
        if (other is StatsCollection stats)
        {
            Merge(stats);
        }

        throw new InvalidCastException($"Other statistic must be of type {GetType()}");
    }
    
    public void Merge(StatsCollection other)
    {
        CountMap.Merge(other.CountMap);
        Ecdf.Merge(other.Ecdf);
    }
}