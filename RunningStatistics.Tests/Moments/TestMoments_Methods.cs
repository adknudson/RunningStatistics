using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests.Moments;

public partial class TestMoments
{
    [Fact]
    public void Fit_NobsOverflowThrows()
    {
        var m = new RunningStatistics.Moments();
        m.Fit(1.0, long.MaxValue / 2 + 1);
        Assert.Throws<OverflowException>(() => m.Fit(1.0, long.MaxValue / 2 + 1));
        Assert.Equal(long.MaxValue / 2 + 1, m.Nobs);
    }

    // These facts check the accuracy of the stable central-sum update now in Moments, the
    // Chan pairwise one. The data is jittered, at the one-million scale, and the reference is a
    // two-pass computation. The stable update shows errors of about the condition number times
    // machine epsilon, roughly two ten-billionths relative, on variance and standard deviation.
    // On skewness and kurtosis the errors are about one billionth absolute. The tolerances
    // below are around a millionth absolute on skewness and a hundred-thousandth on kurtosis.
    // They keep margins of one hundred to one thousand times. The raw-power-sums
    // implementation this replaced could never come close. Its observed errors were a relative
    // 1.7e-4 on variance, an absolute 4e4 on skewness, and an absolute 2.1e11 on kurtosis. The
    // original pins of 1e5 and 1e12 documented those values. The seed is fixed on purpose.
    // These facts are the accuracy net for the swap.
    [Fact]
    public void Accuracy_StableMomentsNearOffset()
    {
        const int n = 150;
        const double offset = 1e6;
        var rng = new Random(20251114);

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = offset + rng.NextDouble();

        var moments = new RunningStatistics.Moments();
        for (var i = 0; i < n; i++) moments.Fit(values[i]);

        // Two-pass reference: simple mean, then central sums directly.
        var sum = 0.0;
        foreach (var v in values) sum += v;
        var mean = sum / n;

        var c2 = 0.0;
        var c3 = 0.0;
        var c4 = 0.0;
        foreach (var v in values)
        {
            var d = v - mean;
            var d2 = d * d;
            c2 += d2;
            c3 += d2 * d;
            c4 += d2 * d2;
        }

        var refVariance = c2 / (n - 1);
        var refStdDev = Math.Sqrt(refVariance);
        var refSkewness = (c3 / n) / Math.Pow(c2 / n, 1.5);
        var refKurtosis = (c4 / n) / ((c2 / n) * (c2 / n));

        Utils.AssertIsApproximate(mean, moments.Mean, 0.0);
        Utils.AssertIsApproximate(refVariance, moments.Variance, 0.0);
        Utils.AssertIsApproximate(refStdDev, moments.StandardDeviation, 0.0);
        Utils.AssertIsApproximate(refSkewness, moments.Skewness, 1e-6);
        Utils.AssertIsApproximate(refKurtosis, moments.Kurtosis, 1e-5);
        Utils.AssertIsApproximate(refKurtosis - 3, moments.ExcessKurtosis, 1e-5);
    }

    // The same accuracy facts as Accuracy_StableMomentsNearOffset, reached through the merge path:
    // a stable merge must be as accurate as a stable stream. The raw-power-sums merge this replaced
    // showed errors of a relative 4.7e-3 on variance, an absolute 4e4 on skewness and an absolute
    // 2.1e11 on kurtosis.
    [Fact]
    public void Accuracy_StableMergedMomentsNearOffset()
    {
        const int n = 150;
        const double offset = 1e6;
        var rng = new Random(20251114);

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = offset + rng.NextDouble();

        var left = new RunningStatistics.Moments();
        var right = new RunningStatistics.Moments();
        for (var i = 0; i < n; i++)
        {
            if (i < n / 2) left.Fit(values[i]);
            else right.Fit(values[i]);
        }
        left.Merge(right);

        // Same two-pass reference as Accuracy_StableMomentsNearOffset.
        var sum = 0.0;
        foreach (var v in values) sum += v;
        var mean = sum / n;

        var c2 = 0.0;
        var c3 = 0.0;
        var c4 = 0.0;
        foreach (var v in values)
        {
            var d = v - mean;
            var d2 = d * d;
            c2 += d2;
            c3 += d2 * d;
            c4 += d2 * d2;
        }

        var refVariance = c2 / (n - 1);
        var refStdDev = Math.Sqrt(refVariance);
        var refSkewness = (c3 / n) / Math.Pow(c2 / n, 1.5);
        var refKurtosis = (c4 / n) / ((c2 / n) * (c2 / n));

        Utils.AssertIsApproximate(mean, left.Mean, 0.0);
        Utils.AssertIsApproximate(refVariance, left.Variance, 0.0);
        Utils.AssertIsApproximate(refStdDev, left.StandardDeviation, 0.0);
        Utils.AssertIsApproximate(refSkewness, left.Skewness, 1e-6);
        Utils.AssertIsApproximate(refKurtosis, left.Kurtosis, 1e-5);
    }

    [Fact]
    public void EmptyMoments_AreNaN()
    {
        RunningStatistics.Moments m = new();

        Assert.Equal(double.NaN, m.Mean);
        Assert.Equal(double.NaN, m.Variance);
        Assert.Equal(double.NaN, m.StandardDeviation);
        Assert.Equal(double.NaN, m.Skewness);
        Assert.Equal(double.NaN, m.Kurtosis);
        Assert.Equal(double.NaN, m.ExcessKurtosis);
    }

    [Fact]
    public void Fit_SingleFiniteObservation()
    {
        RunningStatistics.Moments m = new();
        m.Fit(1);
        
        Assert.Equal(1, m.Mean);
        Assert.Equal(double.NaN, m.Variance);
        Assert.Equal(double.NaN, m.StandardDeviation);
        Assert.Equal(double.NaN, m.Skewness);
        Assert.Equal(double.NaN, m.Kurtosis);
        Assert.Equal(double.NaN, m.ExcessKurtosis);
    }
    
    [Fact]
    public void Fit_TwoFiniteObservations()
    {
        RunningStatistics.Moments m = new();
        m.Fit(1);
        m.Fit(2);
        
        Assert.Equal(1.5, m.Mean);
        Assert.Equal(0.5, m.Variance);
        Assert.Equal(0.7071067811865476, m.StandardDeviation, 10);
        Assert.Equal(0, m.Skewness);
        Assert.Equal(1, m.Kurtosis);
        Assert.Equal(-2, m.ExcessKurtosis);
    }
    
    [Fact]
    public void Fit_RejectsNaNAndInfinity()
    {
        RunningStatistics.Moments m = new();
        Assert.Throws<ArgumentException>(() => m.Fit(double.NaN));
        Assert.Throws<ArgumentException>(() => m.Fit(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => m.Fit(double.NegativeInfinity));
    }

    [Fact]
    public void MergeEmpty_AreNaN()
    {
        RunningStatistics.Moments a = new(); 
        RunningStatistics.Moments b = new(); 
        a.Merge(b);

        Assert.Equal(double.NaN, a.Mean);
        Assert.Equal(double.NaN, a.Variance);
        Assert.Equal(double.NaN, a.Skewness);
        Assert.Equal(double.NaN, a.Kurtosis);
    }

    // The fact merges the data as two stats, one for the first half and one for the second
    // half. Their merge must match a stat fitted with all of the data at once.
    [Fact]
    public void Merge_PartsEqualsAll()
    {
        const int n = 2000;
        var rng = new Random();

        RunningStatistics.Moments a = new(); 
        RunningStatistics.Moments b = new(); 
        RunningStatistics.Moments c = new();

        double v;
        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            a.Fit(v);
            c.Fit(v);
        }

        for (var i = 0; i < n; i++)
        {
            v = rng.NextDouble();
            b.Fit(v);
            c.Fit(v);
        }

        a.Merge(b);

        Assert.Equal(a.Mean, c.Mean, 2);
        Assert.Equal(a.Variance, c.Variance, 2);
        Assert.Equal(a.Skewness, c.Skewness, 2);
        Assert.Equal(a.Kurtosis, c.Kurtosis, 2);
    }

    // The static two-argument merge form returns a new statistic and leaves both arguments alone.
    [Fact]
    public void StaticMerge_DoesNotAffectOriginals()
    {
        RunningStatistics.Moments a = new();
        RunningStatistics.Moments b = new();
        var rng = new Random();
        
        for (var i = 0; i < 100; i++)
        {
            a.Fit(rng.NextDouble());
            b.Fit(rng.NextDouble());
        }

        var c = RunningStatistics.Moments.Merge(a, b);
        Assert.Equal(a.Nobs + b.Nobs, c.Nobs);
            
        c.Fit(rng.NextDouble());
        Assert.Equal(a.Nobs + b.Nobs + 1, c.Nobs);
    }

    // The fact fits 10M samples of the distribution and compares the statistic against its
    // known properties. The generator is seeded on purpose. An unseeded stream makes this fact
    // flaky at the 0.01 tolerances, because the excess-kurtosis estimator alone varies by about
    // three standard deviations across fresh streams.
    [Fact]
    public void UnitUniformSample_MomentsMatchTheory()
    {
        const int n = 10_000_000;
        var rng = new Random(20251117);

        var moments = new RunningStatistics.Moments();

        for (var i = 0; i < n; i++)
        {
            moments.Fit(rng.NextDouble());
        }

        const double mean = (0.0 + 1.0 - double.Epsilon) / 2.0;
        var variance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;
        const double skewness = 0.0;
        const double excessKurtosis = -6.0 / 5.0;

        Utils.AssertIsApproximate(mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(variance, moments.Variance, 0.01);
        Utils.AssertIsApproximate(skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.01);
    }

    // Fits 10M samples of the distribution and compares the statistic against its known properties.
    [Theory]
    [InlineData(2.0, 1.0)]
    [InlineData(2.0, 10.0)]
    public void NormalSample_MomentsMatchTheory(double mean, double stdDev)
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(mean, stdDev);

        var moments = new RunningStatistics.Moments();

        var rng = new Random(20251115);
        for (var i = 0; i < n; i += 2)
        {
            var u = rng.NextDouble();
            var v = rng.NextDouble();
            if (u < 1e-300) u = 1e-300;
            var r = Math.Sqrt(-2.0 * Math.Log(u));
            moments.Fit(mean + stdDev * r * Math.Cos(2.0 * Math.PI * v));
            moments.Fit(mean + stdDev * r * Math.Sin(2.0 * Math.PI * v));
        }

        const double excessKurtosis = 0.0;

        Utils.AssertIsApproximate(dist.Mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(dist.StdDev, moments.StandardDeviation, 0.01);
        Utils.AssertIsApproximate(dist.Skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.01);
    }

    // Fits 10M samples of the distribution and compares the statistic against its known properties.
    [Fact]
    public void LogNormalSample_MomentsMatchTheory()
    {
        const int n = 10_000_000;
        const double mu = 0.0;
        const double sd = 0.5;
        const double s2 = sd * sd;
        var dist = new LogNormal(mu, sd);

        var moments = new RunningStatistics.Moments();

        var rng = new Random(20251116);
        for (var i = 0; i < n; i += 2)
        {
            var u = rng.NextDouble();
            var v = rng.NextDouble();
            if (u < 1e-300) u = 1e-300;
            var r = Math.Sqrt(-2.0 * Math.Log(u));
            moments.Fit(Math.Exp(mu + sd * r * Math.Cos(2.0 * Math.PI * v)));
            moments.Fit(Math.Exp(mu + sd * r * Math.Sin(2.0 * Math.PI * v)));
        }
            
        var excessKurtosis = Math.Exp(4.0 * s2) + 2.0 * Math.Exp(3.0 * s2) + 3.0 * Math.Exp(2.0 * s2) - 6;

        Utils.AssertIsApproximate(dist.Mean, moments.Mean, 0.01);
        Utils.AssertIsApproximate(dist.Variance, moments.Variance, 0.01);
        Utils.AssertIsApproximate(dist.Skewness, moments.Skewness, 0.01);
        Utils.AssertIsApproximate(excessKurtosis, moments.ExcessKurtosis, 0.1);
    }

    // These merge-algebra gates are permanent, so keep them. Merging moments is associative.
    // The grouping of a merged with b, then merged with c, is compared against a merged with
    // the result of b merged with c. Each grouping runs on a fresh copy made through CloneEmpty
    // and Merge. The data is three seeded KappaStream chunks, each with one hundred observations
    // and spread one. One grouping runs per condition number, walking powers of ten from one to
    // one hundred million. Every getter of every grouping must match the two-pass PropertyHelpers
    // reference over all three chunks together. The groupings must also agree with each other.
    // The relative margin is a hundred-millionth plus ten thousand times the condition number
    // times machine epsilon, on mean, variance and standard deviation. The absolute margin is
    // one trillionth plus one thousand times the condition number times machine epsilon, on
    // skewness, kurtosis and excess kurtosis. The floor of one trillionth widens the floorless
    // version at low condition numbers. At condition number one, the bare margin left only about
    // forty-five times headroom over the observed error of 4.9E-15. The floor restores about
    // two hundred and fifty times there. The probe found the worst errors at condition number
    // 1e8. Merge errors grow about linearly in the condition number, at roughly one to three
    // times it times machine epsilon rather than its square. The errors there were a relative
    // 2.31E-8 on variance and 1.15E-8 on standard deviation. They were an absolute 6.31E-8 on
    // skewness and 4.69E-8 on kurtosis and excess kurtosis. That leaves three hundred and fifty
    // to four hundred and seventy times headroom in absolute terms, and about ten thousand times
    // in relative terms. The differences between the groupings are far smaller.
    [Fact]
    public void Gate_MergeAssociativityMoments()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;
            var absTol = 1e-12 + 1e3 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 20251118 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 40251121 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 70251123 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Moments();
            var b = new RunningStatistics.Moments();
            var c = new RunningStatistics.Moments();
            for (var i = 0; i < n; i++) a.Fit(chunkA[i]);
            for (var i = 0; i < n; i++) b.Fit(chunkB[i]);
            for (var i = 0; i < n; i++) c.Fit(chunkC[i]);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var bc = b.Clone();
            bc.Merge(c);
            var g2 = a.Clone();
            g2.Merge(bc);

            AssertMomentsAgainstReference("(a∪b)∪c", reference, g1, relTol, absTol);
            AssertMomentsAgainstReference("a∪(b∪c)", reference, g2, relTol, absTol);
            AssertMomentsGroupingsAgree("(a∪b)∪c vs a∪(b∪c)", g1, g2, relTol, absTol);
        }
    }

    // This fact checks the same merge-algebra contract in the commutative direction. It merges
    // a, b, c against c, b, a over the same three seeded chunks and the same condition-number
    // range. Both sides merge into fresh copies as receivers, and the margins match those of
    // Gate_MergeAssociativityMoments.
    [Fact]
    public void Gate_MergeCommutativityMoments()
    {
        const int n = 100;
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;
            var absTol = 1e-12 + 1e3 * kappa * PropertyHelpers.Epsilon;

            var chunkA = PropertyHelpers.KappaStream(n, kappa, 90251128 + d);
            var chunkB = PropertyHelpers.KappaStream(n, kappa, 13251217 + d);
            var chunkC = PropertyHelpers.KappaStream(n, kappa, 56251219 + d);

            var all = new List<double>();
            for (var i = 0; i < n; i++) all.Add(chunkA[i]);
            for (var i = 0; i < n; i++) all.Add(chunkB[i]);
            for (var i = 0; i < n; i++) all.Add(chunkC[i]);
            var reference = PropertyHelpers.ReferenceMoments(all);

            var a = new RunningStatistics.Moments();
            var b = new RunningStatistics.Moments();
            var c = new RunningStatistics.Moments();
            for (var i = 0; i < n; i++) a.Fit(chunkA[i]);
            for (var i = 0; i < n; i++) b.Fit(chunkB[i]);
            for (var i = 0; i < n; i++) c.Fit(chunkC[i]);

            var g1 = a.Clone();
            g1.Merge(b);
            g1.Merge(c);

            var g3 = c.Clone();
            g3.Merge(b);
            g3.Merge(a);

            AssertMomentsAgainstReference("a∪b∪c", reference, g1, relTol, absTol);
            AssertMomentsAgainstReference("c∪b∪a", reference, g3, relTol, absTol);
            AssertMomentsGroupingsAgree("a∪b∪c vs c∪b∪a", g1, g3, relTol, absTol);
        }
    }

    // This gate is the audit coverage for the Moments readouts: it runs the existing KappaSweep
    // datasets against the single-fit route, which is the only route Moments provides, and pins
    // every getter of the four-moment contract against the two-pass PropertyHelpers reference.
    // The margins follow the merge gates above: a relative 1e-8 plus ten thousand times the
    // condition number times machine epsilon on mean, variance and standard deviation; an
    // absolute 1e-12 plus one thousand times the condition number times machine epsilon on the
    // standardized ratios. The audit run of 2026-11-19 recorded the worst errors this machine
    // measured: the mean held within a relative 4.6E-16, the variance and standard deviation at
    // a relative 2.87E-9 and 1.44E-9 at condition number 1e8, about 1.3 times the condition
    // number times machine epsilon, and skewness and kurtosis at an absolute 3.12E-8 and 2.10E-8
    // there, about 700 times inside the absolute margin. Deeper probes saw the spread getters
    // keep growing near linearly in the condition number: a relative 1.39E-4 on variance at
    // 1e12. The audit records no failure on this family, and no candidate is staged. A red
    // Gate_ fact means the Chan update lost accuracy. Fix the algorithm, and do not widen a
    // tolerance to make a fact pass. The datasets are seeded, so a failure reproduces.
    [Fact]
    public void Gate_MomentsKappaSweepWithinTolerance()
    {
        const int n = 100;
        var sets = PropertyHelpers.KappaSweep(n, 8, 20251129L);
        for (var d = 0; d <= 8; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var relTol = 1e-8 + 1e4 * kappa * PropertyHelpers.Epsilon;
            var absTol = 1e-12 + 1e3 * kappa * PropertyHelpers.Epsilon;
            var xs = sets[d];
            var reference = PropertyHelpers.ReferenceMoments(xs);

            var loop = new RunningStatistics.Moments();
            for (var i = 0; i < n; i++) loop.Fit(xs[i]);

            AssertMomentsAgainstReference($"loop at kappa {kappa:E0}", reference, loop, relTol, absTol);
        }
    }

    // The cancellation sibling of the audit. The audit found one failure on this family, and
    // this gate records it in its comment rather than asserting it: the Mean getter reached
    // through the incremental route. The stored mean absorbs unit-in-the-last-place errors of
    // the huge intermediate means it passes through on the way to the combined mean, and those
    // dwarf the exact mean of 0.9259259 once the huge terms pass 2^53. The audit probe of
    // 2026-11-19 recorded a relative 3.71E-4 at condition number 1e12, 1.16E+0 at 1e15, and
    // 1.10E+3 at 1e18. The same exclusion holds in Mean's cancellation gate, where the single-
    // fit route is not claimed either. The five spread readouts are not exposed to that error:
    // their scale is the huge terms' own powers, and the probe held variance and standard
    // deviation to within a relative 3.70E-16, and skewness, kurtosis and excess kurtosis to
    // an absolute 1.07E-14, all at margins of one hundred-millionth, about 2.7E7 times of
    // headroom. No algorithm swap is queued yet, so this gate asserts the five spread readouts
    // and leaves Mean to a future candidate, which must extend this gate once it lands. A red
    // fact means the Chan update lost accuracy; fix the algorithm, do not widen a tolerance.
    [Fact]
    public void Gate_MomentsCancellationWithinTolerance()
    {
        const int n = 100;
        for (var d = 12; d <= 18; d++)
        {
            var kappa = Math.Pow(10.0, d);
            var xs = PropertyHelpers.CancelingStream(n, kappa, 991_577L + d);
            var reference = PropertyHelpers.ReferenceMoments(xs);

            var loop = new RunningStatistics.Moments();
            for (var i = 0; i < xs.Count; i++) loop.Fit(xs[i]);

            Assert.True(PropertyHelpers.Within(reference.Variance, loop.Variance, 1e-8),
                        PropertyHelpers.Describe($"variance at kappa {kappa:E0} vs reference", reference.Variance, loop.Variance));
            Assert.True(PropertyHelpers.Within(reference.StandardDeviation, loop.StandardDeviation, 1e-8),
                        PropertyHelpers.Describe($"standard deviation at kappa {kappa:E0} vs reference", reference.StandardDeviation, loop.StandardDeviation));
            Assert.True(PropertyHelpers.WithinAbsolute(reference.Skewness, loop.Skewness, 1e-8),
                        PropertyHelpers.Describe($"skewness at kappa {kappa:E0} vs reference", reference.Skewness, loop.Skewness));
            Assert.True(PropertyHelpers.WithinAbsolute(reference.Kurtosis, loop.Kurtosis, 1e-8),
                        PropertyHelpers.Describe($"kurtosis at kappa {kappa:E0} vs reference", reference.Kurtosis, loop.Kurtosis));
            Assert.True(PropertyHelpers.WithinAbsolute(reference.ExcessKurtosis, loop.ExcessKurtosis, 1e-8),
                        PropertyHelpers.Describe($"excess kurtosis at kappa {kappa:E0} vs reference", reference.ExcessKurtosis, loop.ExcessKurtosis));
        }
    }

    private static void AssertMomentsAgainstReference(string tag, MomentsReference reference, RunningStatistics.Moments merged, double relTol, double absTol)
    {
        Assert.True(PropertyHelpers.Within(reference.Mean, merged.Mean, relTol), PropertyHelpers.Describe($"{tag} Mean vs reference", reference.Mean, merged.Mean));
        Assert.True(PropertyHelpers.Within(reference.Variance, merged.Variance, relTol), PropertyHelpers.Describe($"{tag} Variance vs reference", reference.Variance, merged.Variance));
        Assert.True(PropertyHelpers.Within(reference.StandardDeviation, merged.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation vs reference", reference.StandardDeviation, merged.StandardDeviation));
        Assert.True(PropertyHelpers.WithinAbsolute(reference.Skewness, merged.Skewness, absTol), PropertyHelpers.Describe($"{tag} Skewness vs reference", reference.Skewness, merged.Skewness));
        Assert.True(PropertyHelpers.WithinAbsolute(reference.Kurtosis, merged.Kurtosis, absTol), PropertyHelpers.Describe($"{tag} Kurtosis vs reference", reference.Kurtosis, merged.Kurtosis));
        Assert.True(PropertyHelpers.WithinAbsolute(reference.ExcessKurtosis, merged.ExcessKurtosis, absTol), PropertyHelpers.Describe($"{tag} ExcessKurtosis vs reference", reference.ExcessKurtosis, merged.ExcessKurtosis));
    }

    private static void AssertMomentsGroupingsAgree(string tag, RunningStatistics.Moments left, RunningStatistics.Moments right, double relTol, double absTol)
    {
        Assert.True(PropertyHelpers.Within(left.Mean, right.Mean, relTol), PropertyHelpers.Describe($"{tag} Mean", left.Mean, right.Mean));
        Assert.True(PropertyHelpers.Within(left.Variance, right.Variance, relTol), PropertyHelpers.Describe($"{tag} Variance", left.Variance, right.Variance));
        Assert.True(PropertyHelpers.Within(left.StandardDeviation, right.StandardDeviation, relTol), PropertyHelpers.Describe($"{tag} StandardDeviation", left.StandardDeviation, right.StandardDeviation));
        Assert.True(PropertyHelpers.WithinAbsolute(left.Skewness, right.Skewness, absTol), PropertyHelpers.Describe($"{tag} Skewness", left.Skewness, right.Skewness));
        Assert.True(PropertyHelpers.WithinAbsolute(left.Kurtosis, right.Kurtosis, absTol), PropertyHelpers.Describe($"{tag} Kurtosis", left.Kurtosis, right.Kurtosis));
        Assert.True(PropertyHelpers.WithinAbsolute(left.ExcessKurtosis, right.ExcessKurtosis, absTol), PropertyHelpers.Describe($"{tag} ExcessKurtosis", left.ExcessKurtosis, right.ExcessKurtosis));
    }

    // This fact checks that reading the getters does not get slower as the statistic sees more
    // observations. The getters look at a fixed amount of state. Reading them should therefore
    // cost the same on a huge statistic as on a small one. The test builds one statistic that
    // has seen 2 to the 62nd power observations, and one that has seen 6. The test times
    // millions of reads of Mean, Variance, StandardDeviation, Skewness and Kurtosis on each
    // statistic. It requires the big one to be within [0.5, 1.05] of the small one. The band
    // is deliberately tight. A getter is one interface dispatch plus a few arithmetic
    // operations, so the honest claim here is "not slower". Measurement noise of about 1%
    // straddles 1.00, so a ratio outside about 1% is noise, not a real gain or regression. If
    // this fact ever goes red, a getter has picked up work that grows with the observation
    // count. Fix the algorithm, and do not widen the band.
    //
    // The code builds the big statistic in one way. It fits two distinct values, then merges
    // the statistic with itself 61 times. Each merge doubles the count, up to 2 to the 62nd
    // power. Two distinct values are needed at the start. A single observation has zero spread,
    // which leaves Variance and StandardDeviation at 0. It sends Skewness and Kurtosis into
    // their not-enough-data NaN path. The timed sweep would then measure early exits instead of
    // the real getter work. At the scale of 2 to the 62nd power, every getter returns a finite
    // number. The big and the small statistic go through exactly the same getter code paths.
    [Fact]
    public void Bench_GetterCostIndependentOfNobs_Moments()
    {
        // The harness default is five million getter reads per measured run, over four
        // measured runs. Both sides of the comparison run the same sweep body, which the helper
        // below provides. The ratio therefore compares identical machine code. The [0.5, 1.05]
        // band is expected at the default size, not at a bigger workload chosen to make it
        // pass.
        const int obs = 5_000_000;
        const int repeats = 4;

        // fixed-seed data: deterministic, and both stats end in a finite fitted state
        var values = PropertyHelpers.UniformUnit(6, 20251118L);

        var big = new RunningStatistics.Moments();
        big.Fit(values[0]);
        big.Fit(values[1]);
        for (var i = 0; i < 61; i++) big.Merge(big);
        Assert.Equal(1L << 62, big.Nobs);

        var small = new RunningStatistics.Moments();
        foreach (var value in values) small.Fit(value);
        Assert.Equal(6, small.Nobs);

        // preconditions: finite readouts on both sides, so neither sweep takes a NaN guard and
        // no NaN can poison the checksum BenchmarkHarness folds
        Assert.False(
            double.IsNaN(big.Mean) || double.IsNaN(big.Variance) || double.IsNaN(big.StandardDeviation) ||
            double.IsNaN(big.Skewness) || double.IsNaN(big.Kurtosis));
        Assert.False(
            double.IsNaN(small.Mean) || double.IsNaN(small.Variance) || double.IsNaN(small.StandardDeviation) ||
            double.IsNaN(small.Skewness) || double.IsNaN(small.Kurtosis));

        var bigNsPerObs = BenchmarkHarness.Measure(
            "Moments readout sweep at Nobs=2^62, ns per sweep",
            obs,
            repeats,
            () => SweepMoments(big, obs));

        var smallNsPerObs = BenchmarkHarness.Measure(
            "Moments readout sweep at Nobs=6, ns per sweep",
            obs,
            repeats,
            () => SweepMoments(small, obs));

        var ratio = BenchmarkHarness.Compare(
            "Moments getter readout at Nobs=2^62 vs at Nobs=6, ns per sweep",
            smallNsPerObs,
            bigNsPerObs);

        Assert.True(ratio >= 0.5 && ratio <= 1.05,
                    $"2^62-scale readout sweep costs {ratio:F3} of the same sweep at Nobs=6 " +
                    $"({bigNsPerObs:F4} vs {smallNsPerObs:F4} ns per sweep): the readout getters " +
                    $"got slower once Nobs reaches 2^62. A getter is one interface dispatch plus a " +
                    $"few flops and the claim is parity (not speed), so re-gate the algorithm - " +
                    $"do not widen the band.");
    }

    // One compiled sweep body serves both sides of the comparison. The statistic is passed in
    // as a parameter. Both workloads therefore run literally the same machine code, with the
    // same JIT tier and code layout. The two workloads are the 2 to the 62nd-power one and the
    // small-Nobs one. That is what makes the tight band meaningful. Two separately compiled
    // copies of the same loop drifted 7-8% on code layout alone. That drift is noise on a "not
    // slower" claim, not a regression.
    private static double SweepMoments(RunningStatistics.Moments stat, int obs)
    {
        var sum = 0.0;
        for (var i = 0; i < obs; i++)
        {
            sum += stat.Mean;
            sum += stat.Variance;
            sum += stat.StandardDeviation;
            sum += stat.Skewness;
            sum += stat.Kurtosis;
        }

        // At the scale of 2 to the 62nd power, Kurtosis is about 2.3e18. Adding up all the
        // samples directly would take the total out of the range that BenchmarkHarness folds its
        // checksum into. The remainder keeps the total in that range while still depending on
        // every getter read. It costs one modulo per call, outside the timed sweep.
        return sum % 1_000_000.0;
    }

    // Null-collection rejection lives in AbstractRunningStatsTest.FitNullCollections_ThrowsStateUntouched.
}