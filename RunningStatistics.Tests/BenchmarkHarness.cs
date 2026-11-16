using System;
using System.Collections.Generic;

namespace RunningStatistics.Tests;

/// <summary>
/// Reusable wall-clock harness for detecting performance regressions when the algorithm of a
/// running statistic is changed (see the workflow in <c>AGENTS.md</c>).
///
/// Timed work is driven by closures so the same workload can be replayed against a baseline and
/// against a candidate in the same process, on identical fixed-seed data (data generation always
/// happens outside the timed region). Results are min-of-<c>repeats</c> with unmeasured warm-up
/// passes, and every timed action folds a checksum into a sink so the optimizer cannot delete the
/// work.
/// </summary>
internal static class BenchmarkHarness
{
    /// Keeps timed (and warm-up) work alive; the optimizer cannot remove a loop feeding this.
    private static long _sink;

    private static long Sink { get; } = _sink;

    /// <summary>
    /// Times one pass of <paramref name="pass"/> and returns nanoseconds per observation.
    /// <paramref name="pass"/> runs the whole timed workload (e.g. fitting a whole array) and
    /// returns a checksum that is folded into the sink.
    /// </summary>
    /// <param name="label">Printed with the result.</param>
    /// <param name="obs">Number of observations/iterations the pass performs, for normalization.</param>
    /// <param name="repeats">Measured passes; the fastest wins.</param>
    /// <param name="pass">Timed workload closure.</param>
    public static double Measure(string label, int obs, int repeats, Func<double> pass)
    {
        _sink += (long)pass();
        _sink += (long)pass();

        var best = double.PositiveInfinity;
        for (var r = 0; r < repeats; r++)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            _sink += (long)pass();
            sw.Stop();
            var ns = sw.Elapsed.TotalNanoseconds;
            if (ns >= 0 && ns < best) best = ns;
        }

        Console.WriteLine($"{label}: {best / obs:F4} ns/obs");
        return best / obs;
    }

    /// <summary>Reports the candidate/baseline ratio (and the inputs) for one scenario.</summary>
    public static double Compare(string scenario, double baselineNsPerObs, double candidateNsPerObs)
    {
        var ratio = candidateNsPerObs / baselineNsPerObs;
        Console.WriteLine($"{scenario}: ratio {ratio:F3} (baseline {baselineNsPerObs:F4}, candidate {candidateNsPerObs:F4} ns/obs)");
        return ratio;
    }

    /// <summary>
    /// Stream: <c>Fit(value)</c> for every element of <paramref name="data"/>, fresh stat per pass.
    /// Returns ns/obs.
    /// </summary>
    public static double StreamFit(Func<IRunningStatistic<double>> factory, List<double> data, int repeats)
    {
        var passes = Math.Max(1, 1_000_000 / Math.Max(1, data.Count));
        var obs = passes * data.Count;
        return Measure("StreamFit", obs, repeats, () => {
            var sum = 0.0;
            for (var p = 0; p < passes; p++)
            {
                var stat = factory();
                for (var i = 0; i < data.Count; i++)
                {
                    stat.Fit(data[i]);
                }

                sum += stat.Nobs;
            }

            return sum;
        });
    }

    /// <summary>
    /// Weighted: <c>Fit(value, count)</c> per element, fresh stat per pass. Returns ns/obs.
    /// </summary>
    public static double WeightedFit(Func<IRunningStatistic<double>> factory, List<double> data, long count, int repeats)
    {
        var passes = Math.Max(1, 1_000_000 / Math.Max(1, data.Count));
        var obs = passes * data.Count;
        return Measure("WeightedFit", obs, repeats, () => {
            var sum = 0.0;
            for (var p = 0; p < passes; p++)
            {
                var stat = factory();
                for (var i = 0; i < data.Count; i++)
                {
                    stat.Fit(data[i], count);
                }

                sum += stat.Nobs;
            }

            return sum;
        });
    }

    /// <summary>
    /// Merge-heavy: <paramref name="chunks"/> statistics of equal size are fitted and merged into a
    /// fresh stat. Costs of <c>Merge</c> (and, via the base class default, <c>Clone</c>) are what user
    /// code multiplies. Returns ns/obs.
    /// </summary>
    public static double MergeChunks(Func<IRunningStatistic<double>> factory, List<double> data, int chunks, int repeats)
    {
        var passes = Math.Max(1, 1_000_000 / Math.Max(1, data.Count));
        var obs = passes * data.Count;
        var perChunk = Math.Max(1, data.Count / chunks);
        return Measure("MergeChunks", obs, repeats, () => {
            var sum = 0.0;
            for (var p = 0; p < passes; p++)
            {
                var total = factory();
                for (var c = 0; c < chunks; c++)
                {
                    var chunk = factory();
                    for (var i = 0; i < perChunk; i++)
                    {
                        chunk.Fit(data[c * perChunk + i]);
                    }

                    total.UnsafeMerge(chunk);
                }

                sum += total.Nobs;
            }

            return sum;
        });
    }

    /// <summary>
    /// Clone-heavy: <c>Fit(value)</c> then <c>Clone()</c> per element (idiom: snapshot after each
    /// observation). Returns ns/obs.
    /// </summary>
    public static double ClonePerStep(Func<IRunningStatistic<double>> factory, List<double> data, int repeats)
    {
        var passes = Math.Max(1, 100_000 / Math.Max(1, data.Count));
        var obs = passes * data.Count;
        return Measure("ClonePerStep", obs, repeats, () => {
            var sum = 0.0;
            for (var p = 0; p < passes; p++)
            {
                var stat = factory();
                for (var i = 0; i < data.Count; i++)
                {
                    stat.Fit(data[i]);
                    var copy = stat.Clone();
                    sum += copy.Nobs;
                }
            }

            return sum;
        });
    }
}
