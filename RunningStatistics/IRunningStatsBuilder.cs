using System;
using System.Collections.Generic;

namespace RunningStatistics
{
    public interface IRunningStatsBuilder
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
}
