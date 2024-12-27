using System;
using System.Collections;
using System.Collections.Generic;

namespace RunningStatistics;

public class HistogramStepRange : IEnumerable<double>
{
    public HistogramStepRange(double start, double stop, int length)
    {
        if (length < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length), length, "Length must be greater than or equal to 2");
        }

        if (start > stop)
        {
            throw new ArgumentException("The stop must be greater than or equal to the start.");
        }
        
        Start = start;
        Stop = stop;
        Length = length;
    }
    
    
    public double Start { get; set; }
    
    public double Stop { get; set; }
    
    public int Length { get; set; }

    public double Step => (Stop - Start) / (Length - 1);

    public double Range => Stop - Start;

    
    public IEnumerator<double> GetEnumerator()
    {
        yield return Start;

        if (Length > 2)
        {
            for (var i = 1; i < Length - 1; i++)
            {
                yield return Start + Range * i / (Length - 1);
            }
        }
        
        yield return Stop;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"{Start}:{Step}:{Stop}";
}