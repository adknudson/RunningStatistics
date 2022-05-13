using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestMean
    {
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void MeanOfSumOneToN(int n)
        {
            Mean mean = new();
            var trueMean = (n * (n + 1)) / (2.0 * n);

            for (var i = 1; i <= n; i++)
            {
                mean.Fit(i);
            }

            Assert.Equal(mean.Value, trueMean);
        }

        [Fact]
        public void MergeEmptyIsEmpty()
        {
            Mean a = new(); Mean b = new();
            a.Merge(b);

            Assert.Equal(double.NaN, a.Value);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            const int n = 2000;
            var rng = new Random();

            Mean a = new(); Mean b = new(); Mean c = new();

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

            Assert.Equal(a.Value, c.Value, 7);
        }

        [Fact]
        public void CreateFromOther()
        {
            var a = new Mean();
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                a.Fit(rng.NextDouble());
            }

            var b = new Mean(a);
            Assert.Equal(a.Count, b.Count);
            Assert.Equal(a.Value, b.Value);

            b.Fit(100);
            
            Assert.NotEqual(a.Value, b.Value);
            Assert.Equal(a.Count + 1, b.Count);
        }

        [Fact]
        public void StaticMergeDoesntAffectOriginals()
        {
            var a = new Mean();
            var b = new Mean();
            for (var i = 0; i < 10; i++)
            {
                a.Fit(100 - i);
                b.Fit(i + 10);
            }

            var c = Mean.Merge(a, b);
            Assert.Equal(a.Count + b.Count, c.Count);

            c.Fit(12);
            Assert.Equal(a.Count + b.Count + 1, c.Count);
        }

        [Fact]
        public void TestUnitUniform()
        {
            const int n = 1_000_000;
            var rng = new Random();

            Mean mean = new();

            for (var i = 0; i < n; i++)
            {
                mean.Fit(rng.NextDouble());
            }

            var real_mean = (1.0 - double.Epsilon) / 2;

            Assert.Equal(0.0, Utils.RelError(real_mean, mean.Value), 2);
        }

        [Theory]
        [InlineData(100.0, 1.0)]
        [InlineData(100.0, 10.0)]
        [InlineData(100.0, 100.0)]
        public void TestNormal(double mu, double sd)
        {
            const int n = 1_000_000;
            var rng = new Random();

            Mean mean = new();

            for (var i = 0; i < n; i++)
            {
                mean.Fit(rng.RandNorm(mu, sd));
            }

            Assert.Equal(0.0, Utils.RelError(mu, mean.Value), 2);
        }

        [Theory]
        [InlineData(1.0, 1.0)]
        public void TestLogNormal(double mu, double sd)
        {
            const int n = 1_000_000;
            var rng = new Random();

            Mean mean = new();

            for (var i = 0; i < n; i++)
            {
                mean.Fit(rng.RandLogNorm(mu, sd));
            }

            var realMean = Math.Exp(mu + sd * sd / 2.0);

            Assert.Equal(0.0, Utils.RelError(realMean, mean.Value), 2);
        }
    }
}
