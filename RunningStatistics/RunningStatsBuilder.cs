using System;
using System.Collections.Generic;

namespace RunningStatistics
{
    public class RunningStatsBuilder : IRunningStatsBuilder
    {
        private RunningStats<double> _stats = new();



        public RunningStatsBuilder()
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
            this._stats.Add(new Countmap<double>());
        }
        public void BuildOrderStatistics(int b = 200, IList<double> defaultQuantiles = null)
        {
            this._stats.Add(new OrderStatistics(b, defaultQuantiles));
        }



        public RunningStats<double> GetRunningStats()
        {
            RunningStats<double> stats = this._stats;
            this.Reset();
            return stats;
        }
    }
}
