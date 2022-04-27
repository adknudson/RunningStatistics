using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class Series<T> : IRunningStatistic<T>, IEnumerable<IRunningStatistic<T>>
{
    private readonly List<IRunningStatistic<T>> _statistics;

    
    public Series(params IRunningStatistic<T>[] statistics)
    {
        _statistics = statistics.ToList();
        Count = 0;
    }
    
    
    public long Count { get; private set; }
    
    public IRunningStatistic<T> this[int index] => _statistics[index];


    public void Fit(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            Fit(value);
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

    public void Reset()
    {
        Count = 0;
        foreach (var statistic in _statistics)
        {
            statistic.Reset();
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

    public IEnumerator<IRunningStatistic<T>> GetEnumerator() => _statistics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}