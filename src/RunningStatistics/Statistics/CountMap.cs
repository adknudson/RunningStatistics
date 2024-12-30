﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace RunningStatistics;

/// <summary>
/// A dictionary that maps unique values to its number of occurrences. Accessing a non-existent key will return a count
/// of zero, however a new key will not be added to the count map.
/// </summary>
public sealed class CountMap<TObs> : RunningStatisticBase<TObs, CountMap<TObs>>, IReadOnlyDictionary<TObs, long> 
    where TObs : notnull
{
    private readonly Dictionary<TObs, long> _dict = new();
    private long _nobs;
    
    
    public CountMap()
    {
    }

    public CountMap(IDictionary<TObs, long> dictionary)
    {
        Fit(dictionary);
    }

    
    public long this[TObs key] => _dict.GetValueOrDefault(key, 0);
    
    /// <summary>
    /// The number of unique observations that have been fitted. Observations with a count of zero are not included.
    /// </summary>
    public int Count => _dict.Count;

    public IEnumerable<TObs> Keys => _dict.Keys;

    public IEnumerable<long> Values => _dict.Values;


    protected override long GetNobs() => _nobs;

    public override void Fit(TObs value) => UncheckedFit(value, 1);
    
    public void Fit(TObs value, long count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "The count must be non-negative");
        }
        
        if (count == 0) return;
        
        UncheckedFit(value, count);
    }
    
    public void Fit(IEnumerable<KeyValuePair<TObs, long>> keyValuePairs)
    {
        foreach (var kvp in keyValuePairs)
        {
            Fit(kvp.Key, kvp.Value);
        }
    }
    
    private void UncheckedFit(TObs value, long count)
    {
        _nobs += count;

        if (!_dict.TryAdd(value, count))
        {
            _dict[value] += count;
        }
    }

    public override void Merge(CountMap<TObs> countMap)
    {
        foreach (var kvp in countMap)
        {
            UncheckedFit(kvp.Key, kvp.Value);
        }
    }

    public override void Reset()
    {
        _dict.Clear();
        _nobs = 0;
    }

    public override CountMap<TObs> CloneEmpty() => new();

    public override CountMap<TObs> Clone()
    {
        var countMap = new CountMap<TObs>();
        
        foreach (var kvp in _dict)
        {
            countMap.UncheckedFit(kvp.Key, kvp.Value);
        }

        return countMap;
    }

    public TObs Mode()
    {
        if (Nobs == 0)
        {
            throw new Exception("Nobs = 0. The mode does not exist.");
        }
        
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
        if (Nobs == 0)
        {
            throw new Exception("Nobs = 0. The median does not exist.");
        }
        
        return Count % 2 == 0 ? MedianEvenCount() : MedianOddCount();
    }

    private TObs MedianEvenCount()
    {
        var cdf = 0.0;
        
        foreach (var kvp in _dict.OrderBy(kvp => kvp.Key)) 
        {
            cdf += (double)kvp.Value / Nobs;
            if (cdf >= 0.5) return kvp.Key;
        }
        
        // This should be unreachable...
        throw new Exception("Not able to find the median of the count map.");
    }
    
    private TObs MedianOddCount()
    {
        var cdf = 0.0;
        
        foreach (var kvp in _dict.OrderBy(kvp => kvp.Key)) 
        {
            cdf += (double)kvp.Value / Nobs;
            if (cdf > 0.5) return kvp.Key;
        }
        
        // This should be unreachable...
        throw new Exception("Not able to find the median of the count map.");
    }
    
    public bool ContainsKey(TObs key) => _dict.ContainsKey(key);

    public bool TryGetValue(TObs key, out long value) => _dict.TryGetValue(key, out value);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    protected override string GetStatsString() => $"{Count} unique observations";
}
