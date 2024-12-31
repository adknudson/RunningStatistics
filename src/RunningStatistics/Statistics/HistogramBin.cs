using System;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertIfStatementToReturnStatement

namespace RunningStatistics;

public sealed class HistogramBin : IEquatable<HistogramBin>
{
    public HistogramBin(double lower, double upper, bool closedLeft, bool closedRight)
    {
        if (lower >= upper)
        {
            throw new ArgumentException(
                $"Lower bound must be strictly less than upper bound. Lower: {lower}, Upper: {upper}");
        }
        
        if (double.IsInfinity(lower) && double.IsInfinity(upper))
        {
            throw new ArgumentException("Both bounds cannot be infinite");
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

    // Midpoint is allowed to be +/- infinity
    public double Midpoint
    {
        get
        {
            if (double.IsInfinity(Lower)) return Lower;
            if (double.IsInfinity(Upper)) return Upper;
            return Lower + (Upper - Lower) / 2;
        }
    }


    public bool Contains(double value)
    {
        // strictly outside of the bin
        if (value < Lower || value > Upper) return false;
        // strictly inside of the bin
        if (Lower < value && value < Upper) return true;
        
        // potentially on the boundary
        if (value == Lower) return ClosedLeft;
        if (value == Upper) return ClosedRight;

        // should never reach here
        throw new InvalidOperationException("Unexpected state");
    }

    public void Reset()
    {
        Nobs = 0;
    }

    public void Merge(HistogramBin other)
    {
        Nobs += other.Nobs;
    }

    public void Increment(long count)
    {
        Require.NonNegative(count);
        Nobs += count;
    }

    public bool Equals(HistogramBin? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Lower == other.Lower
               && Upper == other.Upper
               && ClosedLeft == other.ClosedLeft
               && ClosedRight == other.ClosedRight;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is HistogramBin other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Lower.GetHashCode();
            hashCode = (hashCode * 397) ^ Upper.GetHashCode();
            hashCode = (hashCode * 397) ^ ClosedLeft.GetHashCode();
            hashCode = (hashCode * 397) ^ ClosedRight.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(HistogramBin? left, HistogramBin? right) => Equals(left, right);

    public static bool operator !=(HistogramBin? left, HistogramBin? right) => !Equals(left, right);

    public override string ToString()
    {
        return $"{typeof(HistogramBin)} Nobs={Nobs} | Bin={BinName}";
    }
}