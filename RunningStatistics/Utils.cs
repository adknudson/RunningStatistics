using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics
{
    internal static class Utils
    {
        internal static double BesselCorrection(int n)
        {
            return (double)n / (n - 1);
        }

        internal static IList<T> Fill<T>(T x, int n)
        {
            List<T> xs = new(n);
            for (int i = 0; i < n; ++i)
            {
                xs.Add(x);
            }
            return xs;
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
                    bins.Add($"{left}{h.Edges[i]:F2}, {h.Edges[i + 1]:F2}{right}");
                }
                bins.Add($"{left}{h.Edges[nbins - 1]:F2}, {h.Edges[nbins]:F2}{end}");
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

        /// <summary>
        /// Find the midpoints of consecutive pairs in a sorted list.
        /// </summary>
        internal static IList<double> Midpoints(IList<double> xs)
        {
            IList<double> ms = new List<double>(xs.Count - 1);
            for (int i = 1; i < xs.Count; i++)
            {
                ms.Add((xs[i] + xs[i - 1]) / 2.0);
            }
            return ms;
        }

        /// <summary>
        /// Find the index of the first matching value in a list sorted from least to greatest.
        /// </summary>
        internal static int SearchSortedFirst(IEnumerable<double> A, double x)
        {
            foreach (var (a, i) in A.Select((item, index) => (item, index)))
            {
                if (a >= x)
                {
                    return i;
                }
            }

            // y > a for all a in A
            return A.Count();
        }

        /// <summary>
        /// Find the index of the last matching value in a list sorted from least to greatest.
        /// </summary>
        internal static int SearchSortedLast(IEnumerable<double> A, double y)
        {
            foreach (var (a, i) in A.Select((item, index) => (item, index)))
            {
                if (a > y)
                {
                    return i - 1;
                }
            }

            // y < a for all a in A
            return A.Count() - 1;
        }

        /// <summary>
        /// Weighted interpolation of two values.
        /// </summary>
        internal static double Smooth(double a, double b, double w)
        {
            return a + w * (b - a);
        }

        /// <summary>
        /// Finds the pth quantile of a collection that is assumed to be sorted.
        /// </summary>
        internal static double SortedQuantile(IEnumerable<double> xs, double p)
        {
            if (p < 0.0 || p > 1.0)
            {
                throw new Exception($"p must be in range [0, 1]. Got {p}.");
            }

            int n = xs.Count();
            int i = (int)Math.Floor((n - 1) * p);
            return xs.ElementAt(i);
        }

        /// <summary>
        /// Compute the (biased) variance of a collection of values.
        /// </summary>
        internal static double Variance(IEnumerable<double> xs)
        {
            double mean = xs.Average();
            double meanSquare = xs.Average(s => s * s);

            return Math.Abs(meanSquare - mean * mean);
        }
    }
}
