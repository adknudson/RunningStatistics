// <copyright file="Brent.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;

namespace RunningStatistics;

/// <summary>
/// Root finding using the Brent method
/// </summary>
internal static class RootFinding
{
    public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8,
        int maxIterations = 100)
    {
        if (TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out var root))
        {
            return root;
        }

        throw new Exception(
            "The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
    }

    private static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy,
        int maxIterations, out double root)
    {
        if (accuracy <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero.");
        }

        var fMin = f(lowerBound);
        var fMax = f(upperBound);
        var fRoot = fMax;
        double d = 0.0, e = 0.0;

        root = upperBound;
        var xMid = double.NaN;

        // Root must be bracketed.
        if (Math.Sign(fMin) == Math.Sign(fMax))
        {
            return false;
        }

        for (var i = 0; i <= maxIterations; i++)
        {
            // adjust bounds
            if (Math.Sign(fRoot) == Math.Sign(fMax))
            {
                upperBound = lowerBound;
                fMax = fMin;
                e = d = root - lowerBound;
            }

            if (Math.Abs(fMax) < Math.Abs(fRoot))
            {
                lowerBound = root;
                root = upperBound;
                upperBound = lowerBound;
                fMin = fRoot;
                fRoot = fMax;
                fMax = fMin;
            }

            // convergence check
            var xAcc1 = Precision.PositiveDoublePrecision * Math.Abs(root) + 0.5 * accuracy;
            var xMidOld = xMid;
            xMid = (upperBound - root) / 2.0;

            if (Math.Abs(xMid) <= xAcc1 || fRoot.AlmostEqualNormRelative(0, fRoot, accuracy))
            {
                return true;
            }

            if (xMid == xMidOld)
            {
                // accuracy not sufficient, but cannot be improved further
                return false;
            }

            if (Math.Abs(e) >= xAcc1 && Math.Abs(fMin) > Math.Abs(fRoot))
            {
                // Attempt inverse quadratic interpolation
                var s = fRoot / fMin;
                double p;
                double q;
                if (lowerBound.AlmostEqualRelative(upperBound))
                {
                    p = 2.0 * xMid * s;
                    q = 1.0 - s;
                }
                else
                {
                    q = fMin / fMax;
                    var r = fRoot / fMax;
                    p = s * (2.0 * xMid * q * (q - r) - (root - lowerBound) * (r - 1.0));
                    q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                }

                if (p > 0.0)
                {
                    // Check whether in bounds
                    q = -q;
                }

                p = Math.Abs(p);
                if (2.0 * p < Math.Min(3.0 * xMid * q - Math.Abs(xAcc1 * q), Math.Abs(e * q)))
                {
                    // Accept interpolation
                    e = d;
                    d = p / q;
                }
                else
                {
                    // Interpolation failed, use bisection
                    d = xMid;
                    e = d;
                }
            }
            else
            {
                // Bounds decreasing too slowly, use bisection
                d = xMid;
                e = d;
            }

            lowerBound = root;
            fMin = fRoot;
            if (Math.Abs(d) > xAcc1)
            {
                root += d;
            }
            else
            {
                root += Sign(xAcc1, xMid);
            }
            
            fRoot = f(root);
        }

        return false;
    }

    /// <summary>Helper method useful for preventing rounding errors.</summary>
    /// <returns>a*sign(b)</returns>
    private static double Sign(double a, double b)
    {
        return b >= 0 ? a >= 0 ? a : -a : a >= 0 ? -a : a;
    }
}