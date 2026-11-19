# AGENTS.md

Notes for automated contributors (and future sessions of this project) working on the
`RunningStatistics` library.

## Comment tone: write for a human reader

Every comment in this repository must read like normal human speech — complete sentences in
plain English a newcomer could read once and understand, with no mental translation.

- No telegraphic notes: no sentence fragments, no arrow chains (`->`), no `≡`/`κ`/`α`/`ε` or
  other shorthand standing in for words, no "i.e." cram, no dense parenthetical asides that
  pack a paragraph into one line.
- State *what is being tested and why it matters* first; keep measured numbers, tolerances and
  budget bands as plain sentences (see the rewritten headers over the
  `Bench_GetterCostIndependentOfNobs_*` facts — they are the reference style).
- This applies to `//` comments, `///` doc comments and new facts alike; when a gate or bench
  is added or revised, its header comment gets rewritten to this tone in the same change.

## Subagent rules

- Spawn only ONE subagent at a time, no exceptions. Wait for the current subagent to finish
  before launching the next; never issue parallel subagent calls.

## Algorithm improvements: benchmarking and regression gates

When replacing the algorithm inside a running statistic (e.g. swapping `Moments` for a
numerically stable central-moment update), follow this workflow so the change is provably
correct and provably not slower. Two pieces of infrastructure make it reusable —
do not reinvent them for each statistic:

- `RunningStatistics.Tests/BenchmarkHarness.cs` — generic timed-loop harness
  (`Measure`, `Compare`, scenario helpers). Data is generated outside the timed region
  with a fixed seed, so every implementation sees identical inputs.
- `RunningStatistics.Tests/PropertyHelpers.cs` — deterministic dataset builders
  (uniform, offset/spread κ-sweep, normal samples), exact/two-pass reference computations
  (`MomentsReference`) and tolerant comparison helpers (`Within`, `Describe`).

### Workflow

1. **STAGED CHANGE.** Never edit the production class first. Implement the candidate as
   `<Name>Stable`/`<Name>Candidate` in the `RunningStatistics.Tests` project with the same
   public surface (`Fit`, `Reset`, `CloneEmpty`, `Merge`, `ToString`, getters) so it can
   drop in unchanged later.
2. **CORRECTNESS GATE FIRST.** Compare candidate vs `PropertyHelpers` reference on
   κ-sweep datasets (κ = |mean|/spread = 1 … 1e12), plus degenerate cases:
   `Nobs < 2` → `NaN`, constant data, `Fit(value, count)` ≡ `count` identical fits,
   merge associativity/commutativity, `Clone()` round-trip. All must pass before any
   perf talk. Record (print, do not assert) how badly the *current* implementation fails
   the same checks — that is the win being claimed.
3. **A/B BENCHMARK.** Run `BenchmarkHarness` over baseline and candidate on the same
   fixed-seed data. Gates (candidate/baseline ns-per-observation):
   - `StreamFit` 1k working set (cache-hot, compute-bound) → ≤ **1.20×**
   - `StreamFit` 1M working set (memory-bound, realistic streaming) → ≤ **1.05×**
   - `WeightedFit`, `MergeChunks`, `ClonePerStep` → ≤ **1.20×**
   - readout getters → ≤ **1.05×** (parity: a getter is one interface dispatch plus a few flops,
     so ~1% measurement noise straddles 1.00; the claim is "not slower", not "faster")
4. **SWAP the production class** only after 2 and 3 pass. Re-run the harness against the
   real class as a final gate, and run the property checks against it too.
5. **CLEANUP.** Delete the candidate class and the bench scratch file; keep
   `BenchmarkHarness.cs` and `PropertyHelpers.cs` permanently.

### Running

```
dotnet test RunningStatistics.Tests/RunningStatistics.Tests.csproj
dotnet test RunningStatistics.Tests/RunningStatistics.Tests.csproj --filter "Gate_KappaSweep_StableWithinTolerance"
```
Or the IDE's Unit Tests window. Bench/gate facts are named `Gate_*`/`Bench_*` so they
can be run in isolation — but note the runner's `--filter` matches bare method names only;
patterns like `Gate_*` or `Class.*` silently match nothing. A red `Bench_*` fact means a
regression (the asserts embed the budgets and the measured ns/obs).

### Notes

- BenchmarkDotNet is deliberately not used: it shells out to generate and compile its own
  harness code, which does not compose with this project's dialect/toolchain build path.
- The budgets mean: same big-O, O(1) state, wall-clock parity on realistic (memory-bound)
  streams; at most 20% more arithmetic on compute-hot micro-loops.
- **Never trade correctness for performance.** A fallback that only re-tunes speed but
  fails the correctness gate (e.g. Kahan-compensated raw power sums, which are still
  κ²-fragile) is rejected.
- The 10M-sample distribution facts (`UnitUniformSample_*`, `NormalSample_*`, `LogNormalSample_*`)
  were reseeded with fixed-seed generators (Box-Muller on `new Random(seed)`) in 2026-11: the
  original unseeded streams made them inherently flaky at the 0.01 tolerances (~1 full-suite
  run in 3 failed `LogNormalSample_MomentsMatchTheory`, on BOTH the old and the new `Moments`
  — the flakiness, not a regression, was sampling noise). Keep sample-facts seeded; never
  reintroduce `new Random()` or MathNet `dist.Sample()` (global unseeded RNG) in accuracy facts.
- Timing in this dialect: `new System.Diagnostics.Stopwatch()` + `Start`/`Stop`, read the total
  with `sw.Elapsed.TotalNanoseconds`. `sw.Elapsed.Nanoseconds` is the duration's 0..999
  COMPONENT, not a total — it silently yields garbage ratios. `Stopwatch.GetElapsedTime()`
  takes no zero-argument overload here.
- The harness times work through `IRunningStatistic<double>` factories, i.e. both
  implementations pay one identical interface dispatch per `Fit`. That compresses
  differences, so the reported ratio is conservative (it understates the candidate's
  arithmetic cost); scratch benches may add a concrete-typed direct loop for a tighter
  signal.
