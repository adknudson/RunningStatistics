using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    /// <summary>
    /// A histogram with bin partitions defined by edges.
    /// </summary>
    public class Histogram : AbstractStatistic<double, IList<int>>, IEnumerable<(string, int)>
    {
        private readonly bool _closed;

        private readonly IList<double> _edges;

        private readonly bool _left;

        private IList<int> _binCounts;

        private IList<int> _outOfBoundsCounts;


        /// <summary>
        /// Return the counts within each bin.
        /// </summary>
        public IList<int> BinCounts { get => _binCounts; }

        /// <summary>
        /// Are the edges closed at the endpoint.
        /// </summary>
        public bool Closed { get => _closed; }

        /// <summary>
        /// Get the bin edges.
        /// </summary>
        public IList<double> Edges { get => _edges; }

        /// <summary>
        /// Are the bins left-closed.
        /// </summary>
        public bool Left { get => _left; }

        /// <summary>
        /// Get the counts for the number of observations that fall outside of the histogram edges. Returns lower count then upper.
        /// </summary>
        public IList<int> OutOfBoundsCounts { get => _outOfBoundsCounts; }

        /// <summary>
        /// Return the counts within each bin.
        /// </summary>
        public override IList<int> Value { get => _binCounts; }

        /// <summary>
        /// Get the number of bins.
        /// </summary>
        private int NumBins { get => _edges.Count - 1; }

        /// <summary>
        /// Print-friendly list of bins defined by <see cref="Edges"/>, <see cref="Left"/>, and <see cref="Closed"/>.
        /// </summary>
        public IList<string> PrintableBins { get => Utils.GetPrintableBins(this); }



        public Histogram(Histogram a) : base(a)
        {
            _edges = new List<double>(a._edges);
            _left = a._left;
            _closed = a._closed;
            _binCounts = new List<int>(a._binCounts);
            _outOfBoundsCounts = new List<int>(a._outOfBoundsCounts);
        }

        /// <summary>
        /// Create a histogram with bins defined by edges.
        /// </summary>
        /// <param name="edges">The end points and partitions of the bins.</param>
        /// <param name="left">Should the edges be left closed (left=true) or right closed (left=false).</param>
        /// <param name="closed">Should the bin on the end be closed. E.g. <c>[a,b),[b,c)</c> vs. <c>[a,b),[b,c]</c></param>
        public Histogram(IList<double> edges, bool left = true, bool closed = true) : base()
        {
            _edges = edges.OrderBy(e => e).ToList();

            _left = left;
            _closed = closed;

            _binCounts = Utils.Fill<int>(0, NumBins);
            _outOfBoundsCounts = Utils.Fill<int>(0, 2);
        }


        public static Histogram Merge(Histogram a, Histogram b)
        {
            Histogram merged = new(a);
            merged.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            Fit(y, 1);
        }


        /// <summary>
        /// Increment a bin by a value other than one.
        /// </summary>
        public void Fit(double y, int k)
        {
            _nobs += k;

            int i = GetBinIndex(y);
            if (0 <= i && i < NumBins)
            {
                _binCounts[i] += k;
            }
            else
            {
                _outOfBoundsCounts[i >= 0 ? 1 : 0] += k;
            }
        }


        public void Merge(Histogram b)
        {
            _nobs += b._nobs;

            bool edgesAreMatching = true;
            if (_edges.Count == b._edges.Count)
            {
                for (int i = 0; i < _edges.Count; i++)
                {
                    if (_edges[i] != b._edges[i])
                    {
                        edgesAreMatching = false;
                        break;
                    }
                }
            }
            else
            {
                edgesAreMatching = false;
            }

            if (edgesAreMatching)
            {
                for (int j = 0; j < NumBins; j++)
                {
                    _binCounts[j] += b._binCounts[j];
                }
            }
            else
            {
                // Using midpoints of source edges as approximate locations for merging
                IList<double> midpoints = Utils.Midpoints(b._edges);
                for (int j = 0; j < midpoints.Count; j++)
                {
                    Fit(midpoints[j], b._binCounts[j]);
                }
            }
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            foreach (var (bin, count) in this)
            {
                stream.WriteLine($"{bin}\t{count}");
            }
        }

        public override void Reset()
        {
            base.Reset();
            _binCounts = Utils.Fill<int>(0, NumBins);
            _outOfBoundsCounts = Utils.Fill<int>(0, 2);
        }

        /// <summary>
        /// Get the index of the bin that an observation falls in.
        /// </summary>
        private int GetBinIndex(double y)
        {
            double a = _edges.First();
            double b = _edges.Last();

            if (y < a)
            {
                return -1;
            }


            if (y > b)
            {
                return NumBins;
            }


            if (_closed && y == a)
            {
                return 0;
            }


            if (_closed && y == b)
            {
                return NumBins - 1;
            }


            return _left ? Utils.SearchSortedLast(_edges, y) : Utils.SearchSortedFirst(_edges, y) - 1;
        }

        public static Histogram operator +(Histogram a, Histogram b)
        {
            return Merge(a, b);
        }


        public IEnumerator<(string, int)> GetEnumerator()
        {
            foreach (var (bin, count) in Enumerable.Zip(PrintableBins, BinCounts))
            {
                yield return (bin, count);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
