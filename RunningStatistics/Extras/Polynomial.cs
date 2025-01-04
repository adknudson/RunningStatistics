using System;

namespace RunningStatistics;

internal static class Polynomial
{
    public static double Evaluate(double z, params double[] coefficients)
    {

#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(coefficients);
#else
        if (coefficients is null)
        {
            throw new ArgumentNullException(nameof(coefficients));
        }
#endif
        
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