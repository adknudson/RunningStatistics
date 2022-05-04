using System.IO;
using System.Linq;

namespace RunningStatistics.Printing;

public static class PrintExtensions
{
    private static void PrintType<T>(IRunningStatistic<T> stat, TextWriter stream)
    {
        stream.WriteLine($"{stat.GetType()}(n={stat.Count})");
    }
    
    public static void Print(this Mean stat, StreamWriter stream)
    {
        stream.WriteLine($"{stat.GetType()}(μ={(double)stat}, n={stat.Count})");
    }

    public static void Print(this Sum stat, StreamWriter stream)
    {
        stream.WriteLine($"{stat.GetType()}(Σ={(double)stat}, n={stat.Count})");
    }

    public static void Print(this Variance stat, StreamWriter stream)
    {
        stream.WriteLine($"{stat.GetType()}(σ²={(double)stat}, n={stat.Count})");
    }

    public static void Print<T>(this Countmap<T> stat, StreamWriter stream, char sep = '\t')
    {
        PrintType(stat, stream);
        stream.WriteLine($"Key{sep}Count");
        foreach (var (key, count) in stat.AsSortedDictionary())
        {
            stream.WriteLine($"{key}{sep}{count}");
        }
    }

    public static void Print(this Extrema stat, StreamWriter stream)
    {
        stream.WriteLine($"{stat.GetType()}(Min={stat.Min}, Max={stat.Max}, CountMin={stat.CountMin}, CountMax={stat.CountMax}, n={stat.Count}");
    }

    public static void Print(this EmpiricalCdf stat, StreamWriter stream, int numQuantiles, char sep = '\t')
    {
        var quantiles = Enumerable.Range(0, numQuantiles)
            .Select(p => (double) p / (numQuantiles - 1))
            .ToList();
        var values = quantiles.Select(stat.Quantile);
        
        PrintType(stat, stream);
        foreach (var (q, x) in quantiles.Zip(values))
        {
            stream.WriteLine($"{q:F3}{sep}{x}");
        }
    }

    public static void Print(this EmpiricalCdf stat, StreamWriter stream, char sep = '\t')
    {
        Print(stat, stream, stat.NumBins, sep);
    }

    public static void Print(this Histogram stat, StreamWriter stream, char sep = '\t')
    {
        PrintType(stat, stream);
        stream.WriteLine($"Bin{sep}Count");
        foreach (var (bin, count) in stat)
        {
            stream.WriteLine($"{bin}{sep}{count}");
        }
    }

    public static void Print(this Moments stat, StreamWriter stream)
    {
        PrintType(stat, stream);
        stream.WriteLine($"\tMean={stat.Mean}");
        stream.WriteLine($"\tVariance={stat.Variance}");
        stream.WriteLine($"\tSkewness={stat.Skewness}");
        stream.WriteLine($"\tKurtosis={stat.Kurtosis}");
    }

    public static void Print<T>(this Series<T> series, StreamWriter stream)
    {
        foreach (dynamic stat in series)
        {
            stat.Print(stream);
            stream.WriteLine();
        }
    }
}