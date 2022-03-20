using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestVariance
    {
        [Fact]
        public void EmptyVarianceIsOne()
        {
            Variance variance = new();

            Assert.Equal(1.0, variance.Value);
        }

        [Fact]
        public void MergeEmptyVarianceIsOne()
        {
            Variance a, b;
            a = new(); b = new();
            a.Merge(b);

            Assert.Equal(1.0, a.Value);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            var n = 2000;
            var rng = new Random();

            Variance a, b, c;
            a = new(); b = new(); c = new();

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

            Assert.Equal(a.Value, c.Value, 1);
        }

        [Fact]
        public void TestUnitUniform()
        {
            var n = 1_000_000;
            var rng = new Random();

            Variance variance = new();

            for (var i = 0; i < n; i++)
            {
                variance.Fit(rng.NextDouble());
            }

            var real_variance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;

            Assert.Equal(0.0, Utils.RelError(real_variance, variance.Value), 1);
        }

        [Theory]
        [InlineData(100.0, 1.0)]
        [InlineData(100.0, 10.0)]
        [InlineData(100.0, 100.0)]
        public void TestNormal(double mu, double sd)
        {
            var n = 1_000_000;
            var rng = new Random();

            Variance variance = new();

            for (var i = 0; i < n; i++)
            {
                variance.Fit(rng.RandNorm(mu, sd));
            }

            var real_variance = sd * sd;

            Assert.Equal(0.0, Utils.RelError(real_variance, variance.Value), 1);
        }

        [Theory]
        [InlineData(1.0, 1.0)]
        public void TestLogNormal(double mu, double sd)
        {
            var n = 1_000_000;
            var rng = new Random();

            Variance variance = new();

            for (var i = 0; i < n; i++)
            {
                variance.Fit(rng.RandLogNorm(mu, sd));
            }

            var s2 = sd * sd;
            var real_variance = (Math.Exp(s2) - 1) * Math.Exp(2.0 * mu + s2);

            Assert.Equal(0.0, Utils.RelError(real_variance, variance.Value), 1);
        }
    }
}
