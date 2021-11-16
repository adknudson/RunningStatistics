using System.Collections.Generic;

namespace RunningStatistics
{
    public interface IRunningStatsBuilder
    {
        void BuildCountmap();
        void BuildExtrema();
        void BuildHistogram(IList<double> edges, bool left = true, bool closed = true);
        void BuildMean();
        void BuildMoments();
        void BuildOrderStatistics(int b, IList<double> defaultQuantiles = null);
        void BuildSum();
        void BuildVariance();
    }
}
