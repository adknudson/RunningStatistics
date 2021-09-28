using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics.Test
{
    class Program
    {
        static void Main()
        {
            OrderStatistics os = new(500);
            Random rng = new();
            int n = 10_000_000;
            IList<double> ps = Enumerable.Range(0, 101).Select(s => (double)s / 100).ToList();

            for (int i = 0; i < n; i++)
            {
                os.Fit(rng.NextDouble() * 100);
            }

            using StreamWriter file = new("../../output.txt");
            os.Write(file, ps);
        }
    }
}
