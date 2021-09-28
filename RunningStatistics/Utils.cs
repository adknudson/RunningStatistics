using System;
using System.Collections.Generic;

namespace RunningStatistics
{
    internal static class Utils
    {
        internal static double Mean(IList<double> xs)
        {
            double sum = 0.0;
            int n = xs.Count;
            foreach(double x in xs)
            {
                sum += x;
            }
            return sum / n;
        }

        internal static double Sum(IList<double> xs)
        {
            double sum = 0.0;
            foreach (double x in xs)
            {
                sum += x;
            }
            return sum;
        }

        internal static double Variance(IList<double> xs)
        {
            double mean = 0.0;
            double meanSquare = 0.0;
            int n = 0;
            double inv_n;

            foreach (double x in xs)
            {
                ++n;
                inv_n = 1.0 / n;
                mean = Smooth(mean, x, inv_n);
                meanSquare = Smooth(meanSquare, x * x, inv_n);
            }

            return Math.Abs(meanSquare - mean * mean);
        }

        internal static double BesselCorrection(int n)
        {
            return (double)n / (n - 1);
        }

        internal static double Smooth(double a, double b, double w)
        {
            return a + w * (b - a);
        }

        // we are assuming that A is sorted from least to greatest
        internal static int SearchSortedFirst(IList<double> A, double x)
        {
            for (int i = 0; i < A.Count; ++i)
            {
                if (A[i] >= x)
                {
                    return i;
                }
            }
            // y > a for all a in A
            return A.Count;
        }

        // we are assuming that A is sorted from least to greatest
        internal static int SearchSortedLast(IList<double> A, double y)
        {
            for (int i = 0; i < A.Count; ++i)
            {
                if (A[i] > y)
                {
                    return i - 1;
                }
            }
            // y < a for ell a in A
            return A.Count - 1;
        }

        internal static IList<T> Fill<T>(T x, int n)
        {
            IList<T> xs = new List<T>(n);
            for (int i = 0; i < n; ++i)
            {
                xs.Add(x);
            }
            return xs;
        }

        // we are assuming that xs are sorted
        internal static IList<double> Midpoints(IList<double> xs)
        {
            IList<double> ms = new List<double>(xs.Count - 1);
            for (int i = 1; i < xs.Count; i++)
            {
                ms.Add((xs[i] + xs[i-1]) / 2.0);
            }
            return ms;
        }

        internal static IList<string> GetPrintableBins(Histogram h)
        {
            int nbins = h.Edges.Count - 1;
            IList<string> bins = new List<string>(nbins);

            int startIdx, endIdx;
            string left, right, end;

            if (h.Left)
            {
                startIdx = 0;
                endIdx = nbins - 1; // last bin is the special case
                left = "[";
                right = ")";
                end = h.Closed ? "]" : ")";
                for (int i = startIdx; i < endIdx; i++)
                {
                    bins.Add($"{left}{h.Edges[i]:F2}, {h.Edges[i+1]:F2}{right}");
                }
                bins.Add($"{left}{h.Edges[nbins-1]:F2}, {h.Edges[nbins]:F2}{end}");
            }
            else
            {
                startIdx = 1; // first bin is a special case
                endIdx = nbins;
                left = "(";
                right = "]";
                end = h.Closed ? "[" : "(";
                bins.Add($"{end}{h.Edges[0]:F2}, {h.Edges[1]:F2}{right}");
                for (int i = startIdx; i < endIdx; i++)
                {
                    bins.Add($"{left}{h.Edges[i]:F2}, {h.Edges[i + 1]:F2}{right}");
                }
            }

            return bins;
        }

        internal static double SortedQuantile(IList<double> xs, double p)
        {
            if (p < 0.0 || p > 1.0)
            {
                throw new Exception($"p must be in range [0, 1]. Got {p}.");
            }

            int n = xs.Count;
            int i = (int)Math.Floor((n - 1) * p);
            return xs[i];
        }
    }
}
