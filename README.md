# RunningStatistics
Online (single pass) algorithms for statistical measures based on the Julia package 
[OnlineStats.jl](https://github.com/joshday/OnlineStats.jl) by [Josh Day](https://github.com/joshday). Useful for streaming and big data.


## List of Statistics

| Statistic          | Description                                                                                                                     |
|:-------------------|:--------------------------------------------------------------------------------------------------------------------------------|
| Mean               | The univariate mean                                                                                                             |
| Sum                | The overall sum of `double` observations                                                                                        |
| Sum\<T\>           | The overall sum of any type that implements `IAdditionOperators` and `IAdditiveIdentity` (requires .NET7 or higher)             |
| Variance           | The univariate variance                                                                                                         |
| Extrema            | The min and max observations and their counts                                                                                   |
| Extrema\<T\>       | The min and max observations of any type that implements `IMinMaxValue` and `IComparisonOperators` (requires .NET7 or higher)   |
| Moments            | Mean, Variance, Skewness, and (excess) Kurtosis                                                                                 |
| EmpiricalCdf       | Approximate order statistics (quantiles)                                                                                        |
| CountMap\<T\>      | Counts for each unique value                                                                                                    |
| ProportionMap\<T\> | Proportions for each unique value                                                                                               |
| Histogram          | A histogram with specified bin edges                                                                                            |



## List of Distributions

| Distribution | Description                          |
|:-------------|:-------------------------------------|
| Normal       | The univariate mean and variance     |
| Beta         | The number of successes and failures |



## Common Interface

All running statistics implement the following interfaces:

```csharp
public interface IRunningStatistic<TObs>
{
    public long Nobs { get; }
    
    public void Fit(TObs value);

    public void Fit(IEnumerable<TObs> values);

    public void Reset();
    
    public IRunningStatistic<TObs> CloneEmpty();
    
    public IRunningStatistic<TObs> Clone();
    
    public void Merge(IRunningStatistic<TObs> other);
}

public interface IRunningStatistic<TObs, TSelf> : IRunningStatistic<TObs> 
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    public TSelf CloneEmpty();

    public TSelf Clone();
    
    public void Merge(TSelf other);
}
```

The interface is split into a base interface `IRunningStatistic<TObs>` and a derived interface 
`IRunningStatistic<TObs, TSelf>`. The base interface contains the generic methods, while the 
derived interface contains more type information. The base interface allows for collections of 
statistics that can be fit to the same data.

## Examples

```csharp
using RunningStatistics;

var mean1 = new Mean();
var mean2 = new Mean();
var ecdf = new EmpiricalCdf();

var rng = new Random();
for (var i = 0; i < 1000; i++)
{
    var x = rng.NextDouble();
    
    mean1.Fit(x);
    mean2.Fit(2*x);
    
    ecdf.Fit(x);
}

mean1.Merge(mean2);
var q1 = ecdf.Quantile(0.25);
```

## Collections of Statistics

Different statistics are intended to be used on their own. If a collection of statistics is 
appropriate, then we recommend that you write your own class that implements the `IRunningStatistic<TObs, YourClass>` 
interface and use the desired statistics as private members. Example:

```csharp
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
    
    public void Merge(IRunningStatistic<double> other)
    {
        if (other is StatsCollection stats)
        {
            Merge(stats);
        }
    }
    
    public void Merge(StatsCollection other)
    {
        CountMap.Merge(other.CountMap);
        Ecdf.Merge(other.Ecdf);
    }
}
```

Notice that in this class, the number of observations is retrieved from `Ecdf.Nobs`, and therefore 
`Nobs` never needs to be updated in the `Fit` method.