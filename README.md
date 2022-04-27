# RunningStatistics
Online (single pass) algorithms for statistical measures based on the Julia package 
[OnlineStats.jl](https://github.com/joshday/OnlineStats.jl) by [Josh Day](https://github.com/joshday). Useful for streaming and big data.

## List of Statistics

| Statistic    | Description                                     |
|:-------------|:------------------------------------------------|
| Mean         | The univariate mean                             |
| Sum          | The overall sum                                 |
| Variance     | The univariate variance                         |
| Extrema      | The min and max observations and their counts   |
| Moments      | Mean, Variance, Skewness, and (excess) Kurtosis |
| EmpiricalCdf | Approximate order statistics (quantiles)        |
| Countmap     | Counts for each unique value                    |
| Histogram    | A histogram with specified bin edges            |

In addition to the above statistics, we also provide the `Series` type which allows for fitting a collection of running statistics.

## Methods

The `IRunningStatistic<T>` interface provides the following members:

```csharp
public interface IRunningStatistic<TObs>
{
    public long Count { get; }

    public void Fit(IEnumerable<TObs> values);

    public void Fit(TObs value);

    public void Reset();
    
    public void Merge(IRunningStatistic<TObs> other);
}
```

Do note that if two running statistics have different concrete classes, then no merging will be performed.

## Examples

```csharp
using RunningStatistics;

var mean1 = new Mean();
var mean2 = new Mean();
var ecdf = new EmpiricalCdf();
var series = new Series(new Moments(), new Extrema()); 

var rng = new Random();
for (var i = 0; i < 1000; i++)
{
    var x = rng.NextDouble();
    
    mean1.Fit(x);
    mean2.Fit(2*x);
    
    ecdf.Fit(x);
    series.Fit(x);
}

mean1.Merge(mean2);
var q1 = ecdf.Quantile(0.25);
```
