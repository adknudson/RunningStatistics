using System.Collections;
using System.Collections.Generic;

namespace RunningStatistics;

public sealed class CountMap<TObs> : RunningStatisticBase<TObs, CountMap<TObs>>, IReadOnlyDictionary<TObs, long> 
    where TObs : notnull
{
    private readonly Dictionary<TObs, long> _dict = new();
    private long _nobs;
    
    
    public long this[TObs key] => _dict.GetValueOrDefault(key, 0);
    
    /// <summary>
    /// The number of unique observations that have been fitted. Observations with a count of zero are not included.
    /// </summary>
    public int Count => _dict.Count;

    public IEnumerable<TObs> Keys => _dict.Keys;

    public IEnumerable<long> Values => _dict.Values;


    protected override long GetNobs() => _nobs;
    
    public override void Fit(TObs value, long count)
    {
        Require.NonNegative(count);
        if (count == 0) return; 
        UncheckedFit(value, count);
    }
    
    private void UncheckedFit(TObs value, long count)
    {
        _nobs += count;

        if (!_dict.TryAdd(value, count))
        {
            _dict[value] += count;
        }
    }

    public override void Merge(CountMap<TObs> countMap) => Fit(countMap);

    public override void Reset()
    {
        _dict.Clear();
        _nobs = 0;
    }

    public override CountMap<TObs> CloneEmpty() => new();
    
    public bool ContainsKey(TObs key) => _dict.ContainsKey(key);

    public bool TryGetValue(TObs key, out long value) => _dict.TryGetValue(key, out value);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    protected override string GetStatsString() => $"{Count} unique observations";
}
