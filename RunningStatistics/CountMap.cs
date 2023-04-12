using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the internal dictionary.
/// </summary>
public class CountMap<TObs> : IReadOnlyDictionary<TObs, long>,
    IRunningStatistic<TObs, IDictionary<TObs, long>, CountMap<TObs>> where TObs : notnull
{
    public CountMap()
    {
        Value = new Dictionary<TObs, long>();
        Nobs = 0;
    }

    public CountMap(IDictionary<TObs, long> dictionary)
    {
        Value = dictionary;
        Nobs = dictionary.Values.Sum();
    }



    public long Nobs { get; private set; }

    public IDictionary<TObs, long> Value { get; }

    public long this[TObs key] => Value.TryGetValue(key, out var value) ? value : 0;

    public IEnumerable<TObs> Keys => Value.Keys;

    public IEnumerable<long> Values => Value.Values;

    
    
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

    public void Merge(CountMap<TObs> countMap)
    {
        foreach (var (key, value) in countMap.Value)
        {
            Fit(key, value);
        }
    }

    public void Reset()
    {
        Value.Clear();
        Nobs = 0;
    }

    public CountMap<TObs> CloneEmpty()
    {
        return new CountMap<TObs>();
    }

    public CountMap<TObs> Clone()
    {
        var countmap = new CountMap<TObs>();
        
        foreach (var (key, nobs) in Value)
        {
            countmap.Fit(key, nobs);
        }

        return countmap;
    }

    public static CountMap<TObs> Merge(CountMap<TObs> first, CountMap<TObs> second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public bool ContainsKey(TObs key) => Value.ContainsKey(key);

    public bool TryGetValue(TObs key, out long value) => Value.TryGetValue(key, out value);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => Value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override string ToString() => $"{typeof(CountMap<TObs>)}(n={Nobs}) with {Value.Keys.Count} unique values.";

    int IReadOnlyCollection<KeyValuePair<TObs, long>>.Count => Value.Count;
}
