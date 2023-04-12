using System;

namespace RunningStatistics.Tests
{
    internal static class Utils
    {
        internal static double RandNorm(this Random rng, double mu = 0.0, double sd = 1.0)
        {
            var u1 = 1.0 - rng.NextDouble();
            var u2 = 1.0 - rng.NextDouble();
            var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mu + z * sd;
        }

        internal static double RandLogNorm(this Random rng, double mu = 0.0, double sd = 1.0)
        {
            return Math.Exp(rng.RandNorm(mu, sd));
        }

        internal static double RelError(double expected, double observed)
        {
            return (observed - expected) / expected;
        }
    }
}
