using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public class RunningStats
    {
        private readonly List<IStatistic> _stats = new();



        public IStatistic this[int i] { get => _stats[i]; }



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



        public void Write(StreamWriter stream)
        {
            foreach (IStatistic stat in _stats)
            {
                stream.WriteLine(stat.ToString());
                stat.Write(stream);
                stream.WriteLine();
            }
        }

    }
}
