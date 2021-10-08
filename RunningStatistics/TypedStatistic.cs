using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public abstract class TypedStatistic<TReturn> : IStatistic
    {
        protected int _n;

        public TypedStatistic()
        {
            _n = 0;
        }
        public TypedStatistic(TypedStatistic<TReturn> a)
        {
            _n = a._n;
        }

        public abstract void Fit(double y);
        public virtual void Fit(IList<double> ys)
        {
            foreach (double y in ys)
            {
                Fit(y);
            }
        }
        public virtual void Reset()
        {
            _n = 0;
        }

        public int Count { get => _n; }
        public abstract TReturn Value { get; }
        public virtual void Write(StreamWriter stream)
        {
            stream.WriteLine($"Count={_n} ({_n:n0})");
        }
    }
}
