using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestOrderStatistics
    {
        [Fact]
        public void QuantileOfEmptyIsZero()
        {
            OrderStatistics o = new(20);
            var rng = new Random();

            for (int i = 0; i < 10; i++)
            {
                o.Fit(rng.NextDouble());
            }

            Assert.Equal(0.0, o.Quantile(0.5));
        }

        [Fact]
        public void MergeEmptyIsZero()
        {
            OrderStatistics a = new(20), b = new(20);
            a.Merge(b);

            Assert.Equal(0.0, a.Quantile(0.5));
        }

        [Fact]
        public void MergeDifferentBuffersThrowsError()
        {
            OrderStatistics a = new(10), b = new(20);
            Assert.Throws<Exception>(() => a.Merge(b));
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            int n = 50_000;
            var rng = new Random();

            OrderStatistics a = new(200), b = new(200), c = new(200);

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

            Assert.Equal(a.Quantile(0.1), c.Quantile(0.1), 2);
            Assert.Equal(a.Median, c.Median, 2);
        }

        [Fact]
        public void TestUnitUniform()
        {
            int n = 1_000_000;
            var rng = new Random();

            OrderStatistics o = new(200);

            for (int i = 0; i < n; i++)
            {
                o.Fit(rng.NextDouble());
            }

            Assert.Equal(0.00, o.Quantile(0.00), 2);
            Assert.Equal(0.25, o.Quantile(0.25), 2);
            Assert.Equal(0.50, o.Quantile(0.50), 2);
            Assert.Equal(0.75, o.Quantile(0.75), 2);
            Assert.Equal(1.00, o.Quantile(1.00), 2);
        }

        [Fact]
        public void TestNormal()
        {
            int n = 1_000_000;
            var rng = new Random();

            OrderStatistics o = new(200);

            for (int i = 0; i < n; i++)
            {
                o.Fit(rng.RandNorm());
            }

            double rel_err = Utils.RelError(-2.326347874040846, o.Quantile(0.01));
            Assert.Equal(0.0, rel_err, 1);

            Assert.Equal(-1.2815515655446004, o.Quantile(0.10), 1);
            Assert.Equal(-0.6744897501960818, o.Quantile(0.25), 1);
            Assert.Equal(0.0, o.Median, 1);
            Assert.Equal(0.6744897501960818, o.Quantile(0.75), 1);
            Assert.Equal(1.2815515655446004, o.Quantile(0.90), 1);

            rel_err = Utils.RelError(2.326347874040846, o.Quantile(0.99));
            Assert.Equal(0.0, rel_err, 1);
        }

        [Fact]
        public void TestLogNormal()
        {
            int n = 1_000_000;
            var rng = new Random();
            double mu = 0.0, sd = 0.1;

            OrderStatistics o = new(200);

            for (int i = 0; i < n; i++)
            {
                o.Fit(rng.RandLogNorm(mu, sd));
            }

            Assert.Equal(0.7924429308225133, o.Quantile(0.01), 1);
            Assert.Equal(0.8797168747159579, o.Quantile(0.10), 1);
            Assert.Equal(0.9347754162964689, o.Quantile(0.25), 1);
            Assert.Equal(1.0, o.Median, 1);
            Assert.Equal(1.0697756729225374, o.Quantile(0.75), 1);
            Assert.Equal(1.136729360026064, o.Quantile(0.90), 1);
            Assert.Equal(1.2619205258882853, o.Quantile(0.99), 1);
        }
    }
}
