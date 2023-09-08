using System;

namespace RunningStatistics;

/// <summary>
/// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
/// </summary>
public sealed class Extrema : AbstractRunningStatistic<double, Extrema>
{
    /// <summary>
    /// Square root of double machine precision
    /// </summary>
    private const double Tolerance = 1.4901161193847656e-8;


    public double Min { get; private set; } = double.PositiveInfinity;

    public double Max { get; private set; } = double.NegativeInfinity;

    public long MinCount { get; private set; }

    public long MaxCount { get; private set; }

    public double Range => Max - Min;

    
    public override void Fit(double value)
    {
        UncheckedFit(value, 1);
    }

    public void Fit(double value, long count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative.");
        UncheckedFit(value, count);
    }

    /// <summary>
    /// Fit the value without checking if the count is non-negative.
    /// </summary>
    private void UncheckedFit(double value, long count)
    {
        if (Nobs == 0) Min = Max = value;

        Nobs += count;
        
        if (value < Min)
        {
            Min = value;
            MinCount = count;
        }

        if (value > Max)
        {
            Max = value;
            MaxCount = count;
        }
        
        if (Math.Abs(value - Min) <= Tolerance) MinCount += count;
        if (Math.Abs(value - Max) <= Tolerance) MaxCount += count;
    }
    
    public override void Reset()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        MinCount = 0;
        MaxCount = 0;
        Nobs = 0;
    }

    public override Extrema CloneEmpty() => new();

    public override Extrema Clone()
    {
        return new Extrema
        {
            Min = Min,
            Max = Max,
            MinCount = MinCount,
            MaxCount = MaxCount,
            Nobs = Nobs
        };
    }

    public override void Merge(Extrema extrema)
    {
        Nobs += extrema.Nobs;

        if (Math.Abs(Min - extrema.Min) <= Tolerance)
        {
            MinCount += extrema.MinCount;
        }
        else if (extrema.Min < Min)
        {
            Min = extrema.Min;
            MinCount = extrema.MinCount;
        }

        if (Math.Abs(Max - extrema.Max) <= Tolerance)
        {
            MaxCount += extrema.MaxCount;
        }
        else if (extrema.Max > Max)
        {
            Max = extrema.Max;
            MaxCount = extrema.MaxCount;
        }
    }
    
    public override string ToString() => $"{typeof(Extrema)} Nobs={Nobs} | Min={Min:F2}, Max={Max:F2}, MinCount={MinCount}, MaxCount={MaxCount}";
}