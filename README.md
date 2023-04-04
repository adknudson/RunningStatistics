# RunningStatistics
Online (single pass) algorithms for statistical measures based on the Julia package 
[OnlineStats.jl](https://github.com/joshday/OnlineStats.jl) by [Josh Day](https://github.com/joshday). Useful for streaming and big data.

## List of Statistics

| Statistic          | Description                                     |
|:-------------------|:------------------------------------------------|
| Mean               | The univariate mean                             |
| Sum\<T\>           | The overall sum of any INumber\<T\> type        |
| Variance           | The univariate variance                         |
| Extrema            | The min and max observations and their counts   |
| Moments            | Mean, Variance, Skewness, and (excess) Kurtosis |
| EmpiricalCdf       | Approximate order statistics (quantiles)        |
| CountMap\<T\>      | Counts for each unique value                    |
 | ProportionMap\<T\> | Proportions for each unique value               |
| Histogram          | A histogram with specified bin edges            |
| Normal             | The univariate mean and variance                |

## Methods

The `IRunningStatistic<in TObs>` interface provides the following members:

```csharp
public interface IRunningStatistic<in TObs>
{
    public long Nobs { get; }

    public void Fit(IEnumerable<TObs> values);

    public void Fit(TObs value);

    public void Reset();
}
```

The `IRunningStatistic<in TObs, out TValue>` interface provides the additional member:

```csharp
public interface IRunningStatistic<in TObs, out TValue> : IRunningStatistic<TObs>
{
    public TValue Value { get; }
}
```

Finally each statistic implements `IRunningStatistic<in TObs, out TValue, TSelf>` to ensure a consistent set of functionality:

```csharp
public interface IRunningStatistic<in TObs, out TValue, TSelf> : IRunningStatistic<TObs, TValue> where TSelf : IRunningStatistic<TObs, TValue, TSelf>
{
    public TSelf CloneEmpty();

    public TSelf Clone();

    public void Merge(TSelf other);

    public static abstract TSelf Merge(TSelf first, TSelf second);
}
```

Therefore merging can only be done when the concrete classes are known, and concrete classes may be cloned.

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
