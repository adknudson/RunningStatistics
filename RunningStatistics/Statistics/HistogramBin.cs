using System;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertIfStatementToReturnStatement

namespace RunningStatistics;

/// <summary>
/// Represents a bin in a histogram.
/// </summary>
public sealed class HistogramBin : IEquatable<HistogramBin>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HistogramBin"/> class.
    /// </summary>
    /// <param name="lower">The lower bound of the bin.</param>
    /// <param name="upper">The upper bound of the bin.</param>
    /// <param name="closedLeft">Indicates if the bin is closed on the left side.</param>
    /// <param name="closedRight">Indicates if the bin is closed on the right side.</param>
    /// <exception cref="ArgumentException">Thrown when the lower bound is not strictly less than
    /// the upper bound or both bounds are infinite.</exception>
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
        BinRep = $"{leftBrace}{Lower}, {Upper}{rightBrace}";

        Nobs = 0;
    }

    
    /// <summary>
    /// Gets the number of observations in the bin.
    /// </summary>
    public long Nobs { get; private set; }

    /// <summary>
    /// Gets the representation of the bin. For example, a bin with lower bound 0 and upper bound 1
    /// with closed left and open right would be represented as "[0.00, 1.00)".
    /// </summary>
    private string BinRep { get; }

    /// <summary>
    /// Gets the lower bound of the bin.
    /// </summary>
    public double Lower { get; }

    /// <summary>
    /// Gets the upper bound of the bin.
    /// </summary>
    public double Upper { get; }

    /// <summary>
    /// Gets a value indicating whether the bin is closed on the left side.
    /// </summary>
    public bool ClosedLeft { get; }

    /// <summary>
    /// Gets a value indicating whether the bin is closed on the right side.
    /// </summary>
    public bool ClosedRight { get; }

    /// <summary>
    /// Gets the midpoint of the bin. If either the lower or upper bound is infinite, then midpoint
    /// is set to the infinite bound.
    /// </summary>
    public double Midpoint
    {
        get
        {
            if (double.IsInfinity(Lower)) return Lower;
            if (double.IsInfinity(Upper)) return Upper;
            return Lower + (Upper - Lower) / 2;
        }
    }


    /// <summary>
    /// Determines whether the specified value falls within the bin.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the bin contains the value, otherwise false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unexpected state is encountered.</exception>

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

    /// <summary>
    /// Resets the number of observations in the bin.
    /// </summary>
    public void Reset()
    {
        Nobs = 0;
    }

    /// <summary>
    /// Merges another bin into this bin.
    /// </summary>
    /// <param name="other">The other bin to merge.</param>
    public void Merge(HistogramBin other)
    {
        Nobs += other.Nobs;
    }

    /// <summary>
    /// Increments the number of observations in the bin by the specified count.
    /// </summary>
    /// <param name="count">The count to increment by.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the count is negative.</exception>
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
        return $"{nameof(HistogramBin)}(Nobs={Nobs:N0}) | Bin={BinRep}";
    }
}