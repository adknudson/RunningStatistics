﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class ProportionMap<TObs> : AbstractRunningStatistic<TObs, ProportionMap<TObs>>, IReadOnlyDictionary<TObs, double> where TObs: notnull
{
    private readonly CountMap<TObs> _countMap;

    
    
    public ProportionMap()
    {
        _countMap = new CountMap<TObs>();
    }

    public ProportionMap(CountMap<TObs> countMap)
    {
        _countMap = countMap;
    }

    public ProportionMap(IDictionary<TObs, long> valueCounts)
    {
        _countMap = new CountMap<TObs>(valueCounts);
    }


    
    public double this[TObs key] => TryGetValue(key, out var value) ? value : default;

    int IReadOnlyCollection<KeyValuePair<TObs, double>>.Count => _countMap.NumUnique;

    public int NumUnique => _countMap.NumUnique;
    
    public IEnumerable<TObs> Keys => _countMap.Keys;

    public IEnumerable<double> Values => _countMap.Values.Select(count => (double) count / Nobs);

    

    protected override long GetNobs() => _countMap.Nobs;

    public override void Fit(IEnumerable<TObs> values) => _countMap.Fit(values);

    public override void Fit(TObs value) => _countMap.Fit(value);

    public void Fit(TObs obs, long count) => _countMap.Fit(obs, count);

    public override void Reset() => _countMap.Reset();
    
    public override ProportionMap<TObs> CloneEmpty() => new();

    public override ProportionMap<TObs> Clone()
    {
        var propMap = new ProportionMap<TObs>();

        foreach (var (key, nobs) in _countMap)
        {
            propMap.Fit(key, nobs);
        }

        return propMap;
    }

    public override void Merge(ProportionMap<TObs> other) => _countMap.Merge(other._countMap);
    
    public bool ContainsKey(TObs key) => _countMap.ContainsKey(key);

    public bool TryGetValue(TObs key, out double value)
    {
        if (_countMap.TryGetValue(key, out var count))
        {
            value = (double) count / Nobs;
            return true;
        }

        value = default;
        return false;
    }
    
    public IEnumerator<KeyValuePair<TObs, double>> GetEnumerator()
    {
        foreach (var (key, prop) in Keys.Zip(Values))
        {
            yield return new KeyValuePair<TObs, double>(key, prop);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override string ToString() => $"{typeof(ProportionMap<TObs>)} Nobs={Nobs} | {NumUnique} unique values";

    public IDictionary<TObs, double> ToDictionary() => this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public CountMap<TObs> AsCountMap() => _countMap;

    public CountMap<TObs> ToCountMap() => _countMap.Clone();
}