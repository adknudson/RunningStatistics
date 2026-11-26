namespace RunningStatistics;

internal static class Utils
{
    /// <summary>
    /// Bessel correction for sample variance.
    /// </summary>
    /// <param name="n">The number of observations in the sample.</param>
    /// <returns>The Bessel correction factor <c>n / (n - 1)</c>.</returns>
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

    /// <summary>
    /// The same blend as <see cref="Smooth"/> without the probability check. Use it only
    /// where <paramref name="p"/> is already known to lie in [0, 1]: validated once at a
    /// public entry point (the public <c>Fit</c>, <c>Merge</c> and <c>Quantile</c> wrappers
    /// keep the guarded <see cref="Smooth"/>), or provably built from a ratio of
    /// non-negative counts. The arithmetic is the identical expression, so a call site
    /// switched from <see cref="Smooth"/> to this form keeps its state bits unchanged.
    /// </summary>
    /// <param name="a">The first value</param>
    /// <param name="b">The second value</param>
    /// <param name="p">The percent of weight given to the second value, in [0, 1]</param>
    /// <returns>A value between <see cref="a"/> and <see cref="b"/></returns>
    public static double SmoothUnchecked(double a, double b, double p) => a + p * (b - a);
}