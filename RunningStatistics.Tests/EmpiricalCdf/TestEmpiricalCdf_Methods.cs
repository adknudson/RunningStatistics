using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using Xunit;

namespace RunningStatistics.Tests.EmpiricalCdf;

public partial class TestEmpiricalCdf
{
    [Fact]
    public void Constructor_ThrowsOnInvalidBins()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.EmpiricalCdf(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.EmpiricalCdf(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunningStatistics.EmpiricalCdf(1));
    }
    
    [Fact]
    public void Fit_RejectsNaN()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Assert.Throws<ArgumentException>(() => cdf.Fit(double.NaN));
    }
    
    [Fact]
    public void Fit_RejectsInfinity()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Assert.Throws<ArgumentException>(() => cdf.Fit(double.PositiveInfinity));
    }
    
    [Fact]
    public void Quantile_ThrowsWhenFewerObservationsThanBins()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        var rng = new Random();

        for (var i = 0; i < 10; i++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(0.25));
        Assert.Throws<InvalidOperationException>(() => cdf.Median);
    }

    [Fact]
    public void Quantile_ExactlyNumBinsSucceeds()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        var rng = new Random();
        
        for (var i = 0; i < 20; i++)
        {
            cdf.Fit(rng.NextDouble());
        }
        
        Assert.Equal(cdf.Min, cdf.Quantile(0));
        Assert.Equal(cdf.Max, cdf.Quantile(1));
        
        // This shouldn't throw an exception
        cdf.Quantile(0.25);
    }

    [Fact]
    public void Merge_ThrowsOnDifferentNumBins()
    {
        RunningStatistics.EmpiricalCdf a = new(10);
        RunningStatistics.EmpiricalCdf b = new(20);
        Assert.Throws<Exception>(() => a.Merge(b));
    }

    [Fact]
    public void Reset_ClearsObservations()
    {
        RunningStatistics.EmpiricalCdf cdf = new(10);
        Random rng = new();
            
        for (var i = 0; i < 1000; i++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Equal(1000, cdf.Nobs);
        cdf.Reset();
        Assert.Equal(0, cdf.Nobs);
    }

    [Fact]
    public void Merge_WithEmptyOtherEqualsPopulated()
    {
        RunningStatistics.EmpiricalCdf a = new(20), b = new(20);
        Random rng = new();

        for (var i = 0; i < 1000; i++)
        {
            b.Fit(rng.NextDouble());
        }

        var aMergeB = RunningStatistics.EmpiricalCdf.Merge(a, b);
        var bMergeA = RunningStatistics.EmpiricalCdf.Merge(b, a);
            
        Assert.Equal(b.Min, aMergeB.Min);
        Assert.Equal(b.Max, aMergeB.Max);
        Assert.Equal(b.Quantile(0.25), aMergeB.Quantile(0.25));
        Assert.Equal(b.Quantile(0.75), aMergeB.Quantile(0.75));

            
        Assert.Equal(b.Min, bMergeA.Min);
        Assert.Equal(b.Max, bMergeA.Max);
        Assert.Equal(b.Quantile(0.25), bMergeA.Quantile(0.25));
        Assert.Equal(b.Quantile(0.75), bMergeA.Quantile(0.75));
    }

    // Merging two stats fitted with the first and second halves of the same data must match
    // a stat fitted with all of the data at once.
    [Fact]
    public void Merge_PartsEqualsAll()
    {
        const int n = 50_000;
        Random rng = new();

        RunningStatistics.EmpiricalCdf a = new();
        RunningStatistics.EmpiricalCdf b = new();
        RunningStatistics.EmpiricalCdf c = new();

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

        Assert.Equal(a.Quantile(0.1), c.Quantile(0.1), 2);
        Assert.Equal(a.Median, c.Median, 2);
    }

    [Fact]
    public void Merge_NoPhantomZeroBias()
    {
        const int bins = 10;
        Random rng = new(1811);

        // Scenario 1: the merge source has only an unflushed partial buffer (9 < bins
        // observations), so its Values slots are all still zero while its Nobs already
        // counts those values. The merge must not weight those phantom zeros.
        RunningStatistics.EmpiricalCdf a = new(bins);
        double[] values = new double[109];
        var count = 0;
        for (var i = 0; i < 100; i++)
        {
            var v = 50.0 + 10.0 * rng.NextDouble();
            a.Fit(v);
            values[count++] = v;
        }

        RunningStatistics.EmpiricalCdf partial = new(bins);
        for (var i = 0; i < 9; i++)
        {
            var v = 50.0 + 10.0 * rng.NextDouble();
            partial.Fit(v);
            values[count++] = v;
        }

        Assert.Equal(100, a.Nobs);
        Assert.Equal(9, partial.Nobs);

        a.Merge(partial);

        Assert.Equal(109, a.Nobs);
        Assert.Equal(9, partial.Nobs);

        // Every observation lies in [50, 60), so an honest merge's quantiles must match
        // the interpolated order statistics of the full combined sample. The buggy merge
        // instead blends the source's all-zero Values, pulling quantiles toward 0.
        foreach (var p in new[] { 0.25, 0.5, 0.75 })
        {
            Utils.AssertIsApproximate(HonestQuantile(values, count, p), a.Quantile(p), 1.0);
        }

        // Scenario 2: the merge source has one flushed block plus an unflushed tail of 5,
        // and all values share one support — the unflushed tail must be replayed into the
        // merged stat without corrupting either side's state.
        RunningStatistics.EmpiricalCdf a2 = new(bins);
        double[] values2 = new double[115];
        var count2 = 0;
        for (var i = 0; i < 100; i++)
        {
            var v = 50.0 + 10.0 * rng.NextDouble();
            a2.Fit(v);
            values2[count2++] = v;
        }

        RunningStatistics.EmpiricalCdf tail = new(bins);
        for (var i = 0; i < 15; i++)
        {
            var v = 50.0 + 10.0 * rng.NextDouble();
            tail.Fit(v);
            values2[count2++] = v;
        }

        var tailQuantileBefore = tail.Quantile(0.5);

        Assert.Equal(100, a2.Nobs);
        Assert.Equal(15, tail.Nobs);

        a2.Merge(tail);

        Assert.Equal(115, a2.Nobs);
        Assert.Equal(15, tail.Nobs);
        Assert.Equal(tailQuantileBefore, tail.Quantile(0.5));

        foreach (var p in new[] { 0.25, 0.5, 0.75 })
        {
            Utils.AssertIsApproximate(HonestQuantile(values2, count2, p), a2.Quantile(p), 1.0);
        }
    }

    private static double HonestQuantile(double[] values, int count, double p)
    {
        var sorted = new double[count];
        for (var i = 0; i < count; i++)
        {
            sorted[i] = values[i];
        }

        Array.Sort(sorted);

        var h = (count - 1) * p;
        var lo = (int)Math.Floor(h);
        var hi = Math.Min(lo + 1, count - 1);
        return sorted[lo] + (h - lo) * (sorted[hi] - sorted[lo]);
    }

    [Fact]
    public void Cdf_EdgeCases()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Random rng = new();
        
        for (var _ = 0; _ < 1000; _++)
        {
            cdf.Fit(rng.NextDouble());
        }
        
        Assert.Equal(0, cdf.Cdf(cdf.Min - 1));
        Assert.Equal(0, cdf.Cdf(cdf.Min));
        Assert.Equal(1, cdf.Cdf(cdf.Max));
        Assert.Equal(1, cdf.Cdf(cdf.Max + 1));
    }
    
    [Fact]
    public void Quantile_EdgeCases()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Random rng = new();
        
        for (var _ = 0; _ < 1000; _++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => cdf.Quantile(-0.1));
        Assert.Equal(cdf.Min, cdf.Quantile(0));
        Assert.Equal(cdf.Max, cdf.Quantile(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => cdf.Quantile(1.1));
    }

    [Fact]
    public void QuantileAndCdf_RejectNaN()
    {
        RunningStatistics.EmpiricalCdf cdf = new(20);
        Random rng = new();
        
        for (var _ = 0; _ < 1000; _++)
        {
            cdf.Fit(rng.NextDouble());
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => cdf.Quantile(double.NaN));
        Assert.Throws<ArgumentException>(() => cdf.Cdf(double.NaN));
    }

    // Fits 10M samples of the distribution and compares Cdf and Quantile against its known values.
    [Fact]
    public void UnitUniformSample_CdfAndQuantileMatch()
    {
        const int n = 10_000_000;

        var dist = new ContinuousUniform();
        RunningStatistics.EmpiricalCdf cdf = new();

        for (var i = 0; i < n; i++)
        {
            cdf.Fit(dist.Sample());
        }

        var p = 0.0;
        while (p <= 1.0)
        {
            Utils.AssertIsApproximate(dist.CumulativeDistribution(p), cdf.Cdf(p), 0.005);
            Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(p), cdf.Quantile(p), 0.005);
            p += 0.001;
        }
    }

    // Fits 10M samples of the distribution and compares Cdf and Quantile against its known values.
    [Fact]
    public void StandardNormalSample_CdfAndQuantileMatch()
    {
        const int n = 10_000_000;
        var dist = new MathNet.Numerics.Distributions.Normal(0, 1);

        RunningStatistics.EmpiricalCdf cdf = new();

        for (var i = 0; i < n; i++)
        {
            cdf.Fit(dist.Sample());
        }
        
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), cdf.Quantile(0.01), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.10), cdf.Quantile(0.10), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.25), cdf.Quantile(0.25), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), cdf.Quantile(0.50), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.75), cdf.Quantile(0.75), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.90), cdf.Quantile(0.90), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), cdf.Quantile(0.99), 0.25);
        
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-2), cdf.Cdf(-2), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(-1), cdf.Cdf(-1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 0), cdf.Cdf( 0), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 1), cdf.Cdf( 1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution( 2), cdf.Cdf( 2), 0.25);
    }

    // Fits 10M samples of the distribution and compares Cdf and Quantile against its known values.
    [Fact]
    public void LogNormalSample_CdfAndQuantileMatch()
    {
        const int n = 10_000_000;
        var dist = new LogNormal(0.0, 0.1);

        RunningStatistics.EmpiricalCdf cdf = new();

        for (var i = 0; i < n; i++)
        {
            cdf.Fit(dist.Sample());
        }

        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.01), cdf.Quantile(0.01), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.10), cdf.Quantile(0.10), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.25), cdf.Quantile(0.25), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.50), cdf.Quantile(0.50), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.75), cdf.Quantile(0.75), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.90), cdf.Quantile(0.90), 0.25);
        Utils.AssertIsApproximate(dist.InverseCumulativeDistribution(0.99), cdf.Quantile(0.99), 0.25);
            
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.7), cdf.Cdf(0.7), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.8), cdf.Cdf(0.8), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(0.9), cdf.Cdf(0.9), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.0), cdf.Cdf(1.0), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.1), cdf.Cdf(1.1), 0.25);
        Utils.AssertIsApproximate(dist.CumulativeDistribution(1.2), cdf.Cdf(1.2), 0.25);
    }

    [Fact]
    public void Quantile_DuplicateOrderStatsInterpolatesHonestly()
    {
        // Exact duplicates in the sample; Nobs == NumBins, so the single flush (alpha = 1)
        // writes Values = the sorted sample exactly: V = [1,2,2,2] with Min = 1, Max = 2.
        // The interpolation window is [Min, V0..V3, Max] = [1,1,2,2,2,2] and Quantile(p)
        // interpolates at index (NumBins + 1) * p in that window.
        RunningStatistics.EmpiricalCdf cdf = new(4);
        foreach (var v in new[] { 1.0, 2.0, 2.0, 2.0 })
        {
            cdf.Fit(v);
        }

        // Index 2.5 lies between the EQUAL adjacent order stats V[1] = V[2] = 2: the honest
        // answer of any interpolation between equal values is that value.
        Utils.AssertIsApproximate(2.0, cdf.Quantile(0.5), 0);
        Utils.AssertIsApproximate(2.0, cdf.Quantile(0.7), 0);
        // Index 4.5 lies between V[3] = 2 and Max = 2 — also equal neighbors.
        Utils.AssertIsApproximate(2.0, cdf.Quantile(0.9), 0);
        // Index 0.5 lies between Min = 1 and V[0] = 1 — equal neighbors at the head.
        Utils.AssertIsApproximate(1.0, cdf.Quantile(0.1), 0);
        // Non-degenerate interior span between V[0] = 1 and V[1] = 2 at index 1.5.
        Utils.AssertIsApproximate(1.5, cdf.Quantile(0.3), 0);
        // Exact whole window index (p = 0.2, index 1.0, r = 0) must take V[0] as-is.
        Utils.AssertIsApproximate(1.0, cdf.Quantile(0.2), 0);

        // All-equal sample: every order stat, Min and Max are the constant.
        RunningStatistics.EmpiricalCdf flat = new(4);
        for (var i = 0; i < 4; i++)
        {
            flat.Fit(5.0);
        }

        Utils.AssertIsApproximate(5.0, flat.Median, 0);
    }

    [Fact]
    public void Cdf_DuplicatesAndBoundaryValuesHonest()
    {
        // _values after the flush: [Min, V0..V3, Max] = [1,1,2,2,2,2] (length 6).
        RunningStatistics.EmpiricalCdf cdf = new(4);
        foreach (var v in new[] { 1.0, 2.0, 2.0, 2.0 })
        {
            cdf.Fit(v);
        }

        // Outside the range: 0 below Min, 1 at/above Max.
        Utils.AssertIsApproximate(0.0, cdf.Cdf(0.5), 0);
        Utils.AssertIsApproximate(1.0, cdf.Cdf(2.0), 0);
        Utils.AssertIsApproximate(1.0, cdf.Cdf(10.0), 0);

        // x equal to Min, which also sits duplicated at window slot 1: the first value
        // greater than x is at index 2 with a POSITIVE span (the run of duplicated 2s is
        // skipped by the first-match scan), so r = 0 and the answer is slot index 1/6.
        Utils.AssertIsApproximate(1.0 / 6.0, cdf.Cdf(1.0), 0);

        // x strictly inside the only positive-span gap (between window slot 1 = 1 and the
        // run of 2s): positional interpolation, e.g. (1 + 0.5) / 6 at x = 1.5.
        Utils.AssertIsApproximate(0.25, cdf.Cdf(1.5), 0);
        Utils.AssertIsApproximate(1.99 / 6.0, cdf.Cdf(1.99), 0);

        // All-equal sample: Min == Max, so every query must fall in a range guard — no
        // 0/0 interpolation is reachable and no index is ever used out of bounds.
        RunningStatistics.EmpiricalCdf flat = new(4);
        for (var i = 0; i < 4; i++)
        {
            flat.Fit(5.0);
        }

        Utils.AssertIsApproximate(0.0, flat.Cdf(4.99), 0);
        Utils.AssertIsApproximate(1.0, flat.Cdf(5.0), 0);
        Utils.AssertIsApproximate(1.0, flat.Cdf(5.01), 0);
    }

    [Fact]
    public void Quantile_ZeroAndOneBounds()
    {
        RunningStatistics.EmpiricalCdf cdf = new(4);
        foreach (var v in new[] { 1.0, 2.0, 2.0, 2.0 })
        {
            cdf.Fit(v);
        }

        // p exactly 0 and 1 must return Min and Max exactly (and -0.0 must count as 0).
        Utils.AssertIsApproximate(cdf.Min, cdf.Quantile(0.0), 0);
        Utils.AssertIsApproximate(cdf.Max, cdf.Quantile(1.0), 0);
        Utils.AssertIsApproximate(cdf.Min, cdf.Quantile(-0.0), 0);

        // Just inside the bounds: after a flush Min == V[0] and V[last] == Max, so the
        // head/tail interpolation spans have zero WIDTH and the honest answer is the
        // endpoint value itself, never NaN.
        Utils.AssertIsApproximate(1.0, cdf.Quantile(0.002), 0);
        Utils.AssertIsApproximate(2.0, cdf.Quantile(0.998), 0);
    }

    [Fact]
    public void SmallBuffer_CdfThrowsContract()
    {
        // Nobs < NumBins: the under-filled buffer state. Cdf must throw System.InvalidOperationException
        // for EVERY x, including x values that would otherwise be perfectly valid queries
        // (below/at Min, interior, at/above Max) — the throw precedes the NotNaN and range guards.
        RunningStatistics.EmpiricalCdf cdf = new(10);
        for (var v = 1.0; v <= 9.0; v++)
        {
            cdf.Fit(v);
        }

        Assert.Equal(9, cdf.Nobs);
        // Getters stay current even while the buffer is under-filled: Min/Max cover all
        // observations, including ones no flush has ever blended into Values.
        Utils.AssertIsApproximate(1.0, cdf.Min, 0);
        Utils.AssertIsApproximate(9.0, cdf.Max, 0);

        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(0.5));
        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(1.0));
        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(5.0));
        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(9.0));
        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(10.0));

        // The Nobs guard precedes Require.NotNaN: even NaN gets the small-buffer
        // InvalidOperationException, not the ArgumentException pinned by QuantileAndCdf_RejectNaN.
        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(double.NaN));
    }

    [Fact]
    public void SmallBuffer_QuantileThrowsContract()
    {
        RunningStatistics.EmpiricalCdf cdf = new(10);
        for (var v = 1.0; v <= 9.0; v++)
        {
            cdf.Fit(v);
        }

        // Would-otherwise-be-valid probabilities, including the p == 0 / p == 1 fast paths
        // that return Min / Max once the stat is filled. (Quantile_ThrowsWhenFewerObservationsThanBins
        // already pins p = 0.25 and Median.)
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(0.0));
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(0.75));
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(1.0));

        // The Nobs guard precedes Require.ValidProbability: invalid p still yields
        // InvalidOperationException rather than the ArgumentOutOfRangeException pinned
        // for a filled stat in Quantile_EdgeCases and QuantileAndCdf_RejectNaN.
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(-0.1));
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(1.1));
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(double.NaN));
    }

    [Fact]
    public void SmallBuffer_ExactlyNumBinsSucceeds()
    {
        // The transition point: at Nobs == NumBins - 1 both methods still throw, and at
        // Nobs == NumBins exactly the first flush (alpha = 1) lands and both succeed.
        // Unsorted input so the flush's sort is exercised: the sample is {1, 2, 3, 4}.
        RunningStatistics.EmpiricalCdf cdf = new(4);
        foreach (var v in new[] { 3.0, 1.0, 4.0 })
        {
            cdf.Fit(v);
        }

        Assert.Throws<InvalidOperationException>(() => cdf.Cdf(3.5));
        Assert.Throws<InvalidOperationException>(() => cdf.Quantile(0.5));

        cdf.Fit(2.0);
        Assert.Equal(4, cdf.Nobs);

        Utils.AssertIsApproximate(cdf.Min, cdf.Quantile(0.0), 0);
        Utils.AssertIsApproximate(cdf.Max, cdf.Quantile(1.0), 0);
        // Values = sorted sample [1,2,3,4]; index (4 + 1) * 0.5 = 2.5 interpolates
        // between V[1] = 2 and V[2] = 3.
        Utils.AssertIsApproximate(2.5, cdf.Quantile(0.5), 0);

        // Window [Min, V0..V3, Max] = [1,1,2,3,4,4] (length 6): Cdf at Min sits duplicated
        // at slot 1, so the first greater value is slot 2 with r = 0 -> 1/6; interior x = 2.5
        // is at fractional slot 2.5 -> 2.5/6; at/above Max -> 1, below Min -> 0.
        Utils.AssertIsApproximate(0.0, cdf.Cdf(0.0), 0);
        Utils.AssertIsApproximate(1.0 / 6.0, cdf.Cdf(1.0), 0);
        Utils.AssertIsApproximate(2.5 / 6.0, cdf.Cdf(2.5), 0);
        Utils.AssertIsApproximate(1.0, cdf.Cdf(4.0), 0);
    }

    [Fact]
    public void SmallBuffer_MergeThresholdTransition()
    {
        // Merging small stats: the merged object's Nobs is the SUM, and the small-buffer
        // throw depends on that SUM, not on either source having been flushed.
        const int bins = 10;
        RunningStatistics.EmpiricalCdf a = new(bins);
        foreach (var v in new[] { 4.0, 3.0, 2.0, 1.0 })
        {
            a.Fit(v);
        }

        RunningStatistics.EmpiricalCdf b = new(bins);
        foreach (var v in new[] { 9.0, 8.0, 7.0, 6.0, 5.0 })
        {
            b.Fit(v);
        }

        Assert.Throws<InvalidOperationException>(() => a.Cdf(5.0));
        Assert.Throws<InvalidOperationException>(() => b.Quantile(0.5));

        a.Merge(b);

        Assert.Equal(9, a.Nobs);
        Assert.Equal(5, b.Nobs);

        // Combined 4 + 5 = 9 still < bins: the merged object keeps throwing, even for
        // p / x that would otherwise be valid, and the source stays untouched and still
        // small-buffered.
        Assert.Throws<InvalidOperationException>(() => a.Cdf(1.0));
        Assert.Throws<InvalidOperationException>(() => a.Quantile(0.0));
        Assert.Throws<InvalidOperationException>(() => a.Quantile(1.0));
        Assert.Throws<InvalidOperationException>(() => b.Cdf(7.0));

        RunningStatistics.EmpiricalCdf c = new(bins);
        c.Fit(10.0);

        a.Merge(c);

        // Combined Nobs now exactly == bins: the replayed tail observation (10.0) closes
        // the buffer and triggers the flush, so the same queries that just threw succeed
        // with the merged sample {1..10}.
        Assert.Equal(10, a.Nobs);
        Assert.Equal(1, c.Nobs);
        Utils.AssertIsApproximate(1.0, a.Min, 0);
        Utils.AssertIsApproximate(10.0, a.Max, 0);

        Utils.AssertIsApproximate(a.Min, a.Quantile(0.0), 0);
        Utils.AssertIsApproximate(a.Max, a.Quantile(1.0), 0);
        Utils.AssertIsApproximate(5.5, a.Quantile(0.5), 0);
        Utils.AssertIsApproximate(0.0, a.Cdf(0.0), 0);
        Utils.AssertIsApproximate(1.0 / 12.0, a.Cdf(1.0), 0);
        Utils.AssertIsApproximate(1.0, a.Cdf(10.0), 0);
    }

    [Fact]
    public void SmallBuffer_NobsBeyondNumBinsRemainderSucceeds()
    {
        // The neighbouring state pinned for contrast: Nobs > NumBins with a partially
        // filled buffer (Nobs % NumBins = 2 != 0). Only Nobs < NumBins throws, so the
        // methods succeed, and Min/Max already cover the unflushed tail observations
        // (0.5 and 10.5) even though Values lag the last completed flush.
        RunningStatistics.EmpiricalCdf cdf = new(10);
        for (var v = 10.0; v >= 1.0; v--)
        {
            cdf.Fit(v);
        }

        cdf.Fit(10.5);
        cdf.Fit(0.5);

        Assert.Equal(12, cdf.Nobs);
        Utils.AssertIsApproximate(0.5, cdf.Min, 0);
        Utils.AssertIsApproximate(10.5, cdf.Max, 0);

        Utils.AssertIsApproximate(cdf.Min, cdf.Quantile(0.0), 0);
        Utils.AssertIsApproximate(cdf.Max, cdf.Quantile(1.0), 0);
        Utils.AssertIsApproximate(0.0, cdf.Cdf(0.4), 0);
        Utils.AssertIsApproximate(1.0, cdf.Cdf(10.5), 0);
        Utils.AssertIsApproximate(1.0, cdf.Cdf(11.0), 0);
    }

    // Null iterable Fit contract — EmpiricalCdf has no iterable Fit override.
    [Fact]
    public void Fit_RejectsNullIterable()
    {
        RunningStatistics.EmpiricalCdf cdf = new(10);
        Assert.Throws<ArgumentNullException>(() => cdf.Fit((IEnumerable<double>?)null));
        Assert.Equal(0, cdf.Nobs);
    }
}