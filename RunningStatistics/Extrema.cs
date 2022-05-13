using System;
using System.Collections.Generic;
using System.IO;

namespace RunningStatistics;

/// <summary>
/// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
/// </summary>
public class Extrema : IRunningStatistic<double>
{
    private const double Tolerance = 1.4901161193847656e-8;


    public Extrema()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        CountMin = 0;
        CountMax = 0;
    }

    public Extrema(Extrema other)
    {
        Min = other.Min;
        Max = other.Max;
        CountMin = other.CountMin;
        CountMax = other.CountMax;
        Count = other.Count;
    }
    

    public nint CountMin { get; private set; }
    public nint CountMax { get; private set; }
    public double Max { get; private set; }
    public double Min { get; private set; }
    public double Range => Max - Min;
    public nint Count { get; private set; }


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

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Extrema extrema) return;

        Count += extrema.Count;

        if (Math.Abs(Min - extrema.Min) < Tolerance)
        {
            CountMin += extrema.CountMin;
        }
        else if (extrema.Min < Min)
        {
            Min = extrema.Min;
            CountMin = extrema.CountMin;
        }

        if (Math.Abs(Max - extrema.Max) < Tolerance)
        {
            CountMax += extrema.CountMax;
        }
        else if (extrema.Max > Max)
        {
            Max = extrema.Max;
            CountMax = extrema.CountMax;
        }
    }

    public static Extrema Merge(Extrema a, Extrema b)
    {
        var c = new Extrema(a);
        c.Merge(b);
        return c;
    }

    public void Reset()
    {
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
        CountMin = 0;
        CountMax = 0;
    }

    public override string ToString() => $"{typeof(Extrema)}(min={Min:F2}, max={Max:F2}, n={Count})";

    public void Print(StreamWriter stream)
    {
        stream.WriteLine($"{GetType()}(Min={Min}, Max={Max}, CountMin={CountMin}, CountMax={CountMax}, n={Count}");
    }
}