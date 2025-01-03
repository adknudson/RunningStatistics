namespace RunningStatistics;

internal static class Utils
{
    /// <summary>
    /// Bessel correction for sample variance.
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static double Bessel(double n)
    {
        return n / (n - 1);
    }

    /// <summary>
    /// Linear interpolation of two values.
    /// </summary>
    /// <param name="a">The first value</param>
    /// <param name="b">The second value</param>
    /// <param name="p">The percent of weight given to the second value</param>
    /// <returns>A value between <see cref="a"/> and <see cref="b"/></returns>
    public static double Smooth(double a, double b, double p)
    {
        Require.ValidProbability(p);
        return a + p * (b - a);
    }
}