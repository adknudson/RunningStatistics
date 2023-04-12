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
            var (mean, variance, skewness, kurtosis) = moments;

            Assert.Equal(double.NaN, mean);
            Assert.Equal(double.NaN, variance);
            Assert.Equal(double.NaN, skewness);
            Assert.Equal(double.NaN, kurtosis);
        }

        [Fact]
        public void MergeEmptyAreNaN()
        {
            Moments a = new(); Moments b = new(); a.Merge(b);
            var (mean, variance, skewness, kurtosis) = a;

            Assert.Equal(double.NaN, mean);
            Assert.Equal(double.NaN, variance);
            Assert.Equal(double.NaN, skewness);
            Assert.Equal(double.NaN, kurtosis);
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            const int n = 2000;
            var rng = new Random();

            Moments a = new(); Moments b = new(); Moments c = new();

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

            Assert.Equal(a.Mean, c.Mean, 2);
            Assert.Equal(a.Variance, c.Variance, 2);
            Assert.Equal(a.Skewness, c.Skewness, 2);
            Assert.Equal(a.Kurtosis, c.Kurtosis, 2);
        }

        [Fact]
        public void StaticMergeDoesntAffectOriginals()
        {
            Moments a = new(), b = new();
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                a.Fit(rng.NextDouble());
                b.Fit(rng.NextDouble());
            }

            var c = Moments.Merge(a, b);
            Assert.Equal(a.Nobs + b.Nobs, c.Nobs);
            
            c.Fit(rng.NextDouble());
            Assert.Equal(a.Nobs + b.Nobs + 1, c.Nobs);
        }

        [Fact]
        public void TestUnitUniform()
        {
            const int n = 10_000_000;
            var rng = new Random();

            var moments = new Moments();

            for (var i = 0; i < n; i++)
            {
                moments.Fit(rng.NextDouble());
            }

            const double mean = (0.0 + 1.0 - double.Epsilon) / 2.0;
            var variance = Math.Pow(1.0 - double.Epsilon, 2) / 12.0;
            const double skewness = 0.0;
            const double excessKurtosis = -6.0 / 5.0;

            Assert.Equal(mean, moments.Mean, 2);
            Assert.Equal(variance, moments.Variance, 2);
            Assert.Equal(skewness, moments.Skewness, 2);
            Assert.Equal(excessKurtosis, moments.ExcessKurtosis, 2);
        }

        [Theory]
        [InlineData(2.0, 1.0)]
        [InlineData(2.0, 10.0)]
        public void TestNormal(double mu, double sd)
        {
            const int n = 10_000_000;
            var rng = new Random();

            var moments = new Moments();

            for (var i = 0; i < n; i++)
            {
                moments.Fit(rng.RandNorm(mu, sd));
            }

            const double skewness = 0.0;
            const double excessKurtosis = 0.0;

            Assert.Equal(0.0, Utils.RelError(mu, moments.Mean), 2);
            Assert.Equal(0.0, Utils.RelError(sd, Math.Sqrt(moments.Variance)), 2);
            Assert.Equal(skewness, moments.Skewness, 2);
            Assert.Equal(excessKurtosis, moments.ExcessKurtosis, 2);
        }

        [Fact]
        public void TestLogNormal()
        {
            const double mu = 0.0;
            const double sd = 0.5;
            const double s2 = sd * sd;
            const int n = 100_000_000;
            var rng = new Random();

            var moments = new Moments();

            for (var i = 0; i < n; i++)
            {
                moments.Fit(rng.RandLogNorm(mu, sd));
            }

            var mean = Math.Exp(mu + s2 / 2.0);
            var variance = (Math.Exp(s2) - 1) * Math.Exp(2.0 * mu + s2);
            var skewness = (Math.Exp(s2) + 2) * Math.Sqrt(Math.Exp(s2) - 1.0);
            var excessKurtosis = Math.Exp(4.0 * s2) + 2.0 * Math.Exp(3.0 * s2) + 3.0 * Math.Exp(2.0 * s2) - 6;

            Assert.Equal(0.0, Utils.RelError(mean, moments.Mean), 2);
            Assert.Equal(0.0, Utils.RelError(variance, moments.Variance), 2);
            Assert.Equal(0.0, Utils.RelError(skewness, moments.Skewness), 2);
            Assert.Equal(0.0, Utils.RelError(excessKurtosis, moments.ExcessKurtosis), 2);
        }
    }
}
