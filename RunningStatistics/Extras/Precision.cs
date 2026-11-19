// <copyright file="Precision.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

internal static partial class Precision
{
    /// <summary>
    /// The number of binary digits used to write the significant part of a double-precision
    /// floating-point number. By analogy, in a decimal number written as 0.134556 times ten
    /// to the fifth, the significant digits are 0.134556 and the exponent is 5.
    /// </summary>
    private const int DoubleWidth = 53;

    /// <summary>
    /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
    /// According to the definition of Prof. Demmel and used in LAPACK and Scilab.
    /// </summary>
    public static readonly double DoublePrecision = Math.Pow(2, -DoubleWidth);

    /// <summary>
    /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
    /// According to the definition of Prof. Higham and used in the ISO C standard and MATLAB.
    /// </summary>
    public static readonly double PositiveDoublePrecision = 2 * DoublePrecision;

    /// <summary>
    /// Ten times the standard double precision, about 1.11022302462516E-15. Used as the
    /// default tolerance for approximate comparisons.
    /// </summary>
    private static readonly double DefaultDoubleAccuracy = DoublePrecision * 10;

    /// <summary>
    /// Increments a floating point number to the next bigger number representable by the data type.
    /// </summary>
    /// <param name="value">The value which needs to be incremented.</param>
    /// <param name="count">How many times the number should be incremented.</param>
    /// <remarks>
    /// The incrementation step length depends on the provided value.
    /// Increment(double.MaxValue) will return positive infinity.
    /// </remarks>
    /// <returns>The next larger floating point value.</returns>
    public static double Increment(this double value, int count = 1)
    {
        if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
        {
            return value;
        }

        if (count < 0)
        {
            return Decrement(value, -count);
        }

        // Convert the bit pattern of the double to a whole number. A
        // positive double maps to a positive whole number that grows with
        // the value. A negative double maps to a negative whole number
        // whose magnitude grows as the value approaches zero. The
        // pattern reaches its largest magnitude just below zero, at zero
        // minus one machine epsilon.
        var intValue = BitConverter.DoubleToInt64Bits(value);
        if (intValue < 0)
        {
            intValue -= count;
        }
        else
        {
            intValue += count;
        }

        // Note that long.MinValue has the same bit pattern as -0.0.
        if (intValue == long.MinValue)
        {
            return 0;
        }

        // Not every long value converts to a double value. Some long values convert to
        // infinity or NaN.
        return BitConverter.Int64BitsToDouble(intValue);
    }

    /// <summary>
    /// Decrements a floating point number to the next smaller number representable by the data type.
    /// </summary>
    /// <param name="value">The value which should be decremented.</param>
    /// <param name="count">How many times the number should be decremented.</param>
    /// <remarks>
    /// The decrementation step length depends on the provided value.
    /// Decrement(double.MinValue) will return negative infinity.
    /// </remarks>
    /// <returns>The next smaller floating point value.</returns>
    private static double Decrement(this double value, int count = 1)
    {
        if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
        {
            return value;
        }

        if (count < 0)
        {
            return Increment(value, -count);
        }

        // Convert the bit pattern of the double to a whole number. A
        // positive double maps to a positive whole number that grows with
        // the value. A negative double maps to a negative whole number
        // whose magnitude grows as the value approaches zero. The
        // pattern reaches its largest magnitude just below zero, at zero
        // minus one machine epsilon.
        var intValue = BitConverter.DoubleToInt64Bits(value);

        // When the input is zero, the method still needs the value -0.
        // The code sets the value to -0, so the rest of the logic should
        // work.
        if (intValue == 0)
        {
            // Note that long.MinValue has the same bit pattern as -0.0.
            intValue = long.MinValue;
        }

        if (intValue < 0)
        {
            intValue += count;
        }
        else
        {
            intValue -= count;
        }

        // Not every long value converts to a double value. Some long values convert to
        // infinity or NaN.
        return BitConverter.Int64BitsToDouble(intValue);
    }
}