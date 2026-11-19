using System;
using System.Collections.Generic;

namespace RunningStatistics.Tests;

/// <summary>
/// Deterministic datasets and reference calculations shared by the correctness gates; the workflow
/// they follow is described in the house-rules file at the root of the repository. Datasets come
/// from a fixed-seed random generator so a failing gate reproduces exactly, and they cover the
/// shapes an incremental algorithm has to survive: streams whose mean sits far off zero, from
/// equal to its spread up to twelve orders of magnitude beyond it, constant data, and small counts.
///
/// The reference values here are computed in two passes, the mean first and then the central sums,
/// with compensated summation. They are correct to within machine epsilon on every dataset these
/// helpers generate. A candidate running statistic is judged against those references, not
/// against the production implementation.
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

    /// The next value, drawn evenly from zero up to, but not including, one.
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

    /// Draws <paramref name="n"/> values evenly from zero up to, but not including, one.
    public static List<double> UniformUnit(int n, long seed)
    {
        Seed(seed);
        var xs = new List<double>();
        for (var i = 0; i < n; i++) xs.Add(NextUnit());
        return xs;
    }

    /// <summary>
    /// Draws <paramref name="n"/> uniform values from <c>offset</c> up to, but not including,
    /// <c>offset</c> plus <c>spread</c>. How many times larger the offset is than the spread is
    /// the condition number that makes naive raw-power-sum algorithms blow up.
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

    /// <summary>
    /// A stream whose mean is about <paramref name="kappa"/> and whose spread is one, so the
    /// condition number, the mean measured in spreads, is <paramref name="kappa"/>.
    /// </summary>
    public static List<double> KappaStream(int n, double kappa, long seed) => OffsetSpread(n, kappa, 1.0, seed);

    /// <summary>
    /// Streams whose condition number, the mean measured in spreads, walks through powers of ten
    /// from one up to ten to the <paramref name="decades"/>th power: the range where an accurate
    /// algorithm must stay accurate and a raw-power-sum algorithm does not.
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

    /// <summary>Builds <paramref name="count"/> copies of <paramref name="value"/>: data with no spread.</summary>
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

    /// Adds the values with Neumaier compensation, accurate to within about one epsilon no matter
    /// the order of the terms or their sizes.
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
    /// Compares two values with a relative tolerance, treating two NaNs as agreeing. The tolerance
    /// is usually derived from the condition number of the data: for a stable algorithm whose
    /// worst error is a small multiple of the condition number times machine epsilon, a margin
    /// built as a billionth plus one hundred times the condition number times machine epsilon
    /// is a typical choice.
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
    /// Compares two values with an absolute tolerance, treating two NaNs as agreeing. Use it for
    /// the standardized moments, skewness and kurtosis: their true values are small ratios, while
    /// the error a stable algorithm makes lives on the data's own scale, a small multiple of the
    /// condition number times machine epsilon measured in units of the standard deviation to the
    /// third and fourth powers. A relative tolerance on those small ratios would demand accuracy
    /// beyond what the stored mean can represent.
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
/// Reference values for the four-moment readout contract, computed in two passes from the data.
/// The members the data cannot support, such as when there are fewer than two values or no
/// spread at all, read NaN.
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
