using System;
using System.IO;

namespace RunningStatistics.FileIO
{
    public static class StatWriter
    {
        public static void WriteStat(StreamWriter stream, IStatistic statistic)
        {
            statistic.Write(stream);
        }
    }
}
