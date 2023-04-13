using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the internal dictionary.
/// </summary>
public sealed class CountMap<TObs> : AbstractRunningStatistic<TObs, CountMap<TObs>>, IReadOnlyDictionary<TObs, long> where TObs : notnull
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

    /// <summary>
    /// The number of unique observations that have been fitted.
    /// </summary>
    public int NumUniqueObs => _dict.Count;

    
    
    public override void Fit(TObs value)
    {
        UncheckedFit(value, 1);
    }

    /// <summary>
    /// Fit multiple counts of the same observation.
    /// </summary>
    public void Fit(TObs value, long count)
    {
        switch (count.CompareTo(0))
        {
            case -1:
                throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative.");
            case 0:
                return;
            case 1:
                UncheckedFit(value, count);
                break;
        }
    }
    
    /// <summary>
    /// Fit a list of value-count pairs.
    /// </summary>
    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        foreach (var kvp in keyValuePairs)
        {
            Fit(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Fit the value without checking if the count is non-negative.
    /// </summary>
    private void UncheckedFit(TObs value, long count)
    {
        Nobs += count;

        if (_dict.ContainsKey(value))
        {
            _dict[value] += count;
        }
        else
        {
            _dict[value] = count;
        }
    }

    public override void Merge(CountMap<TObs> countMap)
    {
        foreach (var kvp in countMap._dict)
        {
            Fit(kvp.Key, kvp.Value);
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
        
        foreach (var kvp in _dict)
        {
            countmap.UncheckedFit(kvp.Key, kvp.Value);
        }

        return countmap;
    }

    public bool ContainsKey(TObs key) => _dict.ContainsKey(key);

    public bool TryGetValue(TObs key, out long value) => _dict.TryGetValue(key, out value);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override string ToString() => $"{typeof(CountMap<TObs>)} Nobs={Nobs} | {NumUniqueObs} unique values";

    int IReadOnlyCollection<KeyValuePair<TObs, long>>.Count => _dict.Count;

    /// <summary>
    /// Returns the current object as a dictionary of Observation-Count pairs.
    /// </summary>
    public IDictionary<TObs, long> AsDictionary() => _dict;

    /// <summary>
    /// Returns a new dictionary with the current Observation-Count pairs.
    /// </summary>
    public IDictionary<TObs, long> ToDictionary() => new Dictionary<TObs, long>(_dict);

    /// <summary>
    /// Returns the current object as a <see cref="ProportionMap{TObs}"/>.
    /// </summary>
    public ProportionMap<TObs> AsProportionMap() => new(this);

    /// <summary>
    /// Returns a clone of the current object as a new <see cref="ProportionMap{TObs}"/>.
    /// </summary>
    public ProportionMap<TObs> ToProportionMap() => new(Clone());

    public TObs Mode()
    {
        var currentMode = this.First();

        foreach (var obs in this)
        {
            if (obs.Value > currentMode.Value)
            {
                currentMode = obs;
            }
        }

        return currentMode.Key;
    }
    
    public TObs Median()
    {
        var sortedData = this.OrderBy(kvp => kvp.Key).ToList();
        var sum = 0.0;
        
        if (sortedData.Count % 2 != 0) // if N is odd
        {
            foreach (var kvp in sortedData)
            {
                sum += (double) kvp.Value / Nobs;
                if (sum > 0.5) return kvp.Key;
            }
        }
        else // if N is even
        {
            foreach (var kvp in sortedData)
            {
                sum += (double)kvp.Value / Nobs;
                if (sum >= 0.5) return kvp.Key;
            }
        }

        throw new Exception("Median not found");
    }
}
