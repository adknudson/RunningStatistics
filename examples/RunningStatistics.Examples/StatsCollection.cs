namespace RunningStatistics.Examples;

public class StatsCollection : AbstractRunningStatistic<double, StatsCollection>
{
    public StatsCollection()
    {
        CountMap = new CountMap<double>();
        Ecdf = new EmpiricalCdf();
    }
    
    
    
    private CountMap<double> CountMap { get; init; }
    private EmpiricalCdf Ecdf { get; init; }



    protected override long GetNobs() => Ecdf.Nobs;

    public override void Fit(double value)
    {
        CountMap.Fit(value);
        Ecdf.Fit(value);
    }

    public override void Reset()
    {
        CountMap.Reset();
        Ecdf.Reset();
    }

    public override StatsCollection CloneEmpty() => new();

    public override StatsCollection Clone()
    {
        return new StatsCollection
        {
            CountMap = CountMap.Clone(),
            Ecdf = Ecdf.Clone()
        };
    }

    public override void Merge(StatsCollection other)
    {
        CountMap.Merge(other.CountMap);
        Ecdf.Merge(other.Ecdf);
    }
}