using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public abstract class AbstractStatistic<T, TReturn> : IStatistic<T>
    {
        protected int _n;

        public AbstractStatistic()
        {
            _n = 0;
        }
        public AbstractStatistic(AbstractStatistic<T, TReturn> a)
        {
            _n = a._n;
        }

        public abstract void Fit(T y);
        public virtual void Fit(IList<T> ys)
        {
            foreach (var y in ys)
            {
                Fit(y);
            }
        }
        public virtual void Reset()
        {
            _n = 0;
        }

        public int Nobs { get => _n; }
        public abstract TReturn Value { get; }
        public virtual void Write(StreamWriter stream)
        {
            stream.WriteLine($"Count={_n} ({_n:n0})");
        }
    }
}
