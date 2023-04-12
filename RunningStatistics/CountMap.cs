using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the internal dictionary.
/// </summary>
public class CountMap<TObs> : AbstractRunningStatistic<TObs, CountMap<TObs>>, IReadOnlyDictionary<TObs, long> where TObs : notnull
{
    private readonly IDictionary<TObs, long> _dict;
    
    
    
    public CountMap()
    {
        _dict = new Dictionary<TObs, long>();
    }

    public CountMap(IDictionary<TObs, long> dictionary)
    {
        _dict = dictionary;
        Nobs = dictionary.Values.Sum();
    }


    
    public long this[TObs key] => _dict.TryGetValue(key, out var value) ? value : 0;

    public IEnumerable<TObs> Keys => _dict.Keys;

    public IEnumerable<long> Values => _dict.Values;

    public int NumUnique => _dict.Count;

    
    
    public override void Fit(TObs value)
    {
        Fit(value, 1);
    }

    /// <summary>
    /// Fit multiple counts of the same observation.
    /// </summary>
    public void Fit(TObs obs, long count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative.");
        
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

    public override void Merge(CountMap<TObs> countMap)
    {
        foreach (var (key, value) in countMap._dict)
        {
            Fit(key, value);
        }
    }

    public override void Reset()
    {
        _dict.Clear();
        Nobs = 0;
    }

    public override CountMap<TObs> CloneEmpty()
    {
        return new CountMap<TObs>();
    }

    public override CountMap<TObs> Clone()
    {
        var countmap = new CountMap<TObs>();
        
        foreach (var (key, nobs) in _dict)
        {
            countmap.Fit(key, nobs);
        }

        return countmap;
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
