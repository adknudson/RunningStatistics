using System;

namespace RunningStatistics;

internal static class Polynomial
{
    public static double Evaluate(double z, params double[] coefficients)
    {

        // 2020-10-07 jbialogrodzki #730 Since this is public API we should probably
        // handle null arguments? It doesn't seem to have been done consistently in this class though.
        ArgumentNullException.ThrowIfNull(coefficients);

        // 2020-10-07 jbialogrodzki #730 Zero polynomials need explicit handling.
        // Without this check, we attempted to peek coefficients at negative indices!
        var n = coefficients.Length;
        if (n == 0)
        {
            return 0;
        }

        var sum = coefficients[n - 1];
        for (var i = n - 2; i >= 0; --i)
        {
            sum *= z;
            sum += coefficients[i];
        }

        return sum;
    }
}