using System;
using System.Linq;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestHistogram
    {
        // |-inf, 0|, |0, 10|, |10, 100|
        double[] edges = { double.NegativeInfinity, 0, 10, 100 };

        [Fact]
        public void EmptyHistIsZero()
        {
            // (-inf, 0], (0, 10], (10, 100]
            Histogram h = new(edges, false, false);

            Assert.Equal(0, h.Count);

            foreach (var (bin, count) in h)
            {
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            int n = 2000;
            var rng = new Random();
            double[] small_edges = { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };

            Histogram a = new(small_edges), b = new(small_edges), c = new(small_edges);

            double v;
            for (int i = 0; i < n; i++)
            {
                v = rng.NextDouble();
                a.Fit(v);
                c.Fit(v);
            }

            for (int i = 0; i < n; i++)
            {
                v = rng.NextDouble();
                b.Fit(v);
                c.Fit(v);
            }

            a.Merge(b);

            foreach (var ((bina, counta), (binb, countb)) in a.Zip(c))
            {
                Assert.Equal(counta, countb);
            }
        }

        [Fact]
        public void LeftClosed()
        {
            // [-inf, 0), [0, 10), [10, 100]
            Histogram h = new(edges, true, true);

            h.Fit(-1);
            h.Fit(1);
            h.Fit(20);
            h.Fit(1000);

            // One value out of bounds on the upper end
            Assert.Equal(0, h.OutOfBoundsCounts[0]);
            Assert.Equal(1, h.OutOfBoundsCounts[1]);

            foreach (var (bin, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void LeftOpen()
        {
            // [-inf, 0), [0, 10), [10, 100)
            Histogram h = new(edges, true, false);

            h.Fit(-1);
            h.Fit(1);
            h.Fit(20);
            h.Fit(100);

            // One value out of bounds on the upper end
            Assert.Equal(0, h.OutOfBoundsCounts[0]);
            Assert.Equal(1, h.OutOfBoundsCounts[1]);

            foreach (var (bin, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void RightClosed()
        {
            // [-inf, 0], (0, 10], (10, 100]
            Histogram h = new(edges, false, true);

            h.Fit(double.NegativeInfinity);
            h.Fit(10);
            h.Fit(100);
            h.Fit(101);

            // One value out of bounds on the upper end
            Assert.Equal(0, h.OutOfBoundsCounts[0]);
            Assert.Equal(1, h.OutOfBoundsCounts[1]);

            foreach (var (bin, count) in h)
            {
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void RightOpen()
        {
            // (-inf, 0], (0, 10], (10, 100]
            Histogram h = new(edges, false, false);

            h.Fit(double.NegativeInfinity);
            h.Fit(0.0);
            h.Fit(10);
            h.Fit(100);
            h.Fit(double.PositiveInfinity);

            // Two values out of bounds on both ends (one each)
            Assert.Equal(1, h.OutOfBoundsCounts[0]);
            Assert.Equal(1, h.OutOfBoundsCounts[1]);

            foreach (var (bin, count) in h)
            {
                Assert.Equal(1, count);
            }
        }
    }
}
