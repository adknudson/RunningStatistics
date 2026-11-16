using System;
using System.Collections.Generic;

namespace RunningStatistics.Tests;

/// <summary>
/// Deterministic datasets and reference computations shared by correctness gates (see the workflow
/// in <c>AGENTS.md</c>). Datasets are generated with a fixed-seed PRNG so a gate failure is
/// reproducible, and cover the pathological shapes an incremental algorithm has to survive:
/// badly offset streams (κ = |mean|/spread from 1 to 1e12), constant data, degenerate counts.
///
/// The reference values here are two-pass (mean first, then central sums) with compensated
/// summation, i.e. correct to ~machine epsilon on every dataset these helpers generate. A candidate
/// running statistic is judged against those, not against the production implementation.
/// </summary>
internal static class PropertyHelpers
{
    /// Machine epsilon for <c>double</c>: the smallest tolerance any accurate algorithm can meet.
    public static double Epsilon { get; } = 2.220446049250313e-16;

    // ---- Deterministic data generation ----

    private static long _state = 0x6C078963554F5A71L;

    /// Seeds the xorshift-style stream used by the dataset builders below.
    public static void Seed(long seed)
    {
        _state = seed == 0 ? 0x6C078963554F5A71L : seed;
    }

    /// Next value in [0, 1).
    public static double NextUnit()
    {
        _state = _state * 6364136223846793005L + 1440329614744452433L;
        var u = (ulong)_state >> 11;
        return (double)u / 9007199254740992.0;
    }

    /// Next standard-normal value (Box–Muller).
    public static double NextNormal()
    {
        var u = NextUnit();
        var v = NextUnit();
        if (u < 1e-300) u = 1e-300;
        return Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
    }

    /// <paramref name="n"/> uniforms in [0, 1).
    public static List<double> UniformUnit(int n, long seed)
    {
        Seed(seed);
        var xs = new List<double>();
        for (var i = 0; i < n; i++) xs.Add(NextUnit());
        return xs;
    }

    /// <summary>
    /// <paramref name="n"/> uniforms spread over [<c>offset</c>, <c>offset + spread</c>): κ ≈
    /// <c>|offset|/spread</c>, the condition number that naive raw-power-sum algorithms blow up on.
    /// </summary>
    public static List<double> OffsetSpread(int n, double offset, double spread, long seed)
    {
        Seed(seed);
        var xs = new List<double>();
        for (var i = 0; i < n; i++) xs.Add(offset + spread * NextUnit());
        return xs;
    }

    /// <paramref name="n"/> normals with the given mean and standard deviation.
    public static List<double> NormalSamples(int n, double mean, double stdDev, long seed)
    {
        Seed(seed);
        var xs = new List<double>();
        for (var i = 0; i < n; i++) xs.Add(mean + stdDev * NextNormal());
        return xs;
    }

    /// <summary>A stream with mean ≈ <paramref name="kappa"/> and spread 1 (κ ≈ kappa).</summary>
    public static List<double> KappaStream(int n, double kappa, long seed) => OffsetSpread(n, kappa, 1.0, seed);

    /// <summary>
    /// Streams whose κ sweeps 10^0 … 10^<paramref name="decades"/>, the range over which an
    /// accurate algorithm must stay accurate and a raw-power-sum algorithm does not.
    /// </summary>
    public static List<List<double>> KappaSweep(int n, int decades, long seed)
    {
        var sets = new List<List<double>>();
        for (var d = 0; d <= decades; d++)
        {
            sets.Add(KappaStream(n, Math.Pow(10.0, d), seed + d));
        }

        return sets;
    }

    /// <summary><paramref name="count"/> copies of <paramref name="value"/> (constant / zero-spread data).</summary>
    public static List<double> Constant(int n, double value)
    {
        var xs = new List<double>();
        for (var i = 0; i < n; i++) xs.Add(value);
        return xs;
    }

    // ---- Reference computations ----

    /// <summary>Two-pass, compensated reference for the four-moment contract.</summary>
    public static MomentsReference ReferenceMoments(List<double> values)
    {
        var reference = new MomentsReference();
        var n = values.Count;
        if (n == 0) return reference;

        var mean = CompensatedMean(values);
        reference.Mean = mean;

        var c2 = 0.0;
        var c3 = 0.0;
        var c4 = 0.0;
        for (var i = 0; i < n; i++)
        {
            var d = values[i] - mean;
            var d2 = d * d;
            c2 += d2;
            c3 += d2 * d;
            c4 += d2 * d2;
        }

        var m2 = c2 / n;
        var m3 = c3 / n;
        var m4 = c4 / n;
        reference.PopulationVariance = m2;
        if (n < 2) return reference;

        reference.Variance = m2 * ((double)n / (n - 1));
        reference.StandardDeviation = Math.Sqrt(reference.Variance);
        if (m2 <= 0) return reference;

        reference.Skewness = m3 / (m2 * Math.Sqrt(m2));
        reference.Kurtosis = m4 / (m2 * m2);
        reference.ExcessKurtosis = reference.Kurtosis - 3;
        return reference;
    }

    /// Neumaier-compensated sum: accurate to ~one epsilon regardless of order/magnitude.
    private static double CompensatedSum(List<double> xs)
    {
        var sum = 0.0;
        var compensation = 0.0;
        for (var i = 0; i < xs.Count; i++)
        {
            var x = xs[i];
            var t = sum + x;
            if (Math.Abs(sum) >= Math.Abs(x)) compensation += (sum - t) + x;
            else compensation += (x - t) + sum;
            sum = t;
        }

        return sum + compensation;
    }

    private static double CompensatedMean(List<double> xs) => CompensatedSum(xs) / xs.Count;

    // ---- Tolerant comparison helpers ----

    /// <summary>
    /// Relative-tolerance comparison with NaN agreement (both NaN passes). <paramref name="relTol"/>
    /// is usually derived from κ, e.g. <c>1e-9 + 100 * kappa * PropertyHelpers.Epsilon</c> for a
    /// stable algorithm whose worst error is a few κ·ε.
    /// </summary>
    public static bool Within(double expected, double actual, double relTol)
    {
        if (double.IsNaN(expected) && double.IsNaN(actual)) return true;
        if (expected == actual) return true;
        if (double.IsNaN(expected) || double.IsNaN(actual)) return false;

        var scale = Math.Max(Math.Abs(expected), Math.Abs(actual));
        if (scale == 0) return true;
        return Math.Abs(expected - actual) <= relTol * scale;
    }

    /// <summary>
    /// Absolute-tolerance comparison with NaN agreement. Use this for the standardized moments
    /// (<c>Skewness</c>, <c>Kurtosis</c>): their true values are small ratios (g1, g2) while the
    /// error a stable algorithm makes is ~κ·ε in <em>data-scale units</em> (σ³, σ⁴), so a relative
    /// tolerance on g1 would demand accuracy better than the representation of the mean allows.
    /// </summary>
    public static bool WithinAbsolute(double expected, double actual, double absTol)
    {
        if (double.IsNaN(expected) && double.IsNaN(actual)) return true;
        if (expected == actual) return true;
        if (double.IsNaN(expected) || double.IsNaN(actual)) return false;
        return Math.Abs(expected - actual) <= absTol;
    }

    /// One-line description of a comparison, for gate failure messages.
    public static string Describe(string label, double expected, double actual)
    {
        var error = expected == actual
            ? 0.0
            : (Math.Abs(expected) > 0 ? Math.Abs(actual - expected) / Math.Abs(expected) : double.PositiveInfinity);
        return $"{label}: expected {expected}, actual {actual}, relative error {error:E2}";
    }
}

/// <summary>
/// Reference values for the four-moment readout contract, computed two-pass from the data.
/// Unread members are <c>NaN</c> when the data does not support them (n &lt; 2, zero spread).
/// </summary>
internal class MomentsReference
{
    public double Mean = double.NaN;
    public double Variance = double.NaN;
    public double StandardDeviation = double.NaN;
    public double PopulationVariance = double.NaN;
    public double Skewness = double.NaN;
    public double Kurtosis = double.NaN;
    public double ExcessKurtosis = double.NaN;
}
