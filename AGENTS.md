# AGENTS.md

This file gives rules to automated contributors and to future sessions of this
project. The rules cover work on the `RunningStatistics` library.

## Algorithm improvements: benchmarking and regression gates

Follow this workflow before you replace the algorithm inside a running
statistic. One example: replace `Moments` with a numerically stable
central-moment update. The workflow proves two things about the change. The
new algorithm is correct, and it is not slower. Two shared files make the
workflow reusable. Do not rebuild them for each statistic:

- `RunningStatistics.Tests/BenchmarkHarness.cs` — a timed-loop harness. It
  provides `Measure`, `Compare`, and scenario helpers. The harness
  generates data outside the timed region with a seed set in advance. Every
  implementation therefore sees identical inputs.
- `RunningStatistics.Tests/PropertyHelpers.cs` — deterministic dataset
  builders and reference helpers. The builders produce uniform values,
  offset/spread datasets, and normal samples. `MomentsReference` computes
  reference values with exact or two-pass methods. `Within` and `Describe`
  compare values with a tolerance.

### Workflow

1. Stage the change first. Do not edit the production class first.
   Implement the candidate in the `RunningStatistics.Tests` project. Name
   it `<Name>Stable` or `<Name>Candidate`. Give it the same public
   surface: `Fit`, `Reset`, `CloneEmpty`, `Merge`, `ToString`, and the
   getters. The same surface lets the candidate drop in unchanged later.
2. Run the correctness gate before you discuss performance. Compare the
   candidate against the `PropertyHelpers` reference on κ-sweep
   datasets. κ is the absolute mean divided by the spread. In a κ-sweep,
   that ratio runs from 1 to 1e12. Also test these degenerate cases:
   - `Nobs` below 2 must return `NaN`.
   - The data is constant.
   - `Fit(value, count)` must give the same result as `count` single
     fits of that value.
   - Merge must be associative and commutative.
   - A `Clone()` must return a state equivalent to the original.
   Every check must pass before any performance claim. Record how badly
   the current implementation fails these same checks. Print that record.
   Do not assert it. That record states the win you claim.
3. Run the A/B benchmark. Run `BenchmarkHarness` over the baseline and
   the candidate on the same seeded data. Compare
   candidate-ns-per-observation to baseline-ns-per-observation. The
   gates:
   - `StreamFit` with a 1k working set: the ratio must stay at most 1.20.
     The set stays in cache, so the loop is compute-bound.
   - `StreamFit` with a 1M working set: the ratio must stay at most 1.05.
     This models realistic streaming, and that work is memory-bound.
   - `WeightedFit`, `MergeChunks`, and `ClonePerStep`: the ratio must
     stay at most 1.20.
   - The readout getters: the ratio must stay at most 1.05. A getter
     costs one interface dispatch plus a few floating-point operations.
     Measurement noise of about 1% straddles 1.00, so the honest claim
     for a getter is "not slower", not "faster".
4. Swap the production class only after steps 2 and 3 pass. Re-run the
   harness against the real class as a final gate. Run the property
   checks against the real class too.
5. Clean up. Delete the candidate class and the bench scratch file. Keep
   `BenchmarkHarness.cs` and `PropertyHelpers.cs` permanently.

### Running

```
dotnet test RunningStatistics.Tests/RunningStatistics.Tests.csproj
dotnet test RunningStatistics.Tests/RunningStatistics.Tests.csproj --filter "Gate_KappaSweep_StableWithinTolerance"
```

You can also run the suite through the IDE's Unit Tests window. Bench and
gate facts carry the `Gate_` and `Bench_` name prefixes, so you can run
one fact alone. The runner's `--filter` matches a full bare method name
only. A pattern such as `Gate_*` or `Class.*` matches nothing, and the
runner reports no error for it. A red `Bench_*` fact means a
performance regression. The asserts embed the budgets and the measured
ns/obs.

### Notes

- Do not add BenchmarkDotNet. It shells out to generate and compile its
  own harness code. That path does not compose with this project's
  dialect and toolchain.
- The budgets carry this meaning: the new algorithm keeps the same big-O
  and the same O(1) state. It keeps wall-clock parity on realistic
  memory-bound streams. It may spend at most 20% more arithmetic on
  compute-hot micro-loops.
- Never trade correctness for performance. Reject any fallback that
  re-tunes speed but fails the correctness gate. Kahan-compensated raw
  power sums are one such rejected fallback. They remain fragile at
  κ².
- The 10M-sample distribution facts (`UnitUniformSample_*`,
  `NormalSample_*`, `LogNormalSample_*`) use seeded generators, with the seed
  set in advance. Box-Muller
  on `new Random(seed)` replaced the original unseeded streams in 2026-11.
  The original unseeded streams made the facts flaky at the 0.01
  tolerances. About 1 full-suite run in 3 failed
  `LogNormalSample_MomentsMatchTheory`. Those failures happened on both
  the old and the new `Moments`. They were sampling noise, not a
  regression. Keep sample facts seeded. Never reintroduce
  `new Random()` or MathNet `dist.Sample()` in accuracy facts. Both draw
  from a global unseeded RNG.
- For timing in this dialect, use `new System.Diagnostics.Stopwatch()`
  with `Start` and `Stop`. Read the total with `sw.Elapsed.TotalNanoseconds`.
  Do not use `sw.Elapsed.Nanoseconds`. That value is the 0..999 component
  of the duration, not the total. A ratio built from it is garbage.
  `Stopwatch.GetElapsedTime()` has no zero-argument overload in this
  dialect.
- The harness times both implementations through
  `IRunningStatistic<double>` factories. Both therefore pay one
  identical interface dispatch per `Fit`. That dispatch compresses the
  difference between the two timings, so the reported ratio is
  conservative: it understates the candidate's arithmetic cost. A scratch
  bench may add a concrete-typed direct loop when it needs a tighter
  signal.
