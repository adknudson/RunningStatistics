using System;
using System.Collections.Generic;

namespace RunningStatistics.Tests;

/// <summary>
/// Reusable wall-clock timing tool. It detects performance regressions when the algorithm of
/// a running statistic changes. See the workflow in <c>AGENTS.md</c>.
///
/// A closure drives the timed work. It can replay one workload against a baseline and
/// against a candidate in one process. The two runs read the same fixed-seed data. The
/// generator always builds that data outside the timed region. A result is the minimum of the
/// measured <c>repeats</c>, after warm-up passes that go unmeasured. Each timed action
/// folds a checksum into a sink, so the optimizer cannot delete the work.
/// </summary>
internal static class BenchmarkHarness
{
    /// Keeps timed (and warm-up) work alive. The optimizer cannot remove a loop that feeds this.
    private static long _sink;

    private static long Sink { get; } = _sink;

    /// <summary>
    /// Times one pass of <paramref name="pass"/> and returns nanoseconds per observation.
    /// <paramref name="pass"/> runs the whole timed workload, for example one whole array.
    /// It returns a checksum, and Measure folds that checksum into the sink.
    /// </summary>
    /// <param name="label">Printed with the result.</param>
    /// <param name="obs">Number of observations/iterations the pass performs, for normalization.</param>
    /// <param name="repeats">Measured passes. The fastest wins.</param>
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
    /// Fits every element of <paramref name="data"/> once each. Each timed pass starts from a
    /// fresh statistic. The result is nanoseconds per observation.
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
    /// Fits every element with one weighted call each. Each timed pass starts from a fresh
    /// statistic. The result is nanoseconds per observation.
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
    /// Builds <paramref name="chunks"/> statistics of equal size, fits them, and merges them all
    /// into a fresh statistic. User code pays for merge costs and for the clone that the base
    /// class default relies on. Those costs belong in the measurement. The result is nanoseconds
    /// per observation.
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
    /// Fits every element and takes a snapshot copy of the statistic after each observation, the
    /// accumulate-into-a-clone idiom. The result is nanoseconds per observation.
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
