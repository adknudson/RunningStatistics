using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    public class Histogram : TypedStatistic<IList<int>>
    {
        private readonly IList<double> _edges;
        private readonly IList<int> _binCounts;
        private readonly IList<int> _outOfBoundsCounts;
        private readonly bool _left;
        private readonly bool _closed;

        private int NumBins { get => _edges.Count - 1; }



        public IList<double> Edges { get => _edges; }
        public IList<int> BinCounts { get => _binCounts; }
        public IList<int> OutOfBoundsCounts { get => _outOfBoundsCounts; }
        public bool Left { get => _left; }
        public bool Closed { get => _closed; }
        public override IList<int> Value { get => _binCounts; }



        public Histogram(IList<double> edges, bool left = true, bool closed = true) : base()
        {
            _edges = edges.OrderBy(e => e).ToList();

            _left = left;
            _closed = closed;

            _binCounts = Utils.Fill<int>(0, NumBins);
            _outOfBoundsCounts = Utils.Fill<int>(0, 2);
        }
        public Histogram(Histogram a) : base(a)
        {
            _edges = new List<double>(a._edges);
            _left = a._left;
            _closed = a._closed;
            _binCounts = new List<int>(a._binCounts);
            _outOfBoundsCounts = new List<int>(a._outOfBoundsCounts);
        }



        public void Fit(double y, int k)
        {
            _n += k;

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
        public override void Fit(double y)
        {
            Fit(y, 1);
        }

        public void Merge(Histogram b)
        {
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
                for (int j = 0; j < _edges.Count; j++)
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



        public static Histogram Merge(Histogram a, Histogram b)
        {
            Histogram merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static Histogram operator +(Histogram a, Histogram b)
        {
            return Merge(a, b);
        }



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

        public override void Write(StreamWriter stream)
        {
            IList<string> printableBins = Utils.GetPrintableBins(this);
            foreach (var (bin, count) in Enumerable.Zip(printableBins, _binCounts))
            {
                stream.WriteLine($"{bin}\t{count}");
            }
        }
    }
}
