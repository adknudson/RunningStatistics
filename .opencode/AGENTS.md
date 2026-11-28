# AGENTS.md

This file gives rules to automated contributors and to future sessions of
this project. The rules cover work on the `RunningStatistics` library.

## Algorithm improvements: benchmarking and regression gates

Follow this workflow before you replace the algorithm inside a running
statistic. One example: replace `Moments` with a numerically stable
central-moment update. The workflow proves two things about the change.
The new algorithm is correct. It is not slower. Two shared files make the
workflow reusable. Do not rebuild them for each statistic:

- `RunningStatistics.Tests/BenchmarkHarness.cs` — a timed-loop harness. It
  provides `Measure`, `Compare`, and scenario helpers. The harness
  generates data outside the timed region and sets its seed in advance.
  Every implementation therefore sees identical inputs.
- `RunningStatistics.Tests/PropertyHelpers.cs` — deterministic dataset
  builders and reference helpers. The builders produce uniform values,
  offset/spread datasets, and normal samples. `MomentsReference` computes
  reference values with exact or two-pass methods. `Within` and `Describe`
  compare values with a tolerance.

Prove performance with a scratch bench file, not with a committed test
fact. Do not add `Bench_*` facts to the test project. Bench facts add
noise to the unit-test suite. They run on every `dotnet test`. Machine
noise makes their bands flaky. A red band tempts you to re-tune the band.
This file forbids that habit.

When you want to know whether a new algorithm is faster, or at least not
statistically slower:

1. Run `BenchmarkHarness` in a scratch bench file.
2. Print the measured ns/obs and the ratios.
3. Put that record in your report or commit message.
4. Delete the scratch bench file.

### Workflow

1. Stage the change first. Do not edit the production class first.
   Implement the candidate in the `RunningStatistics.Tests` project. Name
   it `<Name>Stable` or `<Name>Candidate`. Give it the same public
   surface: `Fit`, `Reset`, `CloneEmpty`, `Merge`, `ToString`, and the
   getters. The candidate can then replace the production class later
   with no change to that surface.
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
3. Run the A/B benchmark in the scratch bench file. Run `BenchmarkHarness`
   over the baseline and the candidate on the same seeded data. Compare
   candidate-ns-per-observation to baseline-ns-per-observation. The
   budgets for that ratio:
   - `StreamFit` with a 1k working set: the ratio must stay at most 1.20.
     The set stays in cache. The loop is therefore compute-bound.
   - `StreamFit` with a 1M working set: the ratio must stay at most 1.05.
     This case models realistic streaming. That work is memory-bound.
   - `WeightedFit`, `MergeChunks`, and `ClonePerStep`: the ratio must
     stay at most 1.20.
   - The readout getters: the ratio must stay at most 1.05. A getter
     costs one interface dispatch plus a few floating-point operations.
     Measurement noise of about 1% straddles 1.00. The honest claim for
     a getter is "not slower", not "faster".
4. Swap the production class only after steps 2 and 3 pass their budgets.
   Re-run the scratch bench against the real class as the final
   measurement. Run the property checks against the real class too.
5. Clean up. Delete the candidate class and the scratch bench file. Keep
   `BenchmarkHarness.cs` and `PropertyHelpers.cs` permanently.

### Running

```
dotnet test RunningStatistics.Tests/RunningStatistics.Tests.csproj
dotnet test RunningStatistics.Tests/RunningStatistics.Tests.csproj --filter "Gate_KappaSweep_StableWithinTolerance"
```

You can also run the suite through the IDE's Unit Tests window. Gate
facts carry the `Gate_` name prefix. You can therefore run one fact
alone. The runner's `--filter` matches a full bare method name only. A
pattern such as `Gate_*` or `Class.*` matches nothing. The runner reports
no error for that pattern. The suite holds gates only. A performance
claim lives in the scratch bench's printed record and in the report that
carries it. It never lives in a committed fact.

### Notes

- Do not add BenchmarkDotNet. It generates and compiles its own harness
  code through a shell. That path does not work with this project's
  dialect and toolchain.
- The budgets mean this: the new algorithm keeps the same big-O and the
  same O(1) state. It keeps wall-clock parity on realistic memory-bound
  streams. It may spend at most 20% more arithmetic on compute-hot
  micro-loops.
- Never trade correctness for performance. Reject any fallback that
  re-tunes speed but fails the correctness gate. Kahan-compensated raw
  power sums are one such rejected fallback. They remain fragile at
  κ².
- The 10M-sample distribution facts (`UnitUniformSample_*`,
  `NormalSample_*`, `LogNormalSample_*`) use seeded generators. The
  generators set their seed in advance. Box-Muller on `new Random(seed)`
  replaced the original unseeded streams in 2026-11. The original
  unseeded streams made the facts flaky at the 0.01 tolerances. About
  1 full-suite run in 3 failed `LogNormalSample_MomentsMatchTheory`.
  Those failures happened with both the old `Moments` and the new
  `Moments`. They were sampling noise, not a regression. Keep sample
  facts seeded. Never reintroduce `new Random()` or MathNet
  `dist.Sample()` in accuracy facts. Both draw from a global unseeded
  RNG.
- For timing in this dialect, use `new System.Diagnostics.Stopwatch()`
  with `Start` and `Stop`. Read the total with
  `sw.Elapsed.TotalNanoseconds`. Do not use `sw.Elapsed.Nanoseconds`.
  That value is the 0..999 nanosecond component of the duration, not the
  total. A ratio built from that value is garbage. `Stopwatch.GetElapsedTime()`
  has no zero-argument overload in this dialect.
- The harness times both implementations through
  `IRunningStatistic<double>` factories. Both therefore pay one
  identical interface dispatch per `Fit`. That dispatch compresses the
  difference between the two timings. The reported ratio is therefore
  conservative. It understates the candidate's arithmetic cost. A
  scratch bench may add a concrete-typed direct loop when it needs a
  tighter signal.

## GitHub issue workflow

Use this workflow when you work on an issue from GitHub.

1. Create a new branch when you start work on the issue. Name it
   `fix/##-description`. `##` is the issue number.
2. Commit your changes to that branch.
3. When you finish the first round of work, create a draft pull request.
4. Never merge a pull request. Never approve one. The user approves and
   merges it.
