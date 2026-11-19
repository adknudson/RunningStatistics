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
    /// Fits <paramref name="value"/> <paramref name="count"/> times, replaying it observation by
    /// observation.
    ///
    /// <b>Cost: O(<paramref name="count"/>) — per observation, not O(1) in the count.</b> Unlike the
    /// other statistics' weighted overloads (e.g. <see cref="Extrema"/> or <see cref="Moments"/>,
    /// which fold a count into their state in closed form), this one replays the observation one
    /// <see cref="Fit(double)"/>-sized step at a time, so a large <paramref name="count"/> costs
    /// proportionally more. It cannot be batched: when a buffer flushes, the blend weight applied
    /// to each bin is <c>alpha = NumBins / Nobs</c>, which depends on the running observation
    /// count — and that count advances with every replay inside the batch, so different replays
    /// of the same call get different alphas.
    ///
    /// Because of that, the weighted fit is equivalent to a loop of <paramref name="count"/>
    /// identical single-observation fits, bitwise: the replay performs the same arithmetic in the
    /// same order, one observation at a time.
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

        // Only observations fully flushed into each object's Values array may carry weight in
        // the blend: slots of empiricalCdf beyond its flushed region are stale (zeros until its
        // first flush), and observations still sitting in its buffer have to be replayed into
        // this object instead of being represented by those slots.
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

        // Replay the source's not-yet-flushed buffer values through this object's buffer and
        // quantile-value update (the buffer half of UncheckedFit): their counts and min/max are
        // already accounted for by the extrema merge above, so they must not be fitted to the
        // extrema again. They belong at the tail positions of the merged observation stream.
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

        // [min, V[0], V[1], ..., V[NumBins - 1], max]
        var i = (NumBins - 1 + 2) * p;
        var r = i % 1; // fractional part of i
        
        // if i is less than 1, return the interpolation between min and V[0]
        if (i < 1)
        {
            return Utils.Smooth(Min, Values[0], r);
        }
        
        // if i is greater than NumBins, return the interpolation between V[^1] and max
        if (i >= NumBins)
        {
            // ReSharper disable once UseIndexFromEndExpression
            return Utils.Smooth(Values[Values.Length - 1], Max, r);
        }
        
        // otherwise, return the interpolation between V[floor(i)] and V[ceil(i)]
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
        // ___v[i]___x___v[i+1]___
        var i = Array.FindIndex(_values, v => v > x);
        
        // x is now between v[i-1] and v[i]
        
        // get the fractional amount between v[i-1] and v[i]
        var r = (x - _values[i - 1]) / (_values[i] - _values[i - 1]);
        
        // return the interpolation between v[i-1] and v[i]
        return Utils.Smooth(i - 1, i, r) / _values.Length;
    }

    public override string ToString() => base.ToString() + $" | NumBins={NumBins}, Min={Min}, Max={Max}";
}
