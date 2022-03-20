using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences.
/// </summary>
/// <typeparam name="T">The observation type.</typeparam>
public class Countmap<T> : IRunningStat<T, Countmap<T>>, IEnumerable<KeyValuePair<T, int>>
{
    private readonly SortedDictionary<T, int> _counter;

    
    
    public Countmap()
    {
        _counter = new SortedDictionary<T, int>();
        Count = 0;
    }

    public Countmap(Countmap<T> other)
    {
        _counter = new SortedDictionary<T, int>(other._counter);
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

    public void Merge(Countmap<T> other)
    {
        foreach (var (key, value) in other)
        {
            Fit(key, value);
        }
    }

    public void Reset()
    {
        _counter.Clear();
        Count = 0;
    }

    public void Print(StreamWriter stream)
    {
        stream.WriteLine(ToString());
        stream.WriteLine("Key\tCount");
        foreach (var (key, value) in this)
        {
            stream.WriteLine($"{key}\t{value}");
        }
    }

    public override string ToString() => $"{typeof(Countmap<T>)}(n={Count})";


    public long Count { get; private set; }
    public T Mode => _counter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    public int this[T key] => _counter.ContainsKey(key) ? _counter[key] : 0;
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => _counter.GetEnumerator();



    public static Countmap<T> operator +(Countmap<T> a, Countmap<T> b)
    {
        return Merge(a, b);
    }

    private static Countmap<T> Merge(Countmap<T> a, Countmap<T> b)
    {
        Countmap<T> merged = new(a);
        merged.Merge(b);
        return merged;
    }
}