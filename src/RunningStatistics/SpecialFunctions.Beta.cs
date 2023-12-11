// <copyright file="Beta.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

// <contribution>
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

using System;

// ReSharper disable once CheckNamespace
namespace RunningStatistics;

internal static partial class SpecialFunctions
{
    public static double BetaRegularized(double a, double b, double x)
    {
        if (a < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(a), "Value must be non-negative.");
        }

        if (b < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(b), "Value must be non-negative.");
        }

        if (x < 0.0 || x > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Value must be in the interval [0, 1].");
        }

        return UnsafeBetaRegularized(a, b, x);
    }
        
    public static double UnsafeBetaRegularized(double a, double b, double x)
    {
        var bt = x == 0 || x == 1
            ? 0.0
            : Math.Exp(GammaLn(a + b) - GammaLn(a) - GammaLn(b) + a*Math.Log(x) + b*Math.Log(1.0 - x));

        var symmetryTransformation = x >= (a + 1.0) / (a + b + 2.0);

        /* Continued fraction representation */
        var eps = Precision.DoublePrecision;
        var fPrecisionMin = 0.0.Increment() / eps;

        if (symmetryTransformation)
        {
            x = 1.0 - x;
            (a, b) = (b, a);
        }

        var qab = a + b;
        var qap = a + 1.0;
        var qam = a - 1.0;
        var c = 1.0;
        var d = 1.0 - qab * x / qap;

        if (Math.Abs(d) < fPrecisionMin)
        {
            d = fPrecisionMin;
        }

        d = 1.0 / d;
        var h = d;

        for (int m = 1, m2 = 2; m <= 50000; m++, m2 += 2)
        {
            var aa = m * (b - m) * x / ((qam + m2) * (a + m2));
            d = 1.0 + aa * d;

            if (Math.Abs(d) < fPrecisionMin)
            {
                d = fPrecisionMin;
            }

            c = 1.0 + aa / c;
            if (Math.Abs(c) < fPrecisionMin)
            {
                c = fPrecisionMin;
            }

            d = 1.0 / d;
            h *= d * c;
            aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
            d = 1.0 + aa * d;

            if (Math.Abs(d) < fPrecisionMin)
            {
                d = fPrecisionMin;
            }

            c = 1.0 + aa / c;

            if (Math.Abs(c) < fPrecisionMin)
            {
                c = fPrecisionMin;
            }

            d = 1.0 / d;
            var del = d * c;
            h *= del;

            if (Math.Abs(del - 1.0) <= eps)
            {
                return symmetryTransformation ? 1.0 - bt * h / a : bt * h / a;
            }
        }

        return symmetryTransformation ? 1.0 - bt * h / a : bt * h / a;
    }
}