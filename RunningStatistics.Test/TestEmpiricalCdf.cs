using System;
using Xunit;

namespace RunningStatistics.Test
{
    public class TestEmpiricalCdf
    {
        [Fact]
        public void QuantileOfEmptyIsZero()
        {
            EmpiricalCdf o = new(20);
            var rng = new Random();

            for (var i = 0; i < 10; i++)
            {
                o.Fit(rng.NextDouble());
            }

            Assert.Equal(0.0, o.Quantile(0.5));
        }

        [Fact]
        public void MergeEmptyIsZero()
        {
            EmpiricalCdf a = new(20), b = new(20);
            a.Merge(b);

            Assert.Equal(0.0, a.Quantile(0.5));
        }

        [Fact]
        public void MergeDifferentBuffersThrowsError()
        {
            EmpiricalCdf a = new(10), b = new(20);
            Assert.Throws<Exception>(() => a.Merge(b));
        }

        [Fact]
        public void MergingWhereOneIsEmptyEqualsNonEmptyInstance()
        {
            EmpiricalCdf a = new(20), b = new(20);
            var rng = new Random();

            for (var i = 0; i < 1000; i++)
            {
                b.Fit(rng.NextDouble());
            }

            var aMergeB = EmpiricalCdf.Merge(a, b);
            var bMergeA = EmpiricalCdf.Merge(b, a);
            
            Assert.Equal(b.Min, aMergeB.Min);
            Assert.Equal(b.Max, aMergeB.Max);
            Assert.Equal(b.Quantile(0.25), aMergeB.Quantile(0.25));
            Assert.Equal(b.Quantile(0.75), aMergeB.Quantile(0.75));

            
            Assert.Equal(b.Min, bMergeA.Min);
            Assert.Equal(b.Max, bMergeA.Max);
            Assert.Equal(b.Quantile(0.25), bMergeA.Quantile(0.25));
            Assert.Equal(b.Quantile(0.75), bMergeA.Quantile(0.75));
        }

        [Fact]
        public void MergePartsEqualsMergeAll()
        {
            const int n = 50_000;
            var rng = new Random();

            EmpiricalCdf a = new(), b = new(), c = new();

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

            Assert.Equal(a.Quantile(0.1), c.Quantile(0.1), 2);
            Assert.Equal(a.Median, c.Median, 2);
        }

        [Fact]
        public void TestUnitUniform()
        {
            const int n = 1_000_000;
            var rng = new Random();

            EmpiricalCdf o = new();

            for (var i = 0; i < n; i++)
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
            const int n = 1_000_000;
            var rng = new Random();

            EmpiricalCdf o = new();

            for (var i = 0; i < n; i++)
            {
                o.Fit(rng.RandNorm());
            }

            var relErr = Utils.RelError(-2.326347874040846, o.Quantile(0.01));
            Assert.Equal(0.0, relErr, 1);

            Assert.Equal(-1.2815515655446004, o.Quantile(0.10), 1);
            Assert.Equal(-0.6744897501960818, o.Quantile(0.25), 1);
            Assert.Equal(0.0, o.Median, 1);
            Assert.Equal(0.6744897501960818, o.Quantile(0.75), 1);
            Assert.Equal(1.2815515655446004, o.Quantile(0.90), 1);

            relErr = Utils.RelError(2.326347874040846, o.Quantile(0.99));
            Assert.Equal(0.0, relErr, 1);
        }

        [Fact]
        public void TestLogNormal()
        {
            const int n = 1_000_000;
            var rng = new Random();
            const double mu = 0.0;
            const double sd = 0.1;

            EmpiricalCdf o = new();

            for (var i = 0; i < n; i++)
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
