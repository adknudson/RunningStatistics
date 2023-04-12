using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the internal dictionary.
/// </summary>
public class CountMap<TObs> : IReadOnlyDictionary<TObs, long>, IRunningStatistic<TObs, CountMap<TObs>> where TObs : notnull
{
    private readonly IDictionary<TObs, long> _dict;
    
    
    
    public CountMap()
    {
        _dict = new Dictionary<TObs, long>();
        Nobs = 0;
    }

    public CountMap(IDictionary<TObs, long> dictionary)
    {
        _dict = dictionary;
        Nobs = dictionary.Values.Sum();
    }



    public long Nobs { get; private set; }

    public long this[TObs key] => _dict.TryGetValue(key, out var value) ? value : 0;

    public IEnumerable<TObs> Keys => _dict.Keys;

    public IEnumerable<long> Values => _dict.Values;

    public int NumUnique => _dict.Count;

    
    
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

    public void Fit(TObs obs, long count)
    {
        Nobs += count;

        if (_dict.ContainsKey(obs))
        {
            _dict[obs] += count;
        }
        else
        {
            _dict[obs] = count;
        }
    }
    
    /// <summary>
    /// Fit a list of value-count pairs.
    /// </summary>
    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        foreach (var (value, count) in keyValuePairs)
        {
            Fit(value, count);
        }
    }

    public void Merge(CountMap<TObs> countMap)
    {
        foreach (var (key, value) in countMap._dict)
        {
            Fit(key, value);
        }
    }

    public void Reset()
    {
        _dict.Clear();
        Nobs = 0;
    }

    public CountMap<TObs> CloneEmpty()
    {
        return new CountMap<TObs>();
    }

    public CountMap<TObs> Clone()
    {
        var countmap = new CountMap<TObs>();
        
        foreach (var (key, nobs) in _dict)
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

    public bool ContainsKey(TObs key) => _dict.ContainsKey(key);

    public bool TryGetValue(TObs key, out long value) => _dict.TryGetValue(key, out value);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override string ToString() => $"{typeof(CountMap<TObs>)} Nobs={Nobs} | {NumUnique} unique values";

    int IReadOnlyCollection<KeyValuePair<TObs, long>>.Count => _dict.Count;

    public IDictionary<TObs, long> AsDictionary() => _dict;

    public IDictionary<TObs, long> ToDictionary() => new Dictionary<TObs, long>(_dict);
}
