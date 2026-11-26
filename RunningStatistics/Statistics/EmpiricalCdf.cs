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
    /// A reference to the values excluding the min and max. The <see cref="Extrema"/> object
    /// handles the min and max.
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
    /// Fits the value the given number of times.
    ///
    /// <b>The cost of this overload grows with count / NumBins, not with count.</b> A loop of
    /// single fits pays one step per observation. This overload folds that step and blends the
    /// bins once per buffer flush.
    ///
    /// The call leaves exactly the same state, bit for bit, as a loop of the same number of
    /// single-observation fits in the same order.
    /// </summary>
    /// <remarks>
    /// Each flush blends the bins with NumBins / Nobs, evaluated with the observation count
    /// at that flush. That count advances inside a batch, so flushes inside one call blend
    /// with different weights, and no single closed-form blend can replace the chain. The
    /// method therefore replays the blend chain, one blend per flush, in the order the
    /// flushes would happen. That blend order is what keeps the state bitwise exact. What
    /// the batching skips is only the bookkeeping a constant run repeats per observation:
    /// buffer index math, the sort of a window holding one repeated value, and the
    /// per-observation extrema update. The extrema fold of a constant segment is proven
    /// bitwise equal to the single-fit loop by Gate_ExtremaWeightedFitBitwiseEqualsLoop.
    /// </remarks>
    /// <param name="value">The value to fit. Must be finite.</param>
    /// <param name="count">The number of times to fit <paramref name="value"/>. Must be non-negative.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is NaN or infinite.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        
        if (count == 0) return;
        
        var numBins = NumBins;
        var windowPos = (int)(Nobs % numBins);
        var valuesView = Values;
        
        // Prefix: fill the rest of the current circular-buffer window with the value.
        var prefix = Math.Min(count, (long)numBins - windowPos);
        var prefixEnd = windowPos + (int)prefix;
        for (var s = windowPos; s < prefixEnd; s++)
        {
            _buffer[s] = value;
        }
        
        _extrema.Fit(value, prefix);
        count -= prefix;
        
        if (prefixEnd == numBins)
        {
            // The prefix completed a window, so replay blends now. Completing the window
            // has advanced Nobs to a positive multiple of numBins, so the blend weight
            // numBins / Nobs lies in (0, 1] and needs no per-blend probability check.
            if (windowPos != 0)
            {
                // The window mixes slots this call did not write, so replay sorts them.
                Array.Sort(_buffer);
                for (var k = 0; k < numBins; k++)
                {
                    valuesView[k] = Utils.SmoothUnchecked(valuesView[k], _buffer[k], (double)numBins / Nobs);
                }
            }
            else
            {
                // The window is all this value, so replay's sort is an observed no-op
                // and every sorted slot equals value.
                for (var k = 0; k < numBins; k++)
                {
                    valuesView[k] = Utils.SmoothUnchecked(valuesView[k], value, (double)numBins / Nobs);
                }
            }
        }
        
        // Full windows: every whole window is this value repeated, so replay's sort is a
        // no-op and the blend uses the value directly. The blends stay one per flush, in
        // replay order; that sequential chain is what keeps the result bitwise exact. The
        // extrema fold all whole windows at once, a form proven bitwise equal to the
        // single-fit loop by the Extrema gate (Gate_ExtremaWeightedFitBitwiseEqualsLoop).
        var windows = count / numBins;
        if (windows > 0)
        {
            var flushed = Nobs;
            _extrema.Fit(value, windows * numBins);
            for (var j = 1L; j <= windows; j++)
            {
                // The weight's denominator is flushed plus at least one window of
                // observations, never below numBins, so alpha stays in (0, 1].
                var alpha = (double)numBins / (flushed + j * (long)numBins);
                for (var s = 0; s < numBins; s++)
                {
                    _buffer[s] = value;
                }
                
                for (var k = 0; k < numBins; k++)
                {
                    valuesView[k] = Utils.SmoothUnchecked(valuesView[k], value, alpha);
                }
            }
            
            count -= windows * numBins;
        }
        
        // Tail: a partial window, stored for a later flush. No blend fires here.
        for (var s = 0; s < (int)count; s++)
        {
            _buffer[s] = value;
        }
        
        _extrema.Fit(value, count);
    }

    private void UncheckedFit(double value)
    {
        var i = Nobs % NumBins;
        var bufferCount = i + 1;
        _buffer[i] = value;
        
        _extrema.Fit(value);

        if (bufferCount < NumBins) return;
        
        Array.Sort(_buffer);
        
        // The flush fired only when the observation count reached a multiple of the
        // bin count, so NumBins / Nobs is a weight in (0, 1].
        for (var k = 0; k < NumBins; k++)
        {
            Values[k] = Utils.SmoothUnchecked(Values[k], _buffer[k], (double) NumBins / Nobs);
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
            // A flush fires at a window's last slot, so the merged count is already at
            // least NumBins here and the weight NumBins / Nobs stays in (0, 1].
            for (var k = 0; k < NumBins; k++)
            {
                Values[k] = Utils.SmoothUnchecked(Values[k], _buffer[k], (double) NumBins / Nobs);
            }
        }
    }
    
    /// <summary>
    /// Finds the pth quantile of the empirical CDF.
    /// </summary>
    /// <param name="p">The probability for which to find the quantile. The value must lie between 0 and 1.</param>
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
        // p is a proven probability here, so i is non-negative and its fractional part
        // r is in [0, 1): the three blends below may skip the probability check.
        var i = (NumBins - 1 + 2) * p;
        var r = i % 1; // fractional part of i
        
        // A position below the first bin interpolates between the minimum and the first bin.
        if (i < 1)
        {
            return Utils.SmoothUnchecked(Min, Values[0], r);
        }
        
        // A position past the last bin interpolates between the last bin and the maximum.
        if (i >= NumBins)
        {
            // ReSharper disable once UseIndexFromEndExpression
            return Utils.SmoothUnchecked(Values[Values.Length - 1], Max, r);
        }
        
        // Otherwise the position falls between two neighboring bins. Interpolate between them.
        var fi = (int)Math.Floor(i) - 1;
        var ci = (int)Math.Ceiling(i) - 1;
        return Utils.SmoothUnchecked(Values[fi], Values[ci], r);
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
        
        // Upper-bound binary search for the index of the first value greater
        // than x. The window between the stamped slots never decreases: each
        // flush blends two sorted runs with one weight through the monotone
        // Smooth, so equal-weight blends keep it sorted, and Min and Max
        // bracket every window value. The stamped maximum at the tail sits
        // above x, so a greater slot always exists and the search ends at the
        // first one, the same index a full scan answers, duplicates included.
        var low = 0;
        var high = _values.Length - 1;
        while (low < high)
        {
            var mid = low + (high - low) / 2;
            if (_values[mid] > x)
            {
                high = mid;
            }
            else
            {
                low = mid + 1;
            }
        }

        var i = low;
        
        // The query now lies between the window value just below it and the one above it.
        
        // How far the query sits through that span, as a fraction of the span's width.
        // The search left _values[i - 1] <= x < _values[i], so r is a non-negative gap
        // over a not-smaller one: a fraction in [0, 1], blendable unchecked.
        var r = (x - _values[i - 1]) / (_values[i] - _values[i - 1]);
        
        // Interpolate between the two window positions and scale by the window length.
        return Utils.SmoothUnchecked(i - 1, i, r) / _values.Length;
    }

    public override string ToString() => base.ToString() + $" | NumBins={NumBins}, Min={Min}, Max={Max}";
}
