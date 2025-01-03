using System.Collections.ObjectModel;

namespace RunningStatistics.Examples;

/// <summary>
/// An example of implementing a collection of <see cref="IRunningStatistic{TObs}"/> objects.
/// </summary>
public sealed class StatsCollection<TObs> : Collection<IRunningStatistic<TObs>>,
    IRunningStatistic<TObs, StatsCollection<TObs>>
{
    public long Nobs { get; private set; }


    public void Fit(TObs value)
    {
        Fit(value, 1);
    }

    public void Fit(TObs value, long count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");
        }

        Nobs += count;

        foreach (var stat in Items)
        {
            stat.Fit(value, count);
        }
    }

    public void Fit(IEnumerable<TObs> values)
    {
        var xs = values.ToList();
        Nobs += xs.Count;

        foreach (var stat in Items)
        {
            stat.Fit(xs);
        }
    }

    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        foreach (var keyValuePair in keyValuePairs)
        {
            Fit(keyValuePair.Key, keyValuePair.Value);
        }
    }

    public void Reset()
    {
        Nobs = 0;

        foreach (var stat in Items)
        {
            stat.Reset();
        }
    }

    public StatsCollection<TObs> CloneEmpty()
    {
        var clone = new StatsCollection<TObs>();

        foreach (var stat in Items)
        {
            clone.Add(stat.CloneEmpty());
        }

        return clone;
    }

    public StatsCollection<TObs> Clone()
    {
        var clone = new StatsCollection<TObs>
        {
            Nobs = Nobs
        };

        foreach (var stat in Items)
        {
            clone.Add(stat.Clone());
        }

        return clone;
    }

    public void Merge(StatsCollection<TObs> other)
    {
        foreach (var (thisStat, otherStat) in this.Zip(other))
        {
            // crude check to ensure that the statistics are of the same type
            if (thisStat.GetType() != otherStat.GetType())
            {
                throw new InvalidOperationException("Cannot merge different types of statistics");
            }

            thisStat.UnsafeMerge(otherStat);
        }
    }

    IRunningStatistic<TObs> IRunningStatistic<TObs>.CloneEmpty() => CloneEmpty();

    IRunningStatistic<TObs> IRunningStatistic<TObs>.Clone() => Clone();

    public void UnsafeMerge(IRunningStatistic<TObs> other) => Merge((StatsCollection<TObs>)other);
}