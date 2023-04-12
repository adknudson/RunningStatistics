using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class ProportionMap<TObs> : IReadOnlyDictionary<TObs, double>, IRunningStatistic<TObs, ProportionMap<TObs>> where TObs: notnull
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



    public long Nobs => _countMap.Nobs;

    public double this[TObs key] => TryGetValue(key, out var value) ? value : default;

    int IReadOnlyCollection<KeyValuePair<TObs, double>>.Count => _countMap.AsDictionary().Count;

    public int NumUnique => _countMap.NumUnique;
    
    public IEnumerable<TObs> Keys => _countMap.Keys;

    public IEnumerable<double> Values => _countMap.Values.Select(count => (double) count / Nobs);
    
    
    
    public void Fit(IEnumerable<TObs> values) => _countMap.Fit(values);

    public void Fit(TObs value) => _countMap.Fit(value);

    public void Fit(TObs obs, long count) => _countMap.Fit(obs, count);

    public void Merge(ProportionMap<TObs> other) => _countMap.Merge(other._countMap);

    public void Reset() => _countMap.Reset();

    public ProportionMap<TObs> CloneEmpty()
    {
        return new ProportionMap<TObs>();
    }

    public ProportionMap<TObs> Clone()
    {
        var propMap = new ProportionMap<TObs>();

        foreach (var (key, nobs) in _countMap)
        {
            propMap.Fit(key, nobs);
        }

        return propMap;
    }

    public static ProportionMap<TObs> Merge(ProportionMap<TObs> first, ProportionMap<TObs> second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }
    
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
}