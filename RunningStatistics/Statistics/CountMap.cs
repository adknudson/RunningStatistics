using System.Collections;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

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
    /// Creates a CountMap object with the default observation comparer.
    /// </summary>
    public CountMap()
    {
        // leave Comparer as null to indicate default comparer is to be used,
        // or to indicate that no ordering is possible
        Comparer = null;
    }

    /// <summary>
    /// Creates a CountMap object with the given observation comparer.
    /// </summary>
    public CountMap(IComparer<TObs>? comparer)
    {
        Comparer = comparer;
    }
    
    
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

    /// <summary>
    /// Gets the value comparer used for ordering observations.
    /// </summary>
    public IComparer<TObs>? Comparer { get; }

    
    protected override long GetNobs() => _nobs;
    
    public override void Fit(TObs value, long count)
    {
        Require.NonNegative(count);
        if (count == 0) return; 
        UncheckedFit(value, count);
    }
    
#if NET5_0_OR_GREATER
    private void UncheckedFit(TObs value, long count)
    {
        _nobs += count;
        ref var valueCount = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, value, out _);
        // default value for 'long' is zero, so we are safe to just use += here
        valueCount += count;
    }
#else
    private void UncheckedFit(TObs value, long count)
    {
        _nobs += count;

        if (!_dict.TryAdd(value, count))
        {
            _dict[value] += count;
        }
    } 
#endif
    
    public override void Merge(CountMap<TObs> countMap) => Fit(countMap);
    
    public override void Reset()
    {
        _dict.Clear();
        _nobs = 0;
    }
    
    public override CountMap<TObs> CloneEmpty() => new();
    
    public bool ContainsKey(TObs key) => _dict.ContainsKey(key);
    
    public bool TryGetValue(TObs key, out long value) => _dict.TryGetValue(key, out value);

    /// <summary>
    /// Finds the minimum key in the count map.
    /// </summary>
    /// <param name="comparer">The comparer to use for finding the minimum key.
    /// If null, the default comparer is used.</param>
    /// <returns>The minimum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no minimum key exists.
    /// </exception>
    public TObs MinKey(IComparer<TObs>? comparer)
    {
        comparer ??= Comparer<TObs>.Default;

        using var enumerator = _dict.Keys.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new KeyNotFoundException("The minimum key does not exist.");
        }

        var min = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (comparer.Compare(enumerator.Current, min) < 0)
            {
                min = enumerator.Current;
            }
        }

        return min!;
    }
    
    /// <summary>
    /// Finds the minimum key in the count map.
    /// </summary>
    /// <returns>The minimum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no minimum key exists.
    /// </exception>
    public TObs MinKey() => MinKey(Comparer);
    
    /// <summary>
    /// Finds the maximum key in the count map.
    /// </summary>
    /// <param name="comparer">The comparer to use for finding the maximum key.
    /// If null, the default comparer is used.</param>
    /// <returns>The maximum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no maximum key exists.
    /// </exception>
    public TObs MaxKey(IComparer<TObs>? comparer)
    {
        comparer ??= Comparer<TObs>.Default;
        
        using var enumerator = _dict.Keys.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new KeyNotFoundException("The maximum key does not exist.");
        }
        
        var max = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (comparer.Compare(enumerator.Current, max) > 0)
            {
                max = enumerator.Current;
            }
        }
        
        return max!;
    }
    
    /// <summary>
    /// Finds the maximum key in the count map.
    /// </summary>
    /// <returns>The maximum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no maximum key exists.
    /// </exception>
    public TObs MaxKey() => MaxKey(Comparer);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => base.ToString() + $" | {Count:N0} unique observations";
}
