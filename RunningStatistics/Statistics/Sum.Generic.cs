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
    /// A precompiled routine that multiplies an observation by a whole-number count, when the
    /// observation type supports that multiplication. If the type implements multiplication by
    /// long, double, decimal or float, the class compiles this delegate once per type
    /// instantiation. It builds the delegate from an expression tree. The weighted fit uses it
    /// to fold a count into the running sum in one step. If the type supports none of these
    /// multiplications, the delegate stays null. The weighted fit then falls back to a doubling
    /// loop whose work grows with the count's bits.
    /// <para>
    /// Note: when the multiplier type is float, the implementation narrows the whole-number count
    /// to float before the multiplication, which loses precision for counts above roughly 16.7
    /// million.
    /// </para>
    /// </summary>
    internal static readonly Func<TObs, long, TObs>? MultiplyDelegate = TryCreateMultiplyDelegate();

    private static Func<TObs, long, TObs>? TryCreateMultiplyDelegate()
    {
        // Try the multiplier types in priority order. Multiplying by double and by
        // decimal widens the whole-number count implicitly. Multiplying by float
        // narrows the count and loses precision for counts above roughly 16.7
        // million. The code still accepts float to give sums of float values a
        // fast path.
        return TryCreate<long>()
            ?? TryCreate<double>()
            ?? TryCreate<decimal>()
            ?? TryCreate<float>();

        Func<TObs, long, TObs>? TryCreate<TMultiplier>()
        {
            // Do not use MakeGenericType here: the multiply-operator interface carries a
            // self-referential constraint that the runtime enforces even at reflection
            // time. Scanning the interfaces the type already implements avoids that
            // enforcement, because those interfaces already passed validation.
            var hasMultiply = Array.Exists(typeof(TObs).GetInterfaces(), i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IMultiplyOperators<,,>) &&
                i.GenericTypeArguments[0] == typeof(TObs) &&
                i.GenericTypeArguments[1] == typeof(TMultiplier) &&
                i.GenericTypeArguments[2] == typeof(TObs));

            if (!hasMultiply) return null;

            var valueParam = Expression.Parameter(typeof(TObs), "value");
            var countParam = Expression.Parameter(typeof(long), "count");

            // Insert an implicit widening conversion when the operator does not take long directly.
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
        checked { _nobs++; }
        Value += value;
    }

    /// <summary>
    /// Adds the given number of observations of the value at once. The method raises the
    /// observation count in one checked step.
    /// <para>
    /// When the multiply delegate above is available, it multiplies the value by the count.
    /// The fit adds that product to the running sum in one step. Otherwise the fit builds the
    /// contribution bit by bit. It doubles the value once per bit position. It gathers the
    /// blocks that belong to the set bits into an accumulator. The accumulator joins the running
    /// sum once.
    /// </para>
    /// <para>
    /// Caveat for floating-point-like observation types: the doubling path groups additions
    /// differently than adding the value one observation at a time does. The result may differ
    /// in the low bits from repeated single fits. Do not test bit-exact equality against
    /// repeated addition for such types.
    /// </para>
    /// </summary>
    public override void Fit(TObs value, long count)
    {
        Require.NonNegative(count);
        if (count == 0) return;

        if (MultiplyDelegate is not null)
        {
            // Fast path: the observation type multiplies directly by a whole-number count.
            checked { _nobs += count; }
            Value += MultiplyDelegate(value, count);
        }
        else
        {
            // Fallback when no multiplication by a count is available: build the
            // contribution by reading the count's bits. Each bit stands for a
            // block of the value doubled once per position. The loop gathers
            // blocks for the set bits into an accumulator. The loop skips the
            // last doubling, because no higher bits remain. Doubling there
            // could overflow even when the exact product would fit.
            var contribution = TObs.AdditiveIdentity;
            var block = value;
            for (var n = count; n > 0; n >>= 1)
            {
                if ((n & 1) != 0) contribution += block;
                if (n > 1) block += block;
            }

            checked { _nobs += count; }
            Value += contribution;
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
        checked { _nobs += sum.Nobs; }
        Value += sum.Value;
    }

    public static explicit operator TObs(Sum<TObs> sum) => sum.Value;

    public override string ToString() => base.ToString() + $" | Σ={Value}";
}
