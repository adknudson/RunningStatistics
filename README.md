# RunningStatistics
Online (single pass) algorithms for statistical measures based on the Julia package 
[OnlineStats.jl](https://github.com/joshday/OnlineStats.jl) by [Josh Day](https://github.com/joshday). Useful for streaming and big data.


## List of Statistics

| Statistic           | Description                                     |
|:--------------------|:------------------------------------------------|
| Mean                | The univariate mean                             |
| Sum\<T\>            | The overall sum of any INumber\<T\> type        |
| Variance            | The univariate variance                         |
| Extrema             | The min and max observations and their counts   |
| Moments             | Mean, Variance, Skewness, and (excess) Kurtosis |
| EmpiricalCdf        | Approximate order statistics (quantiles)        |
| CountMap\<T\>       | Counts for each unique value                    |
| ProportionMap\<T\>  | Proportions for each unique value               |
| Histogram           | A histogram with specified bin edges            |


## List of Distributions

| Distribution | Description                                         |
|:-------------|:----------------------------------------------------|
| Normal       | The univariate mean and variance                    |


## Common Interface

All running statistics inherit from the following abstract class:

```csharp
public abstract class AbstractRunningStatistic<TObs, TSelf> where TSelf : AbstractRunningStatistic<TObs, TSelf>
{
    public virtual long Nobs { get; protected set; }

    public abstract void Fit(TObs value);

    public virtual void Fit(IEnumerable<TObs> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public abstract void Reset();

    public abstract TSelf CloneEmpty();

    public abstract TSelf Clone();

    public abstract void Merge(TSelf other);

    public static TSelf Merge(TSelf first, TSelf second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }
}

```


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

## Combining Statistics

Different statistics are intended to be used on their own. If a collection of statistics is appropriate, then we recommend that you write your own class that inherits from `AbstractRunningStatistic<TObs, YourClass>` and use the desired statistics as private members. Example:

```csharp
public class MyClass : AbstractRunningStatistic<double, MyClass>
{
    private EmpiricalCdf Ecdf { get; init; } = new();
    
    private CountMap<double> CountMap { get; init; } = new();

    public override long Nobs => Ecdf.Nobs;


    public override void Fit(double value)
    {
        Ecdf.Fit(value);
        CountMap.Fit(value);
    }

    public override void Reset()
    {
        Ecdf.Reset();
        CountMap.Reset();
    }

    public override MyClass CloneEmpty()
    {
        return new MyClass();
    }

    public override MyClass Clone()
    {
        return new MyClass
        {
            Ecdf = Ecdf.Clone(),
            CountMap = CountMap.Clone()
        };
    }

    public override void Merge(MyClass other)
    {
        Ecdf.Merge(other.Ecdf);
        CountMap.Merge(other.CountMap);
    }
}
```