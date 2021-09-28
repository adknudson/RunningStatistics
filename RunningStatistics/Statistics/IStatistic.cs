using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public interface IStatistic
    {
        public void Fit(double y);
        public void Fit(IList<double> ys);
        public void Write(StreamWriter stream);
    }



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

        public int Count { get => _n; }
        public abstract TReturn Value { get; }
        public abstract void Write(StreamWriter stream);
    }
}
