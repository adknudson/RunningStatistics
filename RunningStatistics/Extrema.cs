using System;
using System.Collections.Generic;

namespace RunningStatistics;

/// <summary>
/// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
/// </summary>
public class Extrema : IRunningStatistic<double, (double Min, long MinCount, double Max, long MaxCount), Extrema>
{
    private const double Tolerance = 1.4901161193847656e-8;


    
    public Extrema()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        MinCount = 0;
        MaxCount = 0;
    }



    public long MinCount { get; private set; }
    
    public long MaxCount { get; private set; }
    
    public double Max { get; private set; }
    
    public double Min { get; private set; }
    
    public double Range => Max - Min;
    
    public long Nobs { get; private set; }

    public (double Min, long MinCount, double Max, long MaxCount) Value => (Min, MinCount, Max, MaxCount);

    

    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value, long k)
    {
        Nobs += k;
        
        if (value < Min)
        {
            Min = value;
            MinCount = k;
        }

        if (value > Max)
        {
            Max = value;
            MaxCount = k;
        }


        if (Math.Abs(value - Min) < Tolerance)
        {
            MinCount += k;
        }

        if (Math.Abs(value - Max) < Tolerance)
        {
            MaxCount += k;
        }
    }

    public void Fit(double value)
    {
        Fit(value, 1);
    }

    public void Merge(Extrema extrema)
    {
        Nobs += extrema.Nobs;

        if (Math.Abs(Min - extrema.Min) < Tolerance)
        {
            MinCount += extrema.MinCount;
        }
        else if (extrema.Min < Min)
        {
            Min = extrema.Min;
            MinCount = extrema.MinCount;
        }

        if (Math.Abs(Max - extrema.Max) < Tolerance)
        {
            MaxCount += extrema.MaxCount;
        }
        else if (extrema.Max > Max)
        {
            Max = extrema.Max;
            MaxCount = extrema.MaxCount;
        }
    }

    public static Extrema Merge(Extrema first, Extrema second)
    {
        var stat = first.CloneEmpty();
        stat.Merge(first);
        stat.Merge(second);
        return stat;
    }

    public void Reset()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        MinCount = 0;
        MaxCount = 0;
    }

    public Extrema CloneEmpty()
    {
        return new Extrema();
    }

    public Extrema Clone()
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

    public override string ToString() => $"{typeof(Extrema)}(min={Min:F2}, max={Max:F2}, n={Nobs})";
}