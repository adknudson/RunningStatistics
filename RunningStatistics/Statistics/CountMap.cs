using System.Collections;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// Represents a count map that tracks the number of occurrences of each unique observation.
/// Accessing a key that has not been fitted will return zero, but the key will not be added to the
/// dictionary. Fitting a key with a count of zero will also not add the key to the dictionary.
/// </summary>
/// <typeparam name="TObs">The observation type.</typeparam>
public sealed class CountMap<TObs> : RunningStatisticBase<TObs, CountMap<TObs>>, IReadOnlyDictionary<TObs, long> 
    where TObs : notnull
{
    private readonly Dictionary<TObs, long> _dict = new();
    private long _nobs;
    
    
    /// <summary>
    /// Gets the count of the specified key. If the key has not been fitted, zero is returned.
    /// </summary>
    /// <param name="key">The key whose count to get.</param>
    /// <returns>The count of the specified key.</returns>
    public long this[TObs key] => _dict.GetValueOrDefault(key, 0);

    /// <summary>
    /// The number of unique observations that have been fitted. Observations with a count of zero
    /// are not included.
    /// </summary>
    public int Count => _dict.Count;

    /// <summary>
    /// Gets an enumerable collection of the keys in the count map.
    /// </summary>
    public IEnumerable<TObs> Keys => _dict.Keys;

    /// <summary>
    /// Gets an enumerable collection of the values in the count map.
    /// </summary>
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
