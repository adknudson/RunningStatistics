using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences.
/// </summary>
/// <typeparam name="T">The observation type.</typeparam>
public class Countmap<T> : IRunningStatistic<T>, IEnumerable<KeyValuePair<T, int>>
{
    private readonly IDictionary<T, int> _counter;


    public Countmap()
    {
        _counter = new Dictionary<T, int>();
        Count = 0;
    }
    

    public void Fit(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(T value)
    {
        Fit(value, 1);
    }

    public void Fit(T obs, int k)
    {
        Count += k;

        if (_counter.ContainsKey(obs))
        {
            _counter[obs] += k;
        }
        else
        {
            _counter[obs] = k;
        }
    }

    public void Merge(IRunningStatistic<T> other)
    {
        if (other is not Countmap<T> countmap) return;
        
        foreach (var (key, value) in countmap._counter)
        {
            Fit(key, value);
        }
    }

    public void Reset()
    {
        _counter.Clear();
        Count = 0;
    }

    public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => _counter.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"{typeof(Countmap<T>)}(n={Count})";

    public long Count { get; private set; }
    public T Mode => _counter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    public int this[T key] => _counter.TryGetValue(key, out var value) ? value : 0;
    public bool ContainsKey(T key) => _counter.ContainsKey(key);

    public SortedDictionary<T, int> ToSortedDictionary() => new(_counter);
}