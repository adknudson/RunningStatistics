using System;
using System.Linq;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestHistogram
    {
        // |-inf, 0|, |0, 10|, |10, 100|
        private readonly double[] _edges = { double.NegativeInfinity, 0, 10, 100 };

        [Fact]
        public void EmptyHistIsZero()
        {
            // (-inf, 0], (0, 10], (10, 100]
            Histogram h = new(_edges, false, false);

            Assert.Equal(0, h.Count);

            foreach (var (_, count) in h)
            {
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            const int n = 2000;
            var rng = new Random();
            double[] smallEdges = { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };

            Histogram a = new(smallEdges), b = new(smallEdges), c = new(smallEdges);

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

            foreach (var ((_, countA), (_, countB)) in a.Zip(c))
            {
                Assert.Equal(countA, countB);
            }
        }

        [Fact]
        public void LeftClosed()
        {
            // [-inf, 0), [0, 10), [10, 100]
            Histogram h = new(_edges);

            h.Fit(-1);
            h.Fit(1);
            h.Fit(20);
            h.Fit(1000);

            // One value out of bounds on the upper end
            Assert.Equal(0, h.OutOfBoundsCounts.Lower);
            Assert.Equal(1, h.OutOfBoundsCounts.Upper);

            foreach (var (_, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void LeftOpen()
        {
            // [-inf, 0), [0, 10), [10, 100)
            Histogram h = new(_edges, true, false);

            h.Fit(-1);
            h.Fit(1);
            h.Fit(20);
            h.Fit(100);

            // One value out of bounds on the upper end
            Assert.Equal(0, h.OutOfBoundsCounts.Lower);
            Assert.Equal(1, h.OutOfBoundsCounts.Upper);

            foreach (var (_, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void RightClosed()
        {
            // [-inf, 0], (0, 10], (10, 100]
            Histogram h = new(_edges, false);

            h.Fit(double.NegativeInfinity);
            h.Fit(10);
            h.Fit(100);
            h.Fit(101);

            // One value out of bounds on the upper end
            Assert.Equal(0, h.OutOfBoundsCounts.Lower);
            Assert.Equal(1, h.OutOfBoundsCounts.Upper);

            foreach (var (_, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void RightOpen()
        {
            // (-inf, 0], (0, 10], (10, 100]
            Histogram h = new(_edges, false, false);

            h.Fit(double.NegativeInfinity);
            h.Fit(0.0);
            h.Fit(10);
            h.Fit(100);
            h.Fit(double.PositiveInfinity);

            // Two values out of bounds on both ends (one each)
            Assert.Equal(1, h.OutOfBoundsCounts.Lower);
            Assert.Equal(1, h.OutOfBoundsCounts.Upper);

            foreach (var (_, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void CreateFromOther()
        {
            // (0.0, 0.1], (0.1, 0.5], (0.5, 0.9]
            Histogram a = new(new []{0, 0.1, 0.5, 0.9}, leftClosed: false, endsClosed: false);
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                a.Fit(rng.NextDouble());
            }

            var b = new Histogram(a);

            Assert.Equal(a.Count, b.Count);
            Assert.Equal(a.OutOfBoundsCounts.Upper, b.OutOfBoundsCounts.Upper);
            
            b.Fit(0.99, 10);
            Assert.Equal(a.OutOfBoundsCounts.Upper + 10, b.OutOfBoundsCounts.Upper);
        }

        [Fact]
        public void StaticMergeDoesntAffectOriginals()
        {
            Histogram a = new(new []{0, 0.1, 0.5, 0.9}, leftClosed: false, endsClosed: false);
            Histogram b = new(new []{0, 0.1, 0.5, 0.9}, leftClosed: false, endsClosed: false);
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                a.Fit(rng.NextDouble());
                b.Fit(rng.NextDouble());
            }

            var c = Histogram.Merge(a, b);
            Assert.Equal(a.Count + b.Count, c.Count);
            Assert.Equal(a.OutOfBoundsCounts.Upper + b.OutOfBoundsCounts.Upper, c.OutOfBoundsCounts.Upper);

            c.Fit(0.99, 10);
            Assert.Equal(a.OutOfBoundsCounts.Upper + b.OutOfBoundsCounts.Upper + 10, c.OutOfBoundsCounts.Upper);
        }
    }
}
