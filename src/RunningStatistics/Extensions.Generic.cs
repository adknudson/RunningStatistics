using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RunningStatistics;

public static partial class Extensions
{
    
#if NET7_0_OR_GREATER
    
    /// <summary>
    /// Generic sum for any type that supports addition with an additive identity
    /// </summary>
    internal static T MyGenericSum<T>(this IEnumerable<T> source) 
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        return source.Aggregate(T.AdditiveIdentity, (current, value) => current + value);
    }

    /// <summary>
    /// Generic map-sum for any type that supports addition with an additive identity
    /// </summary>
    internal static TReturn MyGenericSum<TSource, TReturn>(this IEnumerable<TSource> source, Func<TSource, TReturn> selector) 
        where TReturn : IAdditionOperators<TReturn, TReturn, TReturn>, IAdditiveIdentity<TReturn, TReturn>
    {
        return source.Select(selector).MyGenericSum();
    }

    /// <summary>
    /// Generic mean for any type that supports addition and can be divided by an <see cref="int"/>
    /// </summary>
    internal static T MyGenericAverage<T>(this IEnumerable<T> source)
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, IDivisionOperators<T, int, T>
    {
        var ls = source.ToList();
        return ls.MyGenericSum() / ls.Count;
    }

    /// <summary>
    /// Generic map-average for any type that supports addition and can be divided by an <see cref="int"/>
    /// </summary>
    internal static TReturn MyGenericAverage<TSource, TReturn>(this IEnumerable<TSource> source, Func<TSource, TReturn> selector)
        where TReturn : IAdditionOperators<TReturn, TReturn, TReturn>, IAdditiveIdentity<TReturn, TReturn>, IDivisionOperators<TReturn, int, TReturn>
    {
        return source.Select(selector).MyGenericAverage();
    }
    
#endif
    
}