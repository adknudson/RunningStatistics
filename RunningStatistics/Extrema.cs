using System;
using System.Collections.Generic;
using System.IO;

namespace RunningStatistics;

/// <summary>
/// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
/// </summary>
public class Extrema : IRunningStatistic<double, Extrema>
{
    private const double Tolerance = 1.4901161193847656e-8;


    public Extrema()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        CountMin = 0;
        CountMax = 0;
    }
    
    public Extrema(Extrema a)
    {
        Min = a.Min;
        Max = a.Max;
        CountMin = a.CountMin;
        CountMax = a.CountMax;
    }


    public int CountMin { get; private set; }
    public int CountMax { get; private set; }
    public double Max { get; private set; }
    public double Min { get; private set; }
    public double Range => Max - Min;
    public long Count { get; private set; }
    
    
    
    public void Fit(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            Fit(value);
        }
    }

    public void Fit(double value)
    {
        Count++;

        if (value < Min)
        {
            Min = value;
            CountMin = 1;
        }
        
        if (value > Max)
        {
            Max = value;
            CountMax = 1;
        }
    
    
        if (Math.Abs(value - Min) < Tolerance)
        {
            CountMin++;
        }
    
        if (Math.Abs(value - Max) < Tolerance)
        {
            CountMax++;
        }
    }
    
    public void Merge(Extrema other)
    {
        Count += other.Count;
        
        if (Math.Abs(Min - other.Min) < Tolerance)
        {
            CountMin += other.CountMin;
        }
        else if (other.Min < Min)
        {
            Min = other.Min;
            CountMin = other.CountMin;
        }
        
        if (Math.Abs(Max - other.Max) < Tolerance)
        {
            CountMax += other.CountMax;
        }
        else if (other.Max > Max)
        {
            Max = other.Max;
            CountMax = other.CountMax;
        }
    }

    public void Reset()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        CountMin = 0;
        CountMax = 0;
    }

    public override string ToString() => $"{typeof(Extrema)}(min={Min:F2}, max={Max:F2}, n={Count})";
    
    private static Extrema Merge(Extrema a, Extrema b)
    {
        Extrema merged = new(a);
        merged.Merge(b);
        return merged;
    }
}