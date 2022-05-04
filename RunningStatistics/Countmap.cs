using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the internal dictionary.
/// </summary>
/// <typeparam name="T">The observation type.</typeparam>
public class Countmap<T> : IRunningStatistic<T>, IReadOnlyDictionary<T, long>
{
    private readonly IDictionary<T, long> _counter;


    public Countmap()
    {
        _counter = new Dictionary<T, long>();
        Count = 0;
    }

    public Countmap(Countmap<T> other)
    {
        _counter = new Dictionary<T, long>(other._counter);
        Count = other.Count;
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

    public void Fit(T obs, long k)
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

    public static Countmap<T> Merge(Countmap<T> a, Countmap<T> b)
    {
        var c = new Countmap<T>(a);
        c.Merge(b);
        return c;
    }

    public void Reset()
    {
        _counter.Clear();
        Count = 0;
    }



    public long Count { get; private set; }
    public T Mode => _counter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    public long this[T key] => _counter.TryGetValue(key, out var value) ? value : 0;
    public IEnumerable<T> Keys => _counter.Keys;
    public IEnumerable<long> Values => _counter.Values;
    
    
    /// <summary>
    /// This will always return <c>true</c>. It will either return the count for the given key, or return 0.
    /// For this reason, you should use an indexer and check if the count is non-zero.
    /// </summary>
    bool IReadOnlyDictionary<T, long>.TryGetValue(T key, out long value)
    {
        if (!_counter.TryGetValue(key, out value))
        {
            value = 0;
        }

        return true;
    }

    /// <summary>
    /// Returns <c>true</c> if the key has been observed, and <c>false</c> otherwise. 
    /// </summary>
    public bool ContainsKey(T key) => _counter.ContainsKey(key);
    public IEnumerable<int> Values => _counter.Values;
    public IEnumerable<double> Probabilities => Values.Select(s => (double) s / Count);
    public IEnumerable<T> Keys => _counter.Keys;

    /// <summary>
    /// Return the counter as a <see cref="SortedDictionary{TKey,TValue}"/>, sorted by the keys.
    /// </summary>
    public SortedDictionary<T, long> AsSortedDictionary() => new(_counter);

    /// <summary>
    /// Please use the <see cref="Count"/> property as it uses an Int64 value to store the count. This method will
    /// throw an exception if the count is too big for an Int32.
    /// </summary>
    int IReadOnlyCollection<KeyValuePair<T, long>>.Count => Convert.ToInt32(Count);
    
    public IEnumerator<KeyValuePair<T, long>> GetEnumerator() => _counter.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override string ToString() => $"{typeof(Countmap<T>)}(n={Count})";
}
