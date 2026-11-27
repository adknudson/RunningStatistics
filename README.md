# RunningStatistics

Online (single-pass) algorithms for statistical measures. Useful for streaming and big data.

This package is [available on NuGet](https://www.nuget.org/packages/RunningStatistics/)

```
dotnet add package RunningStatistics
```

Supports .NET Standard 2.0

## Statistics

| Statistic     | Fits                                                               | Reads out                                                                                            |
|:--------------|:-------------------------------------------------------------------|:-----------------------------------------------------------------------------------------------------|
| Mean          | `double`                                                           | `Value`                                                                                              |
| Sum           | `double`                                                           | `Value`, `Mean`                                                                                      |
| Sum\<T\>      | any type implementing `IAdditionOperators` and `IAdditiveIdentity` | `Value`; `Mean` only for `int`, `long`, `double`, `decimal`                                          |
| Variance      | `double`                                                           | `Value`, `StandardDeviation`                                                                         |
| Extrema       | `double`                                                           | `Min`, `Max`, `MinCount`, `MaxCount`, `Range`                                         |
| Extrema\<T\>  | any type implementing `IComparable<T>`                             | same as `Extrema`, less `Range`: that read stays an extension method                                  |
| Moments       | `double`                                                           | `Mean`, `Variance`, `StandardDeviation`, `Skewness`, `Kurtosis`, `ExcessKurtosis`                    |
| EmpiricalCdf  | `double`, into `numBins` bins (default 200)                        | `NumBins`, `Quantile(p)`, `Cdf(x)`, `Median`, `Min`, `Max`                                           |
| Histogram     | `double`, into bins given as `edges`                               | `NumBins`, `Bins`, `Edges`, `OutOfBoundsCounts`, iteration over `Bin`s with `Lower`, `Upper`, `Nobs` |
| CountMap\<T\> | any type                                                           | `Count`, `Keys`, `Values`, `Mode`, `Median`, `Quantile(p)`, `MinKey`, `MaxKey`, and read-only `Dictionary` access |
| Normal        | `double`                                                           | `Mean`, `Variance`, `StandardDeviation`, `Pdf(x)`, `Cdf(x)`, `Quantile(p)`                           |
| Beta          | `bool`                                                             | `Successes`, `Failures`, `Mean`, `Median`, `Mode`, `Variance`, `Pdf(x)`, `Cdf(x)`                    |

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
most generic and allows mixed running statistics in a collection. `IRunningStatistic<TObs>`
adds the type of observations the running statistic can fit. `IRunningStatistic<TObs, TSelf>`
adds the concrete type of the running statistic, and allows precise cloning and merging.

### Abstract Base Class

The abstract class `RunningStatisticBase` implements the `IRunningStatistic<TObs, TSelf>` interface
and provides default implementations when possible.

## Examples

See the `RunningStatistics.Examples` project for more examples. `UsingACollectionOfStatistics.cs`
mixes different statistics in one collection. `ImplementingACompoundStatistic.cs` builds a new
statistic out of existing ones.

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

## Notes

- Readout getters return `NaN` when there are too few observations. `Variance` returns `NaN`
  while `Nobs < 2`, `EmpiricalCdf.Quantile`, `Cdf` and `Median` return `NaN` until every bin
  holds observations, and other statistics have their own thresholds. One exception: `Sum.Value`
  returns 0 when nothing is fitted, because an empty sum is defined.
- Statistics over generic observation types have no `NaN` to return. `Extrema<T>.Min` and
  `Max` throw `InvalidOperationException`, and `CountMap<T>.MinKey`, `MaxKey`, `Mode` and
  `Quantile(p)` throw `KeyNotFoundException` when nothing is fitted. `TryGetMin`/`TryGetMax` and
  `TryGetMinKey`/`TryGetMaxKey` read the same values without throwing. `Sum<decimal>` has
  no `NaN` either: its `Mean` throws `InvalidOperationException` while no observations are
  fitted, where the `int`, `long` and `double` overloads answer `NaN`.
- Input guards differ by statistic, and the difference is deliberate. `Mean`, `Sum`, `Variance`,
  `Normal`, `Moments` and `EmpiricalCdf` require every observation to be finite: `NaN` or ±Infinity
  throws `ArgumentException`. `Extrema` and `Histogram` take ±Infinity, because a minimum, a
  maximum and an out-of-bounds count are defined there, and throw `ArgumentException` only for `NaN`.
  The generic `Extrema<T>` rejects a null observation.
- A `null` collection is rejected with `ArgumentNullException` before anything is fitted.
- `Histogram.Bin` is a nested supporting type. A `Histogram` iterates as `Bin`s, each with
  `Lower`, `Upper` and `Nobs`. `Histogram.OutOfBoundsCounts` reports observations below or above
  the bins as a `(long Lower, long Upper)` tuple.
- A readout that takes no argument is a property: `Sum.Mean`, `Extrema.Range`, `Beta.Mean`,
  `CountMap.MinKey`, `MaxKey`, `Mode` and `Median` read as properties, and `Sum.Value` sits
  beside them. A readout that takes an argument stays a method, because it has to: `Quantile(p)`,
  `Pdf(x)` and `Variance(mean, corrected)` pass what they need.
- The `Extensions` classes keep only the generic arithmetic reads — `Extrema<T>.Range` and the
  `CountMap<T>` `Sum`, `Mean`, `Variance`, `StandardDeviation`, `Skewness`, `Kurtosis` and
  `ExcessKurtosis`. Each of those needs an operator constraint (`T` must subtract, divide, add)
  that its class does not declare, and a member cannot carry a constraint the class lacks. Putting
  the constraint on the class would rule out ordinary uses such as `Extrema<MyComparable>`, so those
  reads stay extension methods and only the constraint-free reads moved inside.
- `GenericSumExtensions.Mean` covers `Sum<int>`, `Sum<long>`, `Sum<double>` and `Sum<decimal>`,
  the types the library can divide by a count without inventing an operator. There is no average
  for an arbitrary observation type: a count is not a quantity of the measured thing, so the
  library cannot ask a type to divide a total by one. A caller writes the average it wants,
  `sum.Value / sum.Nobs` when the type divides by a `long`, or `(double)sum.Value / sum.Nobs`
  when the answer should be a `double`.
- `CountMap` orders its keys by the comparer you built it with. `MinKey`, `MaxKey`, `Median` and
  `Quantile(p)` use that comparer; `TryGetMinKey(comparer, out key)`, `TryGetMaxKey(comparer, out key)`
  and `Quantile(p, comparer)` take a different one, and `Keys.Min(comparer)` gets the same answer
  through LINQ.

## Development

Contributors and automated agents should read `AGENTS.md`, which carries the
algorithm-improvement workflow, and `DEVELOPERS.md`, which defines the
vocabulary the tests and plans use ("gate", "bench fact", "band", "κ-sweep").

## Credits

Based on the Julia package [OnlineStats.jl](https://github.com/joshday/OnlineStats.jl)
by [Josh Day](https://github.com/joshday).
