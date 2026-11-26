using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests.Beta;

// Audit gates for the Beta parameter estimator. The state is a pair of integer counts,
// so every bookkeeping contract is checked with exact integer equality, with no
// tolerance. The readouts turn those counts into doubles, and those get the
// condition-number-shaped margins. The audit run of 2026-11-19 found no failure and
// staged no candidate. A red fact means the estimator or a readout lost accuracy; fix
// the algorithm, do not widen a tolerance.
public partial class TestBeta
{
    [Fact]
    public void Gate_BetaCountBookkeepingExactAcrossRoutes()
    {
        var bools = new List<bool> { true, false, true, true, false, false, false, true };
        var successes = 0L;
        foreach (var b in bools)
        {
            if (b) successes++;
        }

        var failures = (long)bools.Count - successes;

        var loop = new RunningStatistics.Beta();
        foreach (var b in bools) loop.Fit(b);
        Assert.Equal(successes, loop.Successes);
        Assert.Equal(failures, loop.Failures);
        Assert.Equal((long)bools.Count, loop.Nobs);

        // The weighted overload must land where the single fits landed.
        var weighted = new RunningStatistics.Beta();
        weighted.Fit(true, 3);
        weighted.Fit(false, 4);
        weighted.Fit(true, 1);
        Assert.Equal(loop.Successes, weighted.Successes);
        Assert.Equal(loop.Failures, weighted.Failures);

        // The collection overload counts in one pass over the whole list.
        var list = new RunningStatistics.Beta();
        list.Fit(bools);
        Assert.Equal(loop.Successes, list.Successes);
        Assert.Equal(loop.Failures, list.Failures);

        // The constructor and the two-count Fit must agree with each other and with the
        // loop. Nobs must equal the sum of the counts, not a value tracked separately.
        var ctor = new RunningStatistics.Beta(successes, failures);
        Assert.Equal(loop.Successes, ctor.Successes);
        Assert.Equal(loop.Failures, ctor.Failures);
        Assert.Equal(ctor.Successes + ctor.Failures, ctor.Nobs);

        // A zero count changes nothing.
        var zero = new RunningStatistics.Beta();
        zero.Fit(true, 0);
        zero.Fit(false, 0);
        Assert.Equal(0, zero.Successes);
        Assert.Equal(0, zero.Failures);
        Assert.Equal(0, zero.Nobs);
    }

    [Fact]
    public void Gate_BetaMergeAssociativityCommutativityExact()
    {
        var a = new RunningStatistics.Beta(3, 1);
        var b = new RunningStatistics.Beta(0, 5);
        var c = new RunningStatistics.Beta(7, 2);

        var g1 = a.Clone();
        g1.Merge(b);
        g1.Merge(c);

        var bc = b.Clone();
        bc.Merge(c);
        var g2 = a.Clone();
        g2.Merge(bc);

        var g3 = c.Clone();
        g3.Merge(b);
        g3.Merge(a);

        // Integer addition is associative and commutative, so the groupings must agree
        // exactly, not just within a tolerance.
        Assert.Equal(g1.Successes, g2.Successes);
        Assert.Equal(g1.Failures, g2.Failures);
        Assert.Equal(g1.Successes, g3.Successes);
        Assert.Equal(g1.Failures, g3.Failures);
        Assert.Equal(10, g1.Successes);
        Assert.Equal(8, g1.Failures);

        var clone = g1.Clone();
        Assert.Equal(g1.Successes, clone.Successes);
        Assert.Equal(g1.Failures, clone.Failures);
        Assert.Equal(g1.Nobs, clone.Nobs);
    }

    [Fact]
    public void Gate_BetaReadoutsMatchRationalReference()
    {
        var successesSet = new long[] { 1, 1, 1, 1000, 12345, 1_000_000 };
        var failuresSet = new long[] { 1, 1000, 1, 1, 67890, 999_983 };
        for (var i = 0; i < successesSet.Length; i++)
        {
            var s = successesSet[i];
            var f = failuresSet[i];
            var beta = new RunningStatistics.Beta();
            beta.Fit(true, s);
            beta.Fit(false, f);

            // The mean is one division of exact integers, so it must sit within a few
            // machine epsilands of the same ratio computed independently.
            var n = (double)(s + f);
            var alpha = (double)s / n;
            Assert.True(PropertyHelpers.Within(alpha, beta.Mean, 4 * PropertyHelpers.Epsilon),
                        PropertyHelpers.Describe($"mean at ({s},{f})", alpha, beta.Mean));

            // The variance reference is written in the normalized form, mean times
            // one-minus-mean over count-plus-one. It equals the production form in
            // exact arithmetic, so a difference is pure rounding and must stay tiny.
            var refVariance = alpha * (1.0 - alpha) / (n + 1.0);
            Assert.True(PropertyHelpers.Within(refVariance, beta.Variance, 1e-12),
                        PropertyHelpers.Describe($"variance at ({s},{f})", refVariance, beta.Variance));

            // The mode exists only with more than one of each. It must be NaN below
            // that and the exact ratio above it.
            if (s > 1 && f > 1)
            {
                var refMode = (double)(s - 1) / (n - 2.0);
                Assert.True(PropertyHelpers.Within(refMode, beta.Mode, 4 * PropertyHelpers.Epsilon),
                            PropertyHelpers.Describe($"mode at ({s},{f})", refMode, beta.Mode));
            }
            else
            {
                Assert.True(double.IsNaN(beta.Mode),
                            $"mode must not exist at ({s},{f}) but reads {beta.Mode}");
            }
        }
    }

    [Fact]
    public void Gate_BetaMedianSymmetricAndCdfInversesQuantile()
    {
        // A Beta with equal counts is symmetric, so its median must sit at one half to
        // within the root finder's own accuracy.
        foreach (var side in new long[] { 1, 2, 3, 101, 5000 })
        {
            var beta = new RunningStatistics.Beta();
            beta.Fit(true, side);
            beta.Fit(false, side);
            Assert.True(PropertyHelpers.WithinAbsolute(0.5, beta.Median, 1e-10),
                        PropertyHelpers.Describe($"symmetric median at ({side},{side})", 0.5, beta.Median));
        }

        // The quantile must undo the CDF, and must not walk backwards as the
        // probability grows. Checked for the uniform case and one skewed case.
        var probs = new double[] { 0.05, 0.1, 0.25, 0.5, 0.75, 0.9, 0.95 };
        var uniforms = new long[] { 1, 3 };
        var skewed = new long[] { 1, 7 };
        for (var pair = 0; pair < uniforms.Length; pair++)
        {
            var beta = new RunningStatistics.Beta(uniforms[pair], skewed[pair]);
            for (var i = 0; i < probs.Length; i++)
            {
                var p = probs[i];
                var q = beta.Quantile(p);
                Assert.True(PropertyHelpers.WithinAbsolute(p, beta.Cdf(q), 1e-9),
                            PropertyHelpers.Describe($"Cdf after Quantile at p {p} for Beta({uniforms[pair]},{skewed[pair]})", p, beta.Cdf(q)));
                if (i > 0)
                {
                    var previous = beta.Quantile(probs[i - 1]);
                    Assert.True(q >= previous,
                                $"Quantile walked backwards between p {probs[i - 1]} and p {p} for Beta({uniforms[pair]},{skewed[pair]}): {previous} then {q}");
                }
            }
        }
    }

    [Fact]
    public void Gate_BetaDegenerateReadoutExistence()
    {
        // With one side empty the mean exists at its end, and the spread-shaped
        // readouts must read NaN.
        var successesOnly = new RunningStatistics.Beta(5, 0);
        Assert.Equal(1.0, successesOnly.Mean);
        Assert.True(double.IsNaN(successesOnly.Median));
        Assert.True(double.IsNaN(successesOnly.Variance));
        Assert.True(double.IsNaN(successesOnly.Mode));

        var failuresOnly = new RunningStatistics.Beta(0, 5);
        Assert.Equal(0.0, failuresOnly.Mean);
        Assert.True(double.IsNaN(failuresOnly.Median));
        Assert.True(double.IsNaN(failuresOnly.Variance));
        Assert.True(double.IsNaN(failuresOnly.Mode));

        // One of each already has a median at one half, but no mode until both counts
        // pass one.
        var oneEach = new RunningStatistics.Beta(1, 1);
        Assert.True(PropertyHelpers.WithinAbsolute(0.5, oneEach.Median, 1e-10),
                    PropertyHelpers.Describe("median at (1,1)", 0.5, oneEach.Median));
        Assert.True(double.IsNaN(oneEach.Mode),
                    $"mode must not exist at (1,1) but reads {oneEach.Mode}");

        var twoEach = new RunningStatistics.Beta(2, 2);
        Assert.Equal(0.5, twoEach.Mode);
        Assert.True(PropertyHelpers.Within(0.05, twoEach.Variance, 1e-12),
                    PropertyHelpers.Describe("variance at (2,2)", 0.05, twoEach.Variance));
    }
}
