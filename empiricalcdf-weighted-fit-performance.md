# EmpiricalCdf weighted-fit performance plan

**Status: open proposal.** Today's `EmpiricalCdf.Fit(double value, long count)` is pinned AS-IS in
`main` (docs on the overload + `Bench_EmpiricalCdfWeightedFitIsPerObservationCost` fact) — replay,
`O(count)`, bitwise-identical to a loop of single fits. Nothing in production behavior may change
until this plan's gates pass. Follow the staged workflow in `AGENTS.md` (candidate class in
`RunningStatistics.Tests` → correctness gate → A/B benchmark → swap → cleanup).

## Current contract (pinned, committed)

- `Fit(value, count)` replays `value` through `UncheckedFit` `count` times
  (`RunningStatistics/Statistics/EmpiricalCdf.cs:92-103`).
- State update per observation: circular buffer slot `Nobs % NumBins`, `_extrema.Fit(value)`,
  and on every `NumBins`-th observation: `Array.Sort(_buffer)` + `NumBins × Utils.Smooth` blend
  with **`alpha = NumBins / Nobs`** at that flush. Alpha depends on the RUNNING count, so no single
  closed-form blend represents a batch — that is what the doc comment says.
- Measured on this machine (fact's recorded numbers, bins = 25, count = 200):
  weighted ≈ **74 ns / replayed observation**, explicit loop ≈ **84 ns**, ratio ≈ **0.877–0.881**
  (weighted only slightly cheaper: one interface dispatch + finite-check per `count`, not per obs).

## Where the cost sits

Per replayed observation (~70 ns): interface dispatch, `Nobs % NumBins` index math, buffer store,
branchy `_extrema.Fit` (min/max compare + Nobs++). Amortized flush cost (~per NumBins obs):
dual-pivot sort of the buffer + `NumBins` smooth-blends. At `bins = 25` the per-observation
work (buffer store + extrema + dispatch) dominates; at large `count` it is pure linear replay.

## Proposed improvement: bitwise-preserving batched replay

Candidate class `EmpiricalCdfBatchedFit` in `RunningStatistics.Tests` (same public surface:
`Fit(double)`, `Fit(double, long)`, `Reset`, `CloneEmpty`, `Merge`, `ToString`, getters) so it can
drop in unchanged later. It must remain **bitwise** equal to loop replay — that equivalence is a
released contract (results reproducible across `Merge` replay, `ToString`, user accumulation).

Sketch (the only per-flush blend order in replay is sequential over flushes, k = 0..NumBins−1 —
keeping that loop is what buys bitwise exactness):

```
unchecked Fit(value, count):
  i = Nobs % NumBins
  // 1) prefix: fill slots i..NumBins-1 with value, extrema advanced in O(1) for the segment
  //    (verify Extrema.Fit(value, n) ≡ loop bitwise first — gate G0 covers this)
  prefix = min(count, NumBins - i)
  bulk buffer-store + extrema segment
  count -= prefix
  if buffer just filled:
     sort only if the window mixes old and new values (mixed prefix ⇒ sort as today)
     blend NumBins values with alpha = NumBins / Nobs          // exactly replay's ops, same order
  // 2) full windows: each next NumBins observations are ALL value ⇒ the sorted run is value-repeated
  //    (skip Array.Sort; the buffer flag "uniform window" means sort is an observed no-op)
  while count >= NumBins:
     extrema segment O(1); Values[k] = Smooth(Values[k], value, NumBins / Nobs) per flush
     count -= NumBins
  // 3) tail: same buffer stores + extrema segment, no flush unless the window completes
```

Expected effect: per-observation dispatch/store/extrema work collapses to O(1) per *segment*;
remaining cost is the unavoidable sequential per-flush blends + prefix bookkeeping,
≈ `count / NumBins` flushes. Target ns/replayed-obs ratio vs today ≈ **0.3–0.5 at bins = 25**
(blends + flush bookkeeping survive), even better for larger `NumBins`.

### Rejected alternative (do not try)

Geometric composition of the blend chain in one product
(`Π (1 − α_j)` closed form, O(1) flushes): **not bitwise equal** (different rounding order),
fragile in the same κ sense the Moments gates police, and it would silently change released
results. Rejected per AGENTS.md "never trade correctness for performance".

## Correctness gates FIRST (per AGENTS.md step 2 — all green before any perf talk)

Gate facts named so `--filter` runs them by bare name (runner matches bare method names only):

- `Gate_EmpiricalCdfBatchedBitwiseEqualsReplay` — for `NumBins ∈ {2, 7, 25, 200}`, seeded data
  (`PropertyHelpers.UniformUnit`/`NormalSamples`, fixed seeds; never unseeded RNG) and counts
  `∈ {0, 1, count < NumBins, exact multiples, spans across flush boundaries, > 10·NumBins}`,
  interleaved with ordinary single fits and a partially-filled-buffer start (`Nobs % NumBins ≠ 0`):
  candidate state ≡ literal replay loop, **bitwise** (`Assert.Equal`, tol 0): `Nobs`, `Min`, `Max`,
  `Median`, `Quantile` at the p-set the ECdf facts use, `Cdf` at the x-set, `ToString`.
- `Gate_EmpiricalCdfBatchedDegenerateCounts` — `count = 0` no-ops (state untouched), non-negative
  contract, finite-value contract (NaN/±Inf throw paths unchanged), `Reset`/`CloneEmpty` unchanged
  surface, merge-after-batch ≡ merge-after-loop.
- Re-run ALL existing ECdf facts against the candidate (including the pinned Bench equivalence
  half) — they encode the released contract; a red existing fact is a veto.

## A/B benchmark gates (only after the correctness gates pass)

Use `BenchmarkHarness` (fixed-seed data outside the timed region; interface-dispatch factories so
the ratio is conservative). New scratch bench facts `Bench_EmpiricalCdfBatched*` (deleted at the
end; the harness itself stays):

- `WeightedFit` scenario today's impl (baseline) vs candidate, same data, bins = 25 and bins = 200:
  candidate/baseline ns-per-replayed-obs must be **≤ 1.05** (the claim is strictly-faster; if the
  candidate can't beat 1.05, the change is REJECTED, not landed at parity).
- `StreamFit` 1k / 1M working sets and readout getters: candidate ≤ 1.20× / 1.05× per AGENTS.md
  bands (single Fit(value) path must not regress — the batching lives inside the weighted
  overload, so these are parity gates).

## Swap and re-gate (AGENTS.md steps 4–5)

1. Move the candidate body into production `EmpiricalCdf.Fit(double, long)` (drop-in; same name —
   bitwise equivalence means no downstream fact or contract needs reinterpreting).
2. Update the `///` cost note: O(count) → "O(count / NumBins) flush blends + O(NumBins) boundary
   bookkeeping; equivalent to — and bitwise identical with — a loop of single fits".
3. `Bench_EmpiricalCdfWeightedFitIsPerObservationCost` stays meaningful but its band moves:
   rewrite the ratio assertion to the improvement claim (e.g. `[0.1, 0.5]` at bins = 25) and keep
   the equivalence half untouched at tol 0. A red bench after the swap = regression; re-gate, don't
   re-tune semantics.
4. Full suite green before/after (currently 929 facts); candidate and scratch bench files deleted;
   `BenchmarkHarness.cs` / `PropertyHelpers.cs` kept.

## Open questions for the user

- Land the batched body under the SAME name `Fit(value, count)` (recommended — bitwise-equivalent,
  all callers benefit silently) vs a new method (`FitBatched`) with the old replay kept public for
  source-level clarity? Recommendation: same name, given the equivalence gate.
- Verify `Extrema.Fit(value, n)` (or an internal `UncheckedFitSegment`) is bitwise ≡ loop before
  using it in the prefix/tail fast-forward — currently unverified, gate G0 makes it a requirement.
