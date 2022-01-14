using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestMoments
    {
        [Fact]
        public void EmptyMomentsAreNaN()
        {
            Moments moments = new();
            var (mean, variance, skewness, kurtosis) = moments.Value;

            Assert.Equal(double.NaN, mean);
            Assert.Equal(double.NaN, variance);
            Assert.Equal(double.NaN, skewness);
            Assert.Equal(double.NaN, kurtosis);
        }

        [Fact]
        public void MergeEmptyAreNaN()
        {
            Moments a, b;
            a = new(); b = new(); a.Merge(b);
            var (mean, variance, skewness, kurtosis) = a.Value;

            Assert.Equal(double.NaN, mean);
            Assert.Equal(double.NaN, variance);
            Assert.Equal(double.NaN, skewness);
            Assert.Equal(double.NaN, kurtosis);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            int n = 2000;
            var rng = new Random();

            Moments a, b, c;
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

            Assert.Equal(a.Mean, c.Mean, 2);
            Assert.Equal(a.Variance, c.Variance, 2);
            Assert.Equal(a.Skewness, c.Skewness, 2);
            Assert.Equal(a.Kurtosis, c.Kurtosis, 2);
        }

        [Fact]
        public void TestUnitUniform()
        {
            int n = 10_000_000;
            var rng = new Random();

            Moments moments = new Moments();

            for (int i = 0; i < n; i++)
            {
                moments.Fit(rng.NextDouble());
            }

            double mean = (0.0 + 1.0 - double.Epsilon) / 2.0;
            double variance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;
            double skewness = 0.0;
            double kurtosis = -6.0 / 5.0;

            Assert.Equal(mean, moments.Mean, 2);
            Assert.Equal(variance, moments.Variance, 2);
            Assert.Equal(skewness, moments.Skewness, 2);
            Assert.Equal(kurtosis, moments.Kurtosis, 2);
        }

        [Theory]
        [InlineData(2.0, 1.0)]
        [InlineData(2.0, 10.0)]
        public void TestNormal(double mu, double sd)
        {
            int n = 10_000_000;
            var rng = new Random();

            Moments moments = new Moments();

            for (int i = 0; i < n; i++)
            {
                moments.Fit(rng.RandNorm(mu, sd));
            }

            double skewness = 0.0;
            double kurtosis = 0.0;

            Assert.Equal(0.0, Utils.RelError(mu, moments.Mean), 2);
            Assert.Equal(0.0, Utils.RelError(sd, moments.StdDev), 2);
            Assert.Equal(skewness, moments.Skewness, 2);
            Assert.Equal(kurtosis, moments.Kurtosis, 2);
        }

        [Fact]
        public void TestLogNormal()
        {
            double mu = 0.0, sd = 0.5;
            int n = 10_000_000;
            var rng = new Random();

            Moments moments = new Moments();

            for (int i = 0; i < n; i++)
            {
                moments.Fit(rng.RandLogNorm(mu, sd));
            }

            double s2 = sd * sd;
            double mean = Math.Exp(mu + s2 / 2.0);
            double variance = (Math.Exp(s2) - 1) * Math.Exp(2.0 * mu + s2);
            double skewness = (Math.Exp(s2) + 2) * Math.Sqrt(Math.Exp(s2) - 1.0);
            double kurtosis = Math.Exp(4.0 * s2) + 2.0 * Math.Exp(3.0 * s2) + 3.0 * Math.Exp(2.0 * s2) - 6;

            Assert.Equal(0.0, Utils.RelError(mean, moments.Mean), 2);
            Assert.Equal(0.0, Utils.RelError(variance, moments.Variance), 2);
            Assert.Equal(0.0, Utils.RelError(skewness, moments.Skewness), 2);
            Assert.Equal(0.0, Utils.RelError(kurtosis, moments.Kurtosis), 2);
        }
    }
}
