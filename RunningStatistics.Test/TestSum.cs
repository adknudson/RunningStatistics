using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestSum
    {
        [Fact]
        public void EmptySumIsZero()
        {
            Sum s = new();

            Assert.Equal(0.0, s.Value);
        }

        [Fact]
        public void MergeEmptyIsZero()
        {
            Sum s = new(), t = new();
            s.Merge(t);

            Assert.Equal(0.0, s.Value);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void SumOneToN(int n)
        {
            Sum s = new();

            for (int i = 1; i <= n; i++)
            {
                s.Fit(i);
            }

            Assert.Equal(n * (n + 1) / 2, s.Value);
        }

        [Fact]
        public void SumOfPartsEqualsSumOfWhole()
        {
            int n = 2000;
            var rng = new Random();

            Sum a = new(), b = new(), c = new();

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

            Assert.Equal(a.Value, c.Value, 7);
            Assert.Equal(a.Mean, c.Mean, 7);
        }
    }
}
