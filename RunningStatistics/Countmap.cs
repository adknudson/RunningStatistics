using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the internal dictionary.
/// </summary>
public class Countmap<TObs> : 
    IReadOnlyDictionary<TObs, long>, 
    IRunningStatistic<TObs, IDictionary<TObs, long>, Countmap<TObs>> 
    where TObs : notnull
{
    public Countmap()
    {
        Value = new Dictionary<TObs, long>();
        Nobs = 0;
    }
    
    
    
    public long Nobs { get; private set; }

    public IDictionary<TObs, long> Value { get; }
    
    /// <summary>
    /// The number of unique observations in the countmap.
    /// </summary>
    public int Count => Value.Count;
    
    public long this[TObs key] => Value.TryGetValue(key, out var value) ? value : 0;

    public IEnumerable<TObs> Keys => Value.Keys;

    public IEnumerable<long> Values => Value.Values;

    public TObs Mode => Value.MaxBy(kvp => kvp.Value).Key;

    
    
    public void Fit(IEnumerable<TObs> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(TObs value)
    {
        Fit(value, 1);
    }

    public void Fit(TObs obs, long k)
    {
        Nobs += k;

        if (Value.ContainsKey(obs))
        {
            Value[obs] += k;
        }
        else
        {
            Value[obs] = k;
        }
    }

    public void Merge(Countmap<TObs> countmap)
    {
        foreach (var (key, value) in countmap.Value)
        {
            Fit(key, value);
        }
    }

    public void Reset()
    {
        Value.Clear();
        Nobs = 0;
    }

    public Countmap<TObs> CloneEmpty()
    {
        return new Countmap<TObs>();
    }

    public Countmap<TObs> Clone()
    {
        var countmap = new Countmap<TObs>();
        
        foreach (var (key, nobs) in Value)
        {
            countmap.Fit(key, nobs);
        }

        return countmap;
    }

    public static Countmap<TObs> Merge(Countmap<TObs> first, Countmap<TObs> second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public bool ContainsKey(TObs key) => Value.ContainsKey(key);


    
    public IDictionary<TObs, long> AsDictionary() => Value;

    public Dictionary<TObs, long> ToDictionary() => new(Value);

    public SortedDictionary<TObs, long> ToSortedDictionary() => new(Value);

    public IEnumerable<(TObs Value, double Probability)> ValueProbabilityPairs()
    {
        foreach (var (value, count) in Value)
        {
            yield return (value, (double) count / Nobs);
        }
    }

    public bool TryGetValue(TObs key, out long value) => Value.TryGetValue(key, out value);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => Value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override string ToString() => $"{typeof(Countmap<TObs>)}(n={Nobs}) with {Value.Keys.Count} unique values.";

}
