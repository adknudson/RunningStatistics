﻿using System;

namespace RunningStatistics;

public sealed class HistogramBin
{
    internal HistogramBin(double lower, double upper, bool closedLeft, bool closedRight)
    {
        if (lower >= upper)
        {
            throw new ArgumentException("Lower bound must be strictly less than upper bound");
        }

        Lower = lower;
        Upper = upper;
        ClosedLeft = closedLeft;
        ClosedRight = closedRight;

        var leftBrace = ClosedLeft ? '[' : '(';
        var rightBrace = ClosedRight ? ']' : ')';
        BinName = $"{leftBrace}{Lower:F2}, {Upper:F2}{rightBrace}";

        Nobs = 0;
    }

    

    public long Nobs { get; private set; }

    public string BinName { get; }

    public double Lower { get; }

    public double Upper { get; }

    public bool ClosedLeft { get; }

    public bool ClosedRight { get; }

    // TODO: Check behavior when ends are +/-Inf
    internal double Midpoint => (Lower + Upper) / 2;


    internal bool Contains(double value)
    {
        if (value < Lower || value > Upper) return false;
        if (Lower < value && value < Upper) return true;
        
        if (value.Equals(Lower)) return ClosedLeft;
        if (value.Equals(Upper)) return ClosedRight;

        return false;
    }

    internal void Reset()
    {
        Nobs = 0;
    }

    internal void Merge(HistogramBin other)
    {
        Nobs += other.Nobs;
    }

    internal void Increment(long count)
    {
        Nobs += count;
    }

    public override string ToString()
    {
        return $"{typeof(HistogramBin)} Nobs={Nobs} | Bin={BinName}";
    }

    public bool Equals(HistogramBin other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Lower.Equals(other.Lower)
               && Upper.Equals(other.Upper)
               && ClosedLeft == other.ClosedLeft
               && ClosedRight == other.ClosedRight;
    }
}