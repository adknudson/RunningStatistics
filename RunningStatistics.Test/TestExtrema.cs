using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestExtrema
    {
        [Fact]
        public void EmptyExtremaAreInf()
        {
            Extrema extrema = new();

            Assert.Equal(double.PositiveInfinity, extrema.Min);
            Assert.Equal(double.NegativeInfinity, extrema.Max);
        }

        [Fact]
        public void MergeEmptyAreInf()
        {
            Extrema a, b;
            a = new(); b = new(); a.Merge(b);

            Assert.Equal(double.PositiveInfinity, a.Min);
            Assert.Equal(double.NegativeInfinity, a.Max);
        }

        [Fact]
        public void FittingSingleValue()
        {
            Extrema extrema = new();

            extrema.Fit(0);
            Assert.Equal(0.0, extrema.Min);
            Assert.Equal(0.0, extrema.Max);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            int n = 2000;
            var rng = new Random();

            Extrema a, b, c;
            a = new(); b = new(); c = new();

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

            Assert.Equal(a.Min, c.Min);
            Assert.Equal(a.CountMin, c.CountMin);
            Assert.Equal(a.Max, c.Max);
            Assert.Equal(a.CountMax, c.CountMax);
        }
    }
}
