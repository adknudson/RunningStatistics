using System.Collections;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace RunningStatistics;

/// <summary>
/// Represents a count map that tracks the number of occurrences of each unique observation.
/// The map returns zero for a key that it never fitted. That access does not add the key to the
/// dictionary. A fit with a count of zero also does not add the key.
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
    /// Gets the count of the specified key. If the map never fitted the key, it returns zero.
    /// </summary>
    /// <param name="key">The key whose count to get.</param>
    /// <returns>The count of the specified key.</returns>
    // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
    public long this[TObs key] => _dict.TryGetValue(key, out var value) ? value : 0;

    /// <summary>
    /// The number of unique observations that the map fitted. Observations with a count of zero
    /// do not count toward this number.
    /// </summary>
    public int Count => _dict.Count;

    /// <summary>
    /// The number of unique observations that the map fitted. Same as
    /// <see cref="Count"/>, but as a <c>long</c> instead.
    /// </summary>
    public long UniqueObs => _dict.Count;

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
    
    private void UncheckedFit(TObs value, long count)
    {
        checked { _nobs += count; }
        
        // The checked total already bounds the per-key counts. The code still
        // checks the per-key accumulation explicitly, so it reports overflow
        // rather than wrapping quietly.
#if NET5_0_OR_GREATER
        ref var valueCount = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, value, out _);
        checked { valueCount += count; }
#else
        if (_dict.TryGetValue(value, out var oldCount))
        {
            checked { _dict[value] = oldCount + count; }
        }
        else
        {
            _dict[value] = count;
        }
#endif
    } 
    
    public override void Merge(CountMap<TObs> countMap) => Fit(countMap);
    
    public override void Reset()
    {
        _dict.Clear();
        _nobs = 0;
    }
    
    public override CountMap<TObs> CloneEmpty() => new(Comparer);
    
    public bool ContainsKey(TObs key) => _dict.ContainsKey(key);
    
    public bool TryGetValue(TObs key, out long value) => _dict.TryGetValue(key, out value);

    /// <summary>
    /// Finds the minimum key in the count map.
    /// </summary>
    /// <remarks>
    /// An empty map has no minimum key, and a generic key type has no NaN to stand in, so the
    /// method throws in that case, the counterpart of the NaN readouts on numeric statistics.
    /// Use <see cref="TryGetMinKey(out TObs)"/> when a non-throwing read is needed.
    /// </remarks>
    /// <param name="comparer">The comparer to use for finding the minimum key.
    /// If null, the method uses the default comparer.</param>
    /// <returns>The minimum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no minimum key exists.
    /// </exception>
    public TObs MinKey(IComparer<TObs>? comparer)
    {
        if (TryGetMinKey(comparer, out var min))
        {
            return min;
        }
        throw new KeyNotFoundException("The minimum key does not exist.");
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
    /// Stores the minimum key in the count map in the given reference and reports whether one
    /// exists. The method does not throw for an empty map.
    /// </summary>
    /// <param name="comparer">The comparer to use for finding the minimum key.
    /// If null, the method uses the default comparer.</param>
    /// <param name="key">Receives the minimum key, or the default of the key type when the map is empty.</param>
    /// <returns>True when the map holds at least one key, false when it is empty.</returns>
    public bool TryGetMinKey(IComparer<TObs>? comparer, out TObs key)
    {
        comparer ??= Comparer<TObs>.Default;

        using var enumerator = _dict.Keys.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            // Every return must assign the out parameter; the false return tells callers
            // to ignore this default key.
            key = default!;
            return false;
        }

        var min = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (comparer.Compare(enumerator.Current, min) < 0)
            {
                min = enumerator.Current;
            }
        }

        key = min!;
        return true;
    }
    
    /// <summary>
    /// Stores the minimum key in the count map in the given reference and reports whether one
    /// exists. The method does not throw for an empty map.
    /// </summary>
    /// <param name="key">Receives the minimum key, or the default of the key type when the map is empty.</param>
    /// <returns>True when the map holds at least one key, false when it is empty.</returns>
    public bool TryGetMinKey(out TObs key) => TryGetMinKey(Comparer, out key);
    
    /// <summary>
    /// Finds the maximum key in the count map.
    /// </summary>
    /// <remarks>
    /// An empty map has no maximum key, and a generic key type has no NaN to stand in, so the
    /// method throws in that case, the counterpart of the NaN readouts on numeric statistics.
    /// Use <see cref="TryGetMaxKey(out TObs)"/> when a non-throwing read is needed.
    /// </remarks>
    /// <param name="comparer">The comparer to use for finding the maximum key.
    /// If null, the method uses the default comparer.</param>
    /// <returns>The maximum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no maximum key exists.
    /// </exception>
    public TObs MaxKey(IComparer<TObs>? comparer)
    {
        if (TryGetMaxKey(comparer, out var max))
        {
            return max;
        }
        throw new KeyNotFoundException("The maximum key does not exist.");
    }
    
    /// <summary>
    /// Finds the maximum key in the count map.
    /// </summary>
    /// <returns>The maximum key in the count map, if any exist.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no maximum key exists.
    /// </exception>
    public TObs MaxKey() => MaxKey(Comparer);
    
    /// <summary>
    /// Stores the maximum key in the count map in the given reference and reports whether one
    /// exists. The method does not throw for an empty map.
    /// </summary>
    /// <param name="comparer">The comparer to use for finding the maximum key.
    /// If null, the method uses the default comparer.</param>
    /// <param name="key">Receives the maximum key, or the default of the key type when the map is empty.</param>
    /// <returns>True when the map holds at least one key, false when it is empty.</returns>
    public bool TryGetMaxKey(IComparer<TObs>? comparer, out TObs key)
    {
        comparer ??= Comparer<TObs>.Default;
        
        using var enumerator = _dict.Keys.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            key = default!;
            return false;
        }
        
        var max = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (comparer.Compare(enumerator.Current, max) > 0)
            {
                max = enumerator.Current;
            }
        }
        
        key = max!;
        return true;
    }
    
    /// <summary>
    /// Stores the maximum key in the count map in the given reference and reports whether one
    /// exists. The method does not throw for an empty map.
    /// </summary>
    /// <param name="key">Receives the maximum key, or the default of the key type when the map is empty.</param>
    /// <returns>True when the map holds at least one key, false when it is empty.</returns>
    public bool TryGetMaxKey(out TObs key) => TryGetMaxKey(Comparer, out key);
    
    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => base.ToString() + $" | {Count:N0} unique observations";
}
