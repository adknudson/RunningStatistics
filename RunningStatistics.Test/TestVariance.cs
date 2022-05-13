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

            Assert.Equal(1.0, (double) variance);
        }

        [Fact]
        public void MergeEmptyVarianceIsOne()
        {
            Variance a = new(); 
            Variance b = new();
            a.Merge(b);

            Assert.Equal(1.0, (double) a);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            const int n = 2000;
            var rng = new Random();

            Variance a = new(); 
            Variance b = new();
            Variance c = new();

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

            Assert.Equal((double)a, (double)c, 1);
        }

        [Fact]
        public void TestUnitUniform()
        {
            const int n = 1_000_000;
            var rng = new Random();

            Variance variance = new();

            for (var i = 0; i < n; i++)
            {
                variance.Fit(rng.NextDouble());
            }

            var realVariance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;

            Assert.Equal(0.0, Utils.RelError(realVariance, (double)variance), 1);
        }

        [Theory]
        [InlineData(100.0, 1.0)]
        [InlineData(100.0, 10.0)]
        [InlineData(100.0, 100.0)]
        public void TestNormal(double mu, double sd)
        {
            const int n = 1_000_000;
            var rng = new Random();

            Variance variance = new();

            for (var i = 0; i < n; i++)
            {
                variance.Fit(rng.RandNorm(mu, sd));
            }

            var realVariance = sd * sd;

            Assert.Equal(0.0, Utils.RelError(realVariance, (double)variance), 1);
        }

        [Theory]
        [InlineData(1.0, 1.0)]
        public void TestLogNormal(double mu, double sd)
        {
            const int n = 1_000_000;
            var rng = new Random();

            Variance variance = new();

            for (var i = 0; i < n; i++)
            {
                variance.Fit(rng.RandLogNorm(mu, sd));
            }

            var s2 = sd * sd;
            var realVariance = (Math.Exp(s2) - 1) * Math.Exp(2.0 * mu + s2);

            Assert.Equal(0.0, Utils.RelError(realVariance, (double)variance), 1);
        }

        [Fact]
        public void CreateFromOther()
        {
            Variance a = new();
            var rng = new Random();
            
            for (var i = 0; i < 100; i++)
            {
                a.Fit(rng.NextDouble());
            }

            var b = new Variance(a);
            Assert.Equal(a.Count, b.Count);
            Assert.Equal(a.Value, b.Value);

            b.Fit(rng.NextDouble());
            Assert.Equal(a.Count + 1, b.Count);
            Assert.NotEqual(a.Value, b.Value);
        }

        [Fact]
        public void StaticMergeDoesntAffectOriginals()
        {
            Variance a = new(), b = new();
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                a.Fit(rng.NextDouble());
                b.Fit(rng.NextDouble() * 2);
            }

            var c = Variance.Merge(a, b);
            Assert.Equal(a.Count + b.Count, c.Count);

            c.Fit(rng.NextDouble());
            Assert.Equal(a.Count + b.Count + 1, c.Count);
        }
    }
}
