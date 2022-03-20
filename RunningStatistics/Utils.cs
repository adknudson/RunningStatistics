using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

internal static class Utils
{
    internal static double BesselCorrection(long n)
    {
        return (double)n / (n - 1);
    }

    internal static IList<string> GetPrintableBins(IList<double> edges, bool left, bool closed)
    {
        var numBins = edges.Count - 1;
        IList<string> bins = new List<string>(numBins);

        int startIdx, endIdx;
        string leftBrace, rightBrace, endBrace;

        if (left)
        {
            startIdx = 0;
            endIdx = numBins - 1; // last bin is the special case
            leftBrace = "[";
            rightBrace = ")";
            endBrace = closed ? "]" : ")";
            for (var i = startIdx; i < endIdx; i++)
            {
                bins.Add($"{leftBrace}{edges[i]:F2}, {edges[i + 1]:F2}{rightBrace}");
            }
            bins.Add($"{leftBrace}{edges[numBins - 1]:F2}, {edges[numBins]:F2}{endBrace}");
        }
        else
        {
            startIdx = 1; // first bin is a special case
            endIdx = numBins;
            leftBrace = "(";
            rightBrace = "]";
            endBrace = closed ? "[" : "(";
            bins.Add($"{endBrace}{edges[0]:F2}, {edges[1]:F2}{rightBrace}");
            for (var i = startIdx; i < endIdx; i++)
            {
                bins.Add($"{leftBrace}{edges[i]:F2}, {edges[i + 1]:F2}{rightBrace}");
            }
        }

        return bins;
    }

    /// <summary>
    /// Find the midpoints of consecutive pairs in a sorted list.
    /// </summary>
    internal static IList<double> Midpoints(IList<double> xs)
    {
        IList<double> ms = new List<double>(xs.Count - 1);
        for (var i = 1; i < xs.Count; i++)
        {
            ms.Add((xs[i] + xs[i - 1]) / 2.0);
        }
        return ms;
    }

    /// <summary>
    /// Find the index of the first matching value in a list sorted from least to greatest.
    /// </summary>
    internal static int SearchSortedFirst(IList<double> values, double x)
    {
        foreach (var (a, i) in values.Select((item, index) => (item, index)))
        {
            if (a >= x)
            {
                return i;
            }
        }

        // y > a for all a in A
        return values.Count;
    }

    /// <summary>
    /// Find the index of the last matching value in a list sorted from least to greatest.
    /// </summary>
    internal static int SearchSortedLast(IList<double> values, double y)
    {
        foreach (var (a, i) in values.Select((item, index) => (item, index)))
        {
            if (a > y)
            {
                return i - 1;
            }
        }

        // y < a for all a in A
        return values.Count - 1;
    }

    /// <summary>
    /// Weighted interpolation of two values.
    /// </summary>
    internal static double Smooth(double a, double b, double w)
    {
        return a + w * (b - a);
    }

    /// <summary>
    /// Compute the (biased) variance of a collection of values.
    /// </summary>
    internal static double Variance(IList<double> xs)
    {
        var mean = xs.Average();
        var meanSquare = xs.Average(s => s * s);

        return Math.Abs(meanSquare - mean * mean);
    }
}