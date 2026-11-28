using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Dictionary<TObs, long> _dict = [];
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
    /// Gets the minimum key in the count map, ordered by the comparer the map was built with.
    /// </summary>
    /// <remarks>
    /// An empty map has no minimum key, and a generic key type has no NaN to stand in, so the
    /// property throws in that case, the counterpart of the NaN readouts on numeric statistics.
    /// Use <see cref="TryGetMinKey(out TObs)"/> when a non-throwing read is needed, or the
    /// two-argument overload that takes a comparer when the keys need another ordering;
    /// LINQ's comparer form over <see cref="Keys"/> does the same order scan in one call.
    /// </remarks>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no minimum key exists.
    /// </exception>
    public TObs MinKey => TryGetMinKey(out var min)
        ? min
        : throw new KeyNotFoundException("The minimum key does not exist.");

    /// <summary>
    /// Gets the maximum key in the count map, ordered by the comparer the map was built with.
    /// </summary>
    /// <remarks>
    /// An empty map has no maximum key, and a generic key type has no NaN to stand in, so the
    /// property throws in that case, the counterpart of the NaN readouts on numeric statistics.
    /// Use <see cref="TryGetMaxKey(out TObs)"/> when a non-throwing read is needed, or the
    /// two-argument overload that takes a comparer when the keys need another ordering;
    /// LINQ's comparer form over <see cref="Keys"/> does the same order scan in one call.
    /// </remarks>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty and no maximum key exists.
    /// </exception>
    public TObs MaxKey => TryGetMaxKey(out var max)
        ? max
        : throw new KeyNotFoundException("The maximum key does not exist.");

    /// <summary>
    /// Gets an enumerable collection of the values in the count map.
    /// </summary>
    public IEnumerable<long> Values => _dict.Values;

    /// <summary>
    /// Gets the value comparer used for ordering observations.
    /// </summary>
    public IComparer<TObs>? Comparer { get; }
    
    /// <summary>
    /// Gets the observation with the highest count.
    /// </summary>
    /// <remarks>
    /// The map's own iteration order decides which observation is reported when several share
    /// the highest count, exactly as it did while this read was the
    /// <c>CountMapExtensions.Mode</c> extension method.
    /// </remarks>
    /// <exception cref="KeyNotFoundException">Thrown when the count map has no observations, or when the mode cannot be found.</exception>
    public TObs Mode
    {
        get
        {
            if (Nobs == 0)
            {
                throw new KeyNotFoundException("Nobs == 0. The mode does not exist.");
            }

            TObs? mode = default;
            var maxCount = 0L;

            foreach (var kvp in this)
            {
                if (kvp.Value <= maxCount) continue;
                mode = kvp.Key;
                maxCount = kvp.Value;
            }

            return mode ?? throw new KeyNotFoundException("No mode was found.");
        }
    }
    
    /// <summary>
    /// Gets the median of a CountMap, that is the result of <c>Quantile(0.5)</c>.
    /// </summary>
    /// <remarks>
    /// This method uses the comparer associated with the CountMap. If the CountMap
    /// has no comparer, the method uses the default comparer instead. A caller
    /// that needs to order the keys with another comparer reads
    /// <c>Quantile(0.5, comparer)</c> instead: a property cannot sit beside a
    /// same-named method, so the comparer-taking median overload has no member
    /// of its own.
    /// </remarks>
    /// <returns>The median of the count map.</returns>
    public TObs Median => Quantile(0.5);

    
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
    
    /// <summary>
    /// Gets the p-th quantile of a CountMap, with the keys ordered by the given comparer.
    /// If <c>p</c> is less than or equal to 0, the method returns the minimum key.
    /// If <c>p</c> is greater than or equal to 1, the method returns the maximum key.
    /// Otherwise the keys are scanned in order and the running cumulative frequency
    /// is compared against <c>p</c>; the method returns the first key at which the
    /// running frequency reaches or passes <c>p</c>.
    /// </summary>
    /// <remarks>
    /// Tie rule: the scan accepts the first key whose cumulative frequency is
    /// greater than or equal to <c>p</c>, the inverse empirical CDF (generalized
    /// inverse, R type 1) convention. When <c>p</c> equals a cumulative frequency
    /// exactly, the method returns the key at which that frequency is reached —
    /// the lower of the two neighboring observations in the sorted sample — not
    /// the next key up. The rule is the same for every count map; it does not
    /// depend on the parity of <c>Nobs</c> or on the number of unique keys.
    /// This is the median convention the CountMap median follows: with an even
    /// number of observations, the median is the first observation at which the
    /// cumulative proportion reaches one half.
    /// </remarks>
    /// <param name="p">The quantile to compute. Pass a value between 0 and 1.</param>
    /// <param name="comparer">The comparer to use for ordering the keys.
    /// If null, the method uses the default comparer.</param>
    /// <returns>The p-th quantile of the count map.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the count map is empty or if the quantile cannot be found.
    /// </exception>
    public TObs Quantile(double p, IComparer<TObs>? comparer)
    {
        if (Nobs == 0)
        {
            throw new KeyNotFoundException("The count map is empty. The quantile does not exist.");
        }

        // The end keys come through the non-throwing helpers rather than through the MinKey
        // and MaxKey readouts, so this scan does not depend on how those readouts report a
        // missing extreme. The empty map is rejected above, so the helpers cannot fail here;
        // the fall-through still throws the KeyNotFoundException the old extension body did.
        if (p <= 0)
        {
            if (TryGetMinKey(comparer, out var minKey)) return minKey;
            throw new KeyNotFoundException("The minimum key does not exist.");
        }
        if (p >= 1)
        {
            if (TryGetMaxKey(comparer, out var maxKey)) return maxKey;
            throw new KeyNotFoundException("The maximum key does not exist.");
        }

        var cdf = 0.0;
        var nobs = (double)Nobs;

        foreach (var kvp in this.OrderBy(kvp => kvp.Key, comparer))
        {
            cdf += kvp.Value / nobs;
            if (cdf >= p) return kvp.Key;
        }

        // This should be unreachable...
        throw new KeyNotFoundException($"Not able to find the {p} quantile of the count map.");
    }

    /// <summary>
    /// Gets the p-th quantile of a CountMap.
    /// If <c>p</c> is less than or equal to 0, the method returns the minimum key.
    /// If <c>p</c> is greater than or equal to 1, the method returns the maximum key.
    /// </summary>
    /// <param name="p">The quantile to compute. Pass a value between 0 and 1.</param>
    /// <returns>The p-th quantile of the count map.</returns>
    /// <remarks>
    /// This method uses the comparer associated with the CountMap. If the CountMap
    /// has no comparer, the method uses the default comparer instead.
    /// </remarks>
    public TObs Quantile(double p) => Quantile(p, Comparer);

    public IEnumerator<KeyValuePair<TObs, long>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => base.ToString() + $" | {Count:N0} unique observations";
}
