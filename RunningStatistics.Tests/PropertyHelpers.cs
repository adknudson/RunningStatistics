using System;
using System.Collections.Generic;

namespace RunningStatistics.Tests;

/// <summary>
/// The correctness gates share these helpers. The house-rules file at the repository root
/// describes the workflow they follow. A fixed-seed random generator builds every dataset, so
/// a failing gate reproduces exactly. The datasets cover the shapes an incremental algorithm
/// must survive. They include streams whose mean sits far off zero, from equal to its spread
/// up to twelve orders of magnitude beyond it. They also include constant data and small counts.
///
/// A two-pass method computes each reference value: the mean first, then the central sums,
/// with compensated summation. Each reference matches its dataset to within machine epsilon.
/// The gates judge a candidate running statistic against these references, not against the
/// production implementation.
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
        return u / 9007199254740992.0;
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
    /// <c>offset</c> plus <c>spread</c>. The ratio of the offset to the spread is the condition
    /// number. A high condition number makes naive raw-power-sum algorithms blow up.
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
    /// <paramref name="n"/> values in blocks: one large power-of-two term, a run of ones,
    /// then the negation of that large term. Every term is an exact double and a block's
    /// exact sum is its count of ones, so the reference carries no rounding of its own and
    /// the difference two folds make is the fold's, not the data's. That ratio, the large
    /// term over a block's sum, is the condition number <paramref name="kappa"/> of the
    /// summation problem. Once the large term passes 2^53 its spacing exceeds one, so a
    /// plain fold drops every one as it accumulates and its error is a whole block; a
    /// compensated fold keeps them. Below 2^53 a plain fold is already exact on this data,
    /// and the record says so. The exponent jitter keeps the datasets distinct.
    /// </summary>
    public static List<double> CancelingStream(int n, double kappa, long seed)
    {
        Seed(seed);
        var ones = Math.Max(2, n / 4);
        var blocks = Math.Max(1, n / (ones + 2));
        var exponent = (int)Math.Ceiling(Math.Log2(Math.Max(ones * kappa, 1.0))) + (int)(NextUnit() * 4.0);
        var big = Math.Pow(2.0, exponent);
        var xs = new List<double>();
        for (var b = 0; b < blocks; b++)
        {
            xs.Add(big);
            for (var i = 0; i < ones; i++) xs.Add(1.0);
            xs.Add(-big);
        }

        return xs;
    }

    /// <summary>
    /// A stream whose mean is about <paramref name="kappa"/> and whose spread is one, so the
    /// condition number, the mean measured in spreads, is <paramref name="kappa"/>.
    /// </summary>
    public static List<double> KappaStream(int n, double kappa, long seed) => OffsetSpread(n, kappa, 1.0, seed);

    /// <summary>
    /// Each stream sets a condition number: the mean measured in spreads. The condition number
    /// walks through powers of ten, from one up to ten to the <paramref name="decades"/>th power.
    /// This range is where an accurate algorithm must stay accurate and a raw-power-sum algorithm
    /// does not.
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

    /// Reference value for a sum readout: the compensated sum of the values. Gates for
    /// summation algorithms compare a candidate against this, not against the production
    /// implementation. The condition number of the data enters the honest tolerance the
    /// same way it does for the moment gates: a fold that keeps about one epsilon of
    /// error per running total spends an absolute error of a few times kappa times
    /// epsilon, measured in the magnitude of the running totals.
    public static double SumReference(List<double> xs) => CompensatedSum(xs);

    /// Reference value for a mean readout: the compensated sum of the values divided by their
    /// count. Gates for averaging algorithms compare a candidate against this, not against
    /// the production implementation.
    public static double MeanReference(List<double> xs) => CompensatedMean(xs);

    private static double CompensatedMean(List<double> xs) => CompensatedSum(xs) / xs.Count;

    // ---- Tolerant comparison helpers ----

    /// <summary>
    /// Compares two values with a relative tolerance. Two NaNs agree. The caller usually derives
    /// the tolerance from the condition number of the data. Consider a stable algorithm whose
    /// worst error is a small multiple of the condition number times machine epsilon. For that
    /// algorithm, a good margin is a billionth plus one hundred times the condition number
    /// times machine epsilon.
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
    /// Compares two values with an absolute tolerance. Two NaNs agree. Use it for the
    /// standardized moments, skewness and kurtosis. Their true values are small ratios.
    ///
    /// A stable algorithm makes its error on the data's own scale. That error is a small
    /// multiple of the condition number times machine epsilon. The units for that multiple are
    /// the standard deviation to the third and fourth powers. A relative tolerance on those
    /// small ratios would demand accuracy beyond what the stored mean can represent.
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
