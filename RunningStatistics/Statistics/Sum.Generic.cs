#if NET7_0_OR_GREATER

using System;
using System.Linq.Expressions;
using System.Numerics;

namespace RunningStatistics;

/// <summary>
/// Represents a running sum of observations.
/// </summary>
/// <typeparam name="TObs">The type of the observations. Must implement
/// <see cref="IAdditionOperators{TObs,TObs,TObs}"/> and <see cref="IAdditiveIdentity{TObs,TObs}"/>.</typeparam>
public sealed class Sum<TObs> : RunningStatisticBase<TObs, Sum<TObs>> 
    where TObs : IAdditionOperators<TObs, TObs, TObs>, IAdditiveIdentity<TObs, TObs>
{
    /// <summary>
    /// If <typeparamref name="TObs"/> implements <see cref="IMultiplyOperators{TObs,TMultiplier,TObs}"/>
    /// for any <c>TMultiplier</c> in the set <c>{ long, double, decimal, float }</c>, this delegate
    /// is compiled once (per type instantiation) via an expression tree and used by
    /// <see cref="Fit(TObs,long)"/> to compute <c>value * count</c> in O(1). Otherwise it is
    /// <c>null</c> and the method falls back to an O(count) addition loop.
    /// <para>
    /// Note: when <c>TMultiplier</c> is <c>float</c>, the <c>long</c> count is narrowed to
    /// <c>float</c> before the multiply, which loses precision for counts greater than ~16.7M
    /// (2²⁴).
    /// </para>
    /// </summary>
    internal static readonly Func<TObs, long, TObs>? MultiplyDelegate = TryCreateMultiplyDelegate();

    private static Func<TObs, long, TObs>? TryCreateMultiplyDelegate()
    {
        // Try multiplier types in priority order.
        // long → double and long → decimal are implicit widening conversions.
        // long → float is a narrowing conversion (precision loss for counts > ~16.7M), but is
        // accepted here to provide a fast path for Sum<float>.
        return TryCreate<long>()
            ?? TryCreate<double>()
            ?? TryCreate<decimal>()
            ?? TryCreate<float>();

        Func<TObs, long, TObs>? TryCreate<TMultiplier>()
        {
            // We cannot use MakeGenericType here because IMultiplyOperators<TSelf,…> has a
            // self-referential constraint on TSelf that the runtime enforces even at reflection
            // time. Scanning GetInterfaces() avoids that — it only returns already-validated types.
            var hasMultiply = Array.Exists(typeof(TObs).GetInterfaces(), i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IMultiplyOperators<,,>) &&
                i.GenericTypeArguments[0] == typeof(TObs) &&
                i.GenericTypeArguments[1] == typeof(TMultiplier) &&
                i.GenericTypeArguments[2] == typeof(TObs));

            if (!hasMultiply) return null;

            var valueParam = Expression.Parameter(typeof(TObs), "value");
            var countParam = Expression.Parameter(typeof(long), "count");

            // Insert an implicit widening conversion when the operator doesn't take long directly.
            Expression multiplierExpr = typeof(TMultiplier) == typeof(long)
                ? countParam
                : Expression.Convert(countParam, typeof(TMultiplier));

            var body = Expression.Multiply(valueParam, multiplierExpr);
            return Expression.Lambda<Func<TObs, long, TObs>>(body, valueParam, countParam).Compile();
        }
    }

    private long _nobs;
    
    
    /// <summary>
    /// The current sum of the observations.
    /// </summary>
    public TObs Value { get; private set; } = TObs.AdditiveIdentity;


    protected override long GetNobs() => _nobs;

    public override void Fit(TObs value)
    {
        _nobs++;
        Value += value;
    }

    public override void Fit(TObs value, long count)
    {
        Require.NonNegative(count);
        if (count == 0) return;

        if (MultiplyDelegate is not null)
        {
            // O(1): TObs supports IMultiplyOperators<TObs, long, TObs>
            _nobs += count;
            Value += MultiplyDelegate(value, count);
        }
        else
        {
            // O(count) fallback: no multiply operator available for this type
            for (var i = 0; i < count; i++)
            {
                Fit(value);
            }
        }
    }

    public override void Reset()
    {
        _nobs = 0;
        Value = TObs.AdditiveIdentity;
    }

    public override Sum<TObs> CloneEmpty() => new();
    
    public override void Merge(Sum<TObs> sum)
    {
        _nobs += sum.Nobs;
        Value += sum.Value;
    }

    public static explicit operator TObs(Sum<TObs> sum) => sum.Value;

    public override string ToString() => base.ToString() + $" | Σ={Value}";
}

#endif
