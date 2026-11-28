using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// Represents an empirical cumulative distribution function (CDF) with a specified number of bins.
/// </summary>
public sealed class EmpiricalCdf : RunningStatisticBase<double, EmpiricalCdf>
{
    private readonly double[] _buffer;
    private readonly double[] _values;
    private readonly Extrema _extrema;

    // Where the next observation goes in the circular buffer. This is the observation
    // count modulo the bin count, kept as a field so a single fit does not pay a
    // virtual count read and a 64-bit division to find its slot. Every member that
    // moves observations through the buffer advances this cursor in step.
    private int _windowPos;
    
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

    public override long Nobs => _extrema.Nobs;
    
    /// <summary>
    /// Gets the number of bins.
    /// </summary>
    public int NumBins { get; }
    
    /// <summary>
    /// Gets the median of the empirical CDF. Returns <see cref="double.NaN"/> while the number
    /// of observations is below the bin count, the same undefined-readout value the other
    /// numeric readouts use.
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


    public override void Fit(double value)
    {
        Require.Finite(value);
        UncheckedFit(value, 1);
    }
    
    public override void UncheckedFit(double value)
    {
        UncheckedFit(value, 1);
    }

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    public override void UncheckedFit(double value, long count)
    {
        var numBins = NumBins;
        var windowPos = _windowPos;
        var valuesView = Values;
        
        // Prefix: fill the rest of the current circular-buffer window with the value.
        var prefix = Math.Min(count, (long)numBins - windowPos);
        var prefixEnd = windowPos + (int)prefix;
        for (var s = windowPos; s < prefixEnd; s++)
        {
            _buffer[s] = value;
        }
        
        _extrema.UncheckedFit(value, prefix);
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
                    valuesView[k] = Utils.Smooth(valuesView[k], _buffer[k], (double)numBins / Nobs);
                }
            }
            else
            {
                // The window is all this value, so replay's sort is an observed no-op
                // and every sorted slot equals value.
                for (var k = 0; k < numBins; k++)
                {
                    valuesView[k] = Utils.Smooth(valuesView[k], value, (double)numBins / Nobs);
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
            _extrema.UncheckedFit(value, windows * numBins);
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
                    valuesView[k] = Utils.Smooth(valuesView[k], value, alpha);
                }
            }
            
            count -= windows * numBins;
        }
        
        // Tail: a partial window, stored for a later flush. No blend fires here.
        for (var s = 0; s < (int)count; s++)
        {
            _buffer[s] = value;
        }

        _extrema.UncheckedFit(value, count);

        // The window left count slots filled when the last window closed on the
        // prefix boundary; otherwise the window stopped at prefixEnd.
        _windowPos = prefixEnd == numBins ? (int)count : prefixEnd;
    }

    public override void Reset()
    {
        for (var i = 0; i < NumBins; i++)
        {
            Values[i] = 0;
            _buffer[i] = 0;
        }

        _extrema.Reset();
        _windowPos = 0;
    }
    
    public override EmpiricalCdf CloneEmpty() => new(NumBins);

    /// <summary>
    /// Merges another empirical CDF into this empirical CDF.
    /// </summary>
    /// <param name="empiricalCdf">The other empirical CDF to merge in. Its bin count must
    /// match this empirical CDF's.</param>
    /// <remarks>
    /// The merge works only when the two objects share a bin count: the merge then blends
    /// the matching bins and replays the source's unflushed tail, and the result stays
    /// exact.
    ///
    /// When the bin counts differ, the merge throws and leaves this empirical CDF
    /// unchanged. An incompatible merge is an error, not a silent approximation. A caller
    /// who wants to combine two empirical CDFs of different sizes should use
    /// <see cref="InexactMerge" /> instead, which resamples the source's quantile curve
    /// onto this object's bin grid.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the other empirical CDF has a different number of bins. 
    /// This empirical CDF keeps its state when the merge throws.
    /// </exception>
    public override void Merge(EmpiricalCdf empiricalCdf)
    {
        // The shape check runs before any state is touched, so a rejected merge leaves
        // this object exactly as it was: an approximation of a mismatched pair belongs
        // to InexactMerge, not to a half-applied merge here.
        if (NumBins != empiricalCdf.NumBins)
        {
            throw new ArgumentException($"The two {nameof(EmpiricalCdf)} objects must have the same batch size. " +
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
            // alpha is a ratio of two non-negative flush counts, so it lies in [0, 1]
            // and the bins can blend without the per-blend probability check.
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
                Values[k] = Utils.Smooth(Values[k], _buffer[k], (double) NumBins / Nobs);
            }
        }

        // The replay placed the source's tail at the merged stream's tail positions,
        // so the next single fit belongs at the merged count's slot.
        _windowPos = (int)(Nobs % NumBins);
    }

    /// <summary>
    /// Merges another empirical CDF into this empirical CDF, approximating the other's
    /// distribution on this empirical CDF's bin grid when the two bin counts differ.
    /// </summary>
    /// <param name="empiricalCdf">The other empirical CDF to merge in. It may have any
    /// number of bins.</param>
    /// <remarks>
    /// Same bin count: this method merges exactly like <see cref="Merge" />, so the exact
    /// path pays nothing for the approximation.
    ///
    /// Different bin counts: the other's bins cannot be blended one for one, so the method
    /// reads the other's quantile curve instead. That curve is sampled at this object's
    /// own bin positions and blended in, and the other's unflushed raw tail is replayed
    /// through this object's own buffer and flush.
    ///
    /// What stays exact: the observation counts, the minimum and the maximum, and this
    /// object's own bins, which carry their own history untouched by the resampling. The
    /// replayed tail values are exact too.
    ///
    /// What is approximate: the other's distribution, because its quantile curve is only
    /// known at this object's bin positions. Between those positions the other's bin values
    /// are estimates, so readouts that land away from them inherit that estimate. The other
    /// object keeps its own state; only this object sees the approximation.
    /// </remarks>
    public void InexactMerge(EmpiricalCdf empiricalCdf)
    {
        if (NumBins == empiricalCdf.NumBins)
        {
            Merge(empiricalCdf);
            return;
        }

        // Only observations a flush has already blended into the bin arrays may carry
        // weight in this blend. Each side's slots past its own flushed region are stale,
        // and they hold zeros until that side's first flush, so both flush counts must be
        // read before anything here mutates this object or its extrema. This object's
        // flushed region runs on its own bin count, the other's on the other's.
        var otherPartial = empiricalCdf.Nobs % empiricalCdf.NumBins;
        var thisFlushed = Nobs - (Nobs % NumBins);
        var otherFlushed = empiricalCdf.Nobs - otherPartial;

        // A side with no flushed observation contributes no curve at all, so a pair that
        // has never flushed blends nothing: its whole history is raw and the replay below
        // carries it. A non-negative flush count sum also means the blend weight below is
        // a ratio of counts, hence a probability in [0, 1] that needs no per-blend check.
        if (otherFlushed > 0)
        {
            var alpha = (double)otherFlushed / (thisFlushed + otherFlushed);

            // The other's curve is sampled at this object's bin positions. Quantile maps
            // a probability onto the window position (NumBins + 1) p, so this object's
            // bin k, zero-based, sits at probability (k + 1) / (NumBins + 1). Sampling
            // that way keeps the blended bins in the same order the window needs: both
            // curves are non-decreasing, and one weight across two sorted runs leaves a
            // sorted run.
            //
            // The other's Quantile only answers once its own count reaches its own bin
            // count, which is exactly when its flushed count is positive: the guard above
            // is that precondition, and the sampled probabilities are all proper ones.
            var valuesView = Values;
            for (var k = 0; k < NumBins; k++)
            {
                var otherEstimate = empiricalCdf.Quantile((k + 1.0) / (NumBins + 1));
                valuesView[k] = Utils.Smooth(valuesView[k], otherEstimate, alpha);
            }
        }

        // Counts, min and max for all of the source's observations, including its
        // unflushed tail. The extrema carry no bin structure, so this fold stays exact
        // whatever the two bin counts are.
        _extrema.Merge(empiricalCdf._extrema);

        // Replay the source's still-unflushed buffer values through this object's own
        // buffer and flush. Their counts and extremes were already merged above, so they
        // must not be fitted to the extrema a second time here. Placing them in order
        // puts them at the tail positions of the merged observation stream, on this
        // object's bin count: a source tail longer than one of this object's windows
        // simply completes more than one window, and each of those flushes blends the
        // window the replay built.
        for (var r = 0; r < otherPartial; r++)
        {
            var i = (int)((Nobs - otherPartial + r) % NumBins);
            _buffer[i] = empiricalCdf._buffer[r];

            if (i + 1 < NumBins) continue;

            Array.Sort(_buffer);
            // A flush fires at a window's last slot, so the merged count is already at
            // least NumBins here and the weight NumBins / Nobs stays in (0, 1].
            for (var k = 0; k < NumBins; k++)
            {
                Values[k] = Utils.Smooth(Values[k], _buffer[k], (double)NumBins / Nobs);
            }
        }

        // The replay placed the source's tail at the merged stream's tail positions,
        // so the next single fit belongs at the merged count's slot.
        _windowPos = (int)(Nobs % NumBins);
    }

    /// <summary>
    /// Finds the pth quantile of the empirical CDF.
    /// </summary>
    /// <remarks>
    /// The quantile estimate needs every bin filled by at least one full flush of observations.
    /// While the number of observations is below the bin count, the estimate is undefined and
    /// the method returns <see cref="double.NaN"/>, the value numeric statistics use for
    /// undefined readouts. An invalid probability is a caller error and still throws.
    /// </remarks>
    /// <param name="p">The probability for which to find the quantile. The value must lie between 0 and 1.</param>
    /// <returns>The pth quantile, or <see cref="double.NaN"/> when the number of observations
    /// is less than the number of bins.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="p"/> is not a valid probability.</exception>
    public double Quantile(double p)
    {
        Require.ValidProbability(p);

        if (Nobs < NumBins)
        {
            return double.NaN;
        }

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
    /// <remarks>
    /// The CDF estimate needs every bin filled by at least one full flush of observations.
    /// While the number of observations is below the bin count, the estimate is undefined and
    /// the method returns <see cref="double.NaN"/>, the value numeric statistics use for
    /// undefined readouts. A NaN query is a caller error and still throws.
    /// </remarks>
    /// <param name="x">The value for which to compute the CDF.</param>
    /// <returns>The probability that X will take on a value less than or equal to x, or
    /// <see cref="double.NaN"/> when the number of observations is less than the number of bins.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="x"/> is NaN.</exception>
    public double Cdf(double x)
    {
        Require.NotNaN(x);
        
        if (Nobs < NumBins)
        {
            return double.NaN;
        }
        
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
        return Utils.Smooth(i - 1, i, r) / _values.Length;
    }

    public override string ToString() => base.ToString() + $" | NumBins={NumBins}, Min={Min}, Max={Max}";
}
