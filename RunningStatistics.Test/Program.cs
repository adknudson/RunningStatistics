using System;
using System.IO;

namespace RunningStatistics.Test
{
    class Program
    {
        static void Main()
        {
            var rng = new Random();
            long n = (long)100e6;


            RunningStatsDirector director = new();
            RunningStatsBuilder builder = new();
            director.Builder = builder;

            director.BuildAllSimpleStats();
            builder.BuildOrderStatistics(200);
            var rs = builder.GetRunningStats();


            for (long i = 0; i < n; i++)
            {
                rs.Fit(rng.NextDouble() * 100);
            }


            using StreamWriter file = new("../../output.txt");
            rs.Write(file);
        }
    }
}
