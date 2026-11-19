using System;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace RunningStatistics;

/// <summary>
/// Represents an empirical cumulative distribution function (CDF) with a specified number of bins.
/// </summary>
public sealed class EmpiricalCdf : RunningStatisticBase<double, EmpiricalCdf>
{
    private readonly double[] _buffer;
    private readonly double[] _values;
    private readonly Extrema _extrema;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmpiricalCdf"/> class with the specified number of bins.
    /// </summary>
    /// <param name="numBins">The number of bins to use. Must be >= 2.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numBins"/> is less than 2.</exception>
    public EmpiricalCdf(int numBins = 200)
    {
        if (numBins < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numBins), numBins, "The number of bins must be >=2.");
        }
        
        NumBins = numBins;
        _buffer = new double[NumBins];
        _values = new double[NumBins + 2];
        _extrema = new Extrema();
    }


    /// <summary>
    /// A reference to the values excluding the min and max. The min and max are handled by the
    /// <see cref="Extrema"/> object.
    /// </summary>
    private Span<double> Values => _values.AsSpan(1, NumBins);
    
    /// <summary>
    /// Gets the number of bins.
    /// </summary>
    private int NumBins { get; }
    
    /// <summary>
    /// Gets the median of the empirical CDF.
    /// </summary>
    public double Median => Quantile(0.5);
    
    /// <summary>
    /// Gets the minimum value observed.
    /// </summary>
    public double Min => _extrema.Min;
    
    /// <summary>
    /// Gets the maximum value observed.
    /// </summary>
    public double Max => _extrema.Max;

    
    protected override long GetNobs() => _extrema.Nobs;

    public override void Fit(double value)
    {
        Require.Finite(value);
        UncheckedFit(value);
    }

    /// <summary>
    /// Fits the value a given number of times, replaying it one observation at a time.
    ///
    /// <b>The cost of this overload grows with the count: it pays one fitting step per
    /// observation, not one step per call.</b> That sets it apart from the weighted overloads of
    /// other statistics, such as the extrema or the moments, which fold a count straight into
    /// their state in closed form. This one cannot: when the internal buffer flushes, each bin
    /// is blended with a weight equal to the batch size divided by the running observation
    /// count, and that count advances with every replayed observation inside the batch. So
    /// different replays of the same call would blend with different weights, and no shortcut
    /// can serve a whole batch without changing what a flush means.
    ///
    /// Because of that, a weighted fit leaves exactly the same state, bit for bit, as a loop of
    /// the same number of single-observation fits in the same order: the replay performs the
    /// same arithmetic at the same points, one observation at a time.
    /// </summary>
    /// <param name="value">The value to fit. Must be finite.</param>
    /// <param name="count">The number of times to fit <paramref name="value"/>. Must be non-negative.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is NaN or infinite.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        
        if (count == 0) return;
        
        for (var i = 0; i < count; i++)
        {
            UncheckedFit(value);
        }
    }

    private void UncheckedFit(double value)
    {
        var i = Nobs % NumBins;
        var bufferCount = i + 1;
        _buffer[i] = value;
        
        _extrema.Fit(value);

        if (bufferCount < NumBins) return;
        
        Array.Sort(_buffer);
        
        for (var k = 0; k < NumBins; k++)
        {
            Values[k] = Utils.Smooth(Values[k], _buffer[k], (double) NumBins / Nobs);
        }
    }

    public override void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            Values[i] = 0;
            _buffer[i] = 0;
        }

        _extrema.Reset();
    }
    
    public override EmpiricalCdf CloneEmpty() => new(NumBins);

    public override void Merge(EmpiricalCdf empiricalCdf)
    {
        if (NumBins != empiricalCdf.NumBins)
        {
            throw new Exception($"The two {nameof(EmpiricalCdf)} objects must have the same batch size. " +
                                $"Got {NumBins} and {empiricalCdf.NumBins}.");
        }

        // Only observations fully flushed into each object's Values array may carry
        // weight in the blend. Slots of empiricalCdf beyond its flushed region
        // are stale, and they hold zeros until the first flush. This object
        // must instead replay observations still sitting in that buffer, rather
        // than represent them through those slots.
        var otherPartial = empiricalCdf.Nobs % NumBins;
        var thisFlushed = Nobs - (Nobs % NumBins);
        var otherFlushed = empiricalCdf.Nobs - otherPartial;

        var flushedTotal = thisFlushed + otherFlushed;
        if (flushedTotal > 0)
        {
            var alpha = (double) otherFlushed / flushedTotal;
            for (var k = 0; k < NumBins; k++)
            {
                Values[k] = Utils.Smooth(Values[k], empiricalCdf.Values[k], alpha);
            }
        }

        // Counts, min and max for all of the source's observations, including its unflushed tail.
        _extrema.Merge(empiricalCdf._extrema);

        // Replay the source's still-unflushed buffer values through this object's own buffer
        // and quantile-value update. Their counts and extremes were already merged above, so
        // they must not be re-fitted to the extremes here. Placing them in order puts them at
        // the tail positions of the merged observation stream.
        for (var r = 0; r < otherPartial; r++)
        {
            var i = (int) ((Nobs - otherPartial + r) % NumBins);
            _buffer[i] = empiricalCdf._buffer[r];

            if (i + 1 < NumBins) continue;

            Array.Sort(_buffer);
            for (var k = 0; k < NumBins; k++)
            {
                Values[k] = Utils.Smooth(Values[k], _buffer[k], (double) NumBins / Nobs);
            }
        }
    }
    
    /// <summary>
    /// Finds the pth quantile of the empirical CDF.
    /// </summary>
    /// <param name="p">The probability for which to find the quantile. Must be between 0 and 1.</param>
    /// <returns>The pth quantile.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the number of observations is less than the number of bins.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="p"/> is not a valid probability.</exception>
    public double Quantile(double p)
    {
        if (Nobs < NumBins)
        {
            throw new InvalidOperationException(
                $"The number of observations must be at least {NumBins} to compute the quantile.");
        }
        
        Require.ValidProbability(p);

        if (p == 0)
        {
            return Min;
        }

        if (p == 1)
        {
            return Max;
        }

        // The interpolation window: the minimum, the bins in order, then the maximum.
        var i = (NumBins - 1 + 2) * p;
        var r = i % 1; // fractional part of i
        
        // A position below the first bin interpolates between the minimum and the first bin.
        if (i < 1)
        {
            return Utils.Smooth(Min, Values[0], r);
        }
        
        // A position past the last bin interpolates between the last bin and the maximum.
        if (i >= NumBins)
        {
            // ReSharper disable once UseIndexFromEndExpression
            return Utils.Smooth(Values[Values.Length - 1], Max, r);
        }
        
        // Otherwise the position falls between two neighboring bins. Interpolate between them.
        var fi = (int)Math.Floor(i) - 1;
        var ci = (int)Math.Ceiling(i) - 1;
        return Utils.Smooth(Values[fi], Values[ci], r);
    }

    /// <summary>
    /// The probability that X will take on a value less than or equal to x.
    /// </summary>
    /// <param name="x">The value for which to compute the CDF.</param>
    /// <returns>The probability that X will take on a value less than or equal to x.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the number of observations is less than the number of bins.</exception>
    public double Cdf(double x)
    {
        if (Nobs < NumBins)
        {
            throw new InvalidOperationException(
                $"The number of observations must be at least {NumBins} to compute the CDF.");
        }
        
        Require.NotNaN(x);
        
        if (x < Min)
        {
            return 0;
        }
        
        if (x >= Max)
        {
            return 1;
        }
        
        // set the min and max values. The rest are already set due to the Values span.
        _values[0] = Min;
        // ReSharper disable once UseIndexFromEndExpression
        _values[_values.Length - 1] = Max;
        
        // find the index of the first value greater than x
        // The query sits between that value and the window value just before it.
        var i = Array.FindIndex(_values, v => v > x);
        
        // The query now lies between the window value just below it and the one above it.
        
        // How far the query sits through that span, as a fraction of the span's width.
        var r = (x - _values[i - 1]) / (_values[i] - _values[i - 1]);
        
        // Interpolate between the two window positions and scale by the window length.
        return Utils.Smooth(i - 1, i, r) / _values.Length;
    }

    public override string ToString() => base.ToString() + $" | NumBins={NumBins}, Min={Min}, Max={Max}";
}
