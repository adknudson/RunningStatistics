# RunningStatistics
Online (single pass) algorithms for statistical measures based on the Julia package 
[OnlineStats.jl](https://github.com/joshday/OnlineStats.jl) by [Josh Day](https://github.com/joshday). Useful for streaming and big data.

This package is [available on NuGet](https://www.nuget.org/packages/RunningStatistics/)

Supports .NET Standard 2.0

## List of Statistics

| Statistic          | Description                                                                              |
|:-------------------|:-----------------------------------------------------------------------------------------|
| Mean               | The univariate mean                                                                      |
| Sum                | The overall sum of `double` observations                                                 |
| Sum\<T\>           | The overall sum of any type that implements `IAdditionOperators` and `IAdditiveIdentity` |
| Variance           | The univariate variance                                                                  |
| Extrema            | The min and max of `double` observations                                                 |
| Extrema\<T\>       | The min and max of any type that implements `IComparable<T>`                             |
| Moments            | The Mean, Variance, Skewness, and (excess) Kurtosis                                      |
| EmpiricalCdf       | Approximate order statistics (quantiles)                                                 |
| CountMap\<T\>      | Counts for each unique value                                                             |
| Histogram          | A histogram with specified bin edges                                                     |


## List of Distributions

| Distribution | Description                          |
|:-------------|:-------------------------------------|
| Normal       | The univariate mean and variance     |
| Beta         | The number of successes and failures |


## Common Interface

All running statistics implement the following interfaces:

```csharp
public interface IRunningStatistic
{
    public long Nobs { get; }

    public void Reset();

    public IRunningStatistic CloneEmpty();

    public IRunningStatistic Clone();

    public void UnsafeMerge(IRunningStatistic other);
}

public interface IRunningStatistic<TObs> : IRunningStatistic
{
    public void Fit(TObs value);

    public void Fit(TObs value, long count);

    public void Fit(IEnumerable<TObs> values);

    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs);

    public new IRunningStatistic<TObs> CloneEmpty();

    public new IRunningStatistic<TObs> Clone();

    public void UnsafeMerge(IRunningStatistic<TObs> other);
}

public interface IRunningStatistic<TObs, TSelf> : IRunningStatistic<TObs>
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    public new TSelf CloneEmpty();

    public new TSelf Clone();

    public void Merge(TSelf other);
}
```

The interface is layered as three interfaces of increasing specificity. `IRunningStatistic` is the
most generic and allows for mixed running statistics to be in a collection. `IRunningStatistic<TObs>`
expands by adding information about the type of observations that the running statistic can fit.
Finally `IRunningStatistic<TObs, TSelf>` adds information about the concrete type of the running
statistic, and allows for precise cloning and merging.

### Abstract Base Class

The abstract class `RunningStatisticBase` implements the `IRunningStatistic<TObs, TSelf>` interface
and provides default implementations when possible.

## Examples

See the `RunningStatistics.Examples` project for more examples.

### Basic Usage

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