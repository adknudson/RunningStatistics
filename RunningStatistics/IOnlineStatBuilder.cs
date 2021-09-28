using System;
using System.Collections.Generic;

namespace RunningStatistics
{
    public interface IOnlineStatBuilder
    {
        void BuildMean();
        void BuildVariance();
        void BuildSum();
        void BuildExtrema();
        void BuildMoments();
        void BuildHistogram(IList<double> edges, bool left = true, bool closed = true);
        void BuildCountmap();
        void BuildOrderStatistics(int b);
    }



    public class OnlineStatBuilder : IOnlineStatBuilder
    {
        private OnlineStats _stats = new();
        public OnlineStatBuilder()
        {
            this.Reset();
        }
        public void Reset()
        {
            this._stats = new();
        }
        public void BuildMean()
        {
            this._stats.Add(new Mean());
        }
        public void BuildVariance()
        {
            this._stats.Add(new Variance());
        }
        public void BuildExtrema()
        {
            this._stats.Add(new Extrema());
        }
        public void BuildMoments()
        {
            this._stats.Add(new Moments());
        }
        public void BuildSum()
        {
            this._stats.Add(new Sum());
        }
        public void BuildHistogram(IList<double> edges, bool left = true, bool closed = true)
        {
            this._stats.Add(new Histogram(edges, left, closed));
        }
        public void BuildCountmap()
        {
            this._stats.Add(new Countmap());
        }
        public void BuildOrderStatistics(int b = 100)
        {
            this._stats.Add(new OrderStatistics(b));
        }



        public OnlineStats GetOnlineStats()
        {
            OnlineStats stats = this._stats;
            this.Reset();
            return stats;
        }
    }



    public class OnlineStats
    {
        private readonly List<IStatistic> _stats = new();
        public void Add(IStatistic stat)
        {
            this._stats.Add(stat);
        }
        public void Fit(double x)
        {
            foreach (var stat in _stats)
            {
                stat.Fit(x);
            }
        }
        public void Fit(IList<double> xs)
        {
            foreach (var stat in _stats)
            {
                stat.Fit(xs);
            }
        }

        public IStatistic this[int i] { get => _stats[i]; }
    }
}
