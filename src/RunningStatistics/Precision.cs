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
    /// The number of binary digits used to represent the binary number for a double precision floating
    /// point value. i.e. there are this many digits used to represent the
    /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
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
    /// Value representing 10 * 2^(-53) = 1.11022302462516E-15
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

        // Translate the bit pattern of the double to an integer.
        // Note that this leads to:
        // double > 0 --> long > 0, growing as the double value grows
        // double < 0 --> long < 0, increasing in absolute magnitude as the double
        //                          gets closer to zero!
        //                          i.e. 0 - double.epsilon will give the largest long value!
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

        // Note that not all long values can be translated into double values. There's a whole bunch of them
        // which return weird values like infinity and NaN
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

        // Translate the bit pattern of the double to an integer.
        // Note that this leads to:
        // double > 0 --> long > 0, growing as the double value grows
        // double < 0 --> long < 0, increasing in absolute magnitude as the double
        //                          gets closer to zero!
        //                          i.e. 0 - double.epsilon will give the largest long value!
        var intValue = BitConverter.DoubleToInt64Bits(value);

        // If the value is zero then we'd really like the value to be -0. So we'll make it -0
        // and then everything else should work out.
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

        // Note that not all long values can be translated into double values. There's a whole bunch of them
        // which return weird values like infinity and NaN
        return BitConverter.Int64BitsToDouble(intValue);
    }
}