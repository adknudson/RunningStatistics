using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestMean
    {
        [Fact]
        public void EmptyMeanValueIsNaN()
        {
            Mean stat = new();
            var val = stat.Value;

            Assert.Equal(double.NaN, val);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void MeanOfSumOneToN(int n)
        {
            Mean mean = new();
            double true_mean = (double)(n * (n + 1)) / (2.0 * n);

            for (int i = 1; i <= n; i++)
            {
                mean.Fit(i);
            }

            Assert.Equal(mean.Value, true_mean);
        }

        [Fact]
        public void MergeEmptyIsEmpty()
        {
            Mean a, b;
            a = new(); b = new();
            a.Merge(b);

            Assert.Equal(a.Value, double.NaN);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            int n = 2000;
            var rng = new Random();

            Mean a, b, c;
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

            Assert.Equal(a.Value, c.Value, 7);
        }

        [Fact]
        public void TestUnitUniform()
        {
            int n = 1_000_000;
            var rng = new Random();

            Mean mean = new();

            for (int i = 0; i < n; i++)
            {
                mean.Fit(rng.NextDouble());
            }

            double real_mean = (1.0 - double.Epsilon) / 2;

            Assert.Equal(0.0, Utils.RelError(real_mean, mean.Value), 2);
        }

        [Theory]
        [InlineData(100.0, 1.0)]
        [InlineData(100.0, 10.0)]
        [InlineData(100.0, 100.0)]
        public void TestNormal(double mu, double sd)
        {
            int n = 1_000_000;
            var rng = new Random();

            Mean mean = new();

            for (int i = 0; i < n; i++)
            {
                mean.Fit(rng.RandNorm(mu, sd));
            }

            Assert.Equal(0.0, Utils.RelError(mu, mean.Value), 2);
        }

        [Theory]
        [InlineData(1.0, 1.0)]
        public void TestLogNormal(double mu, double sd)
        {
            int n = 1_000_000;
            var rng = new Random();

            Mean mean = new();

            for (int i = 0; i < n; i++)
            {
                mean.Fit(rng.RandLogNorm(mu, sd));
            }

            double real_mean = Math.Exp(mu + sd * sd / 2.0);

            Assert.Equal(0.0, Utils.RelError(real_mean, mean.Value), 2);
        }
    }
}
