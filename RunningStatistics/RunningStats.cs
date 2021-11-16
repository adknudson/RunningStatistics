using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public class RunningStats<T>
    {
        private readonly List<IStatistic<T>> _stats = new();



        public IStatistic<T> this[int i] { get => _stats[i]; }
        public int Count => _stats.Count;



        public void Add(IStatistic<T> stat)
        {
            this._stats.Add(stat);
        }



        public void Fit(T x)
        {
            foreach (var stat in _stats)
            {
                stat.Fit(x);
            }
        }

        public void Fit(IList<T> xs)
        {
            foreach (var stat in _stats)
            {
                stat.Fit(xs);
            }
        }



        public void Reset()
        {
            foreach (var stat in _stats)
            {
                stat.Reset();
            }
        }



        public void Write(StreamWriter stream)
        {
            foreach (var stat in _stats)
            {
                stream.WriteLine(stat.ToString());
                stat.Write(stream);
                stream.WriteLine();
            }
        }

    }
}
