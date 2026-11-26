# Developer vocabulary

This file defines the words this repository uses. `AGENTS.md` carries the
workflow rules; this file only says what the terms mean. The same words show
up in test-fact names, in the `plans/` files, and in contributor notes.

## Test facts

A test fact is a `[Fact]`-marked method in `RunningStatistics.Tests`. The
suite runs them with `dotnet test`. The name of a fact carries its kind, and
the runner's `--filter` matches a full bare name.

- **gate** — a fact named `Gate_...`. It pins correctness: a number, an
  exception, a merge result. A red gate is a real bug in an algorithm. Gates
  never measure time.
- **bench fact** — a fact named `Bench_...`. It times two implementations of
  the same work over identical seeded data, forms a ratio of their
  nanoseconds per observation, and asserts the ratio sits inside a band. A
  red bench fact means a performance regression, not a wrong number.
- **band** (also **budget**) — the ratio range a bench fact allows. For
  example the readout budget `≤ 1.05` says the new readout costs at most
  5% more per call than the old one; the fit budgets allow at most 20% more
  arithmetic on compute-hot loops. A band states an expectation about the
  algorithm's cost.
- **re-gate** — when a bench fact turns red after an algorithm change,
  re-derive the honest band from the algorithm's new cost — or fix the
  algorithm. Never widen a band just to make a red fact green again; the
  band is the claim, and loosening it deletes the claim.

## Measurement terms

- **ns/obs** — nanoseconds per observation (per fit, per query).
  `BenchmarkHarness.Measure` normalizes a timed pass to this unit and prints
  it.
- **warm-up** — measured passes are preceded by passes the harness does not
  time, so the optimizer and caches get a fair first shot. The harness keeps
  its own warm-ups unmeasured before timing the best of `repeats`.
- **checksum / sink** — every timed pass folds a running sum into the
  result. The optimizer cannot delete timed work, because deleting it would
  change the sum.
- **working set** — the amount of data a benchmark streams. A 1k set fits in
  cache, so the loop measures arithmetic time. A 1M set does not, so the
  loop measures memory time. The two get different bands.
- **baseline / candidate** — the baseline is the current production
  implementation; the candidate is a staged replacement living in the test
  project (see *staged candidate* below). A bench fact runs both and divides
  candidate time by baseline time.
- **interface dispatch** — the harness drives both sides through
  `IRunningStatistic<double>` factories, so each `Fit` costs one identical
  interface call on both sides. This makes the reported ratio conservative:
  it understates how much more arithmetic a slower candidate really spends.
- **seeded** — generated from a fixed seed, so a fact gets exactly the same
  dataset on every run. Accuracy facts must be seeded; an unseeded generator
  makes a fact flaky and proves nothing.

## Data shapes

- **κ (kappa)** — the absolute mean of a dataset divided by its spread. It
  measures how badly conditioned the data is. A numerically fragile
  algorithm loses accuracy about as fast as κ grows; fragile power-sum
  tricks lose at κ².
- **κ-sweep** — a family of datasets that runs κ from 1 to 1e12. A correct
  stable algorithm answers every member as well as a two-pass reference does.
- **canceling stream / canceling family** — a dataset built so huge terms
  cancel: a run of ones plus a huge term and its negation, as with the
  condition numbers 1e12 through 1e18. It probes whether an algorithm
  reabsorbs the cancellation into a lost low-order bit.
- **duplicate-heavy** — a stream with many repeated values, from a small
  multiset or from snapping uniform draws. Repeats keep equal neighbors in a
  window and test the "first strictly greater" search contracts, where an
  upper-bound search must agree with a scan, not just find any match.
- **degenerate case** — a boundary input that could break a formula: too
  few observations (`NaN` reads out), constant data, a zero-width span, an
  empty merge.
- **sweep** — a loop over a systematic set of inputs: bin sizes, a grid of
  query points, a grid of probabilities. Sweeps turn one fact into coverage
  of a whole contract instead of one lucky point.
- **bit-for-bit (bitwise) equality** — the exact same `double` bits out of
  both sides, compared with no tolerance. Weighted fits and their replay
  loops promise this; tolerance is reserved for approximation claims.

## Algorithm-change workflow

- **staged candidate** — the proposed algorithm replacement, first written
  into the test project and named `<Name>Stable` or `<Name>Candidate`. It
  carries the same public surface as the production class, so gates and
  benches can drop it in unchanged. The production class stays untouched
  while the candidate is in staging.
- **gate first, swap later** — the rule from `AGENTS.md`: a candidate runs
  the correctness gates before anyone discusses speed, then the bench bands,
  and the production class changes only after both pass. The candidate and
  its scratch benches are then deleted; the harness, the helpers, and the
  permanent facts stay.
- **replay** — a literal loop of single-observation fits. Batched or
  weighted overloads must leave exactly the state a replay loop builds, in
  the same order, bit-for-bit.
