#if NETSTANDARD2_0

using System.Collections.Generic;

namespace RunningStatistics;

internal static class NetStandard2Extensions
{
    internal static TValue GetValueOrDefault<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
    
    internal static bool TryAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key)) return false;
        dictionary.Add(key, value);
        return true;
    }
}

#endif
