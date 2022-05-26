using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A collection of <see cref="IRunningStatistic{TObs}"/>s that all get fit to the same observations.
/// </summary>
public class Series<T> : IRunningStatistic<T>, IEnumerable<IRunningStatistic<T>>
{
    private readonly IList<IRunningStatistic<T>> _statistics;


    public Series(params IRunningStatistic<T>[] statistics)
    {
        _statistics = statistics.ToList();
        Count = 0;
    }
    
    
    public long Count { get; private set; }
    public IRunningStatistic<T> this[int index] => _statistics[index];


    public void Fit(IEnumerable<T> values)
    {
        var ys = values.ToList();
        Count += ys.Count;

        foreach (var statistic in _statistics)
        {
            statistic.Fit(ys);
        }
    }

    public void Fit(T value)
    {
        Count++;
        foreach (var statistic in _statistics)
        {
            statistic.Fit(value);
        }
    }
    
    public void Merge(IRunningStatistic<T> other)
    {
        if (other is not Series<T> series) return;
        
        Count += series.Count;
        for (var i = 0; i < _statistics.Count; i++)
        {
            _statistics[i].Merge(series._statistics[i]);
        }
    }

    public void Reset()
    {
        Count = 0;
        foreach (var statistic in _statistics)
        {
            statistic.Reset();
        }
    }

    public IEnumerator<IRunningStatistic<T>> GetEnumerator() => _statistics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Print(StreamWriter stream)
    {
        stream.WriteLine($"{GetType()}(n={Count})");
        foreach (var statistic in _statistics)
        {
            statistic.Print(stream);
            stream.WriteLine();
        }
    }
}