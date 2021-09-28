using System.Collections.Generic;

namespace RunningStats
{
    public interface IStatistic
    {
        public void Fit(double y);
        public void Fit(IList<double> ys);
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
    }
}
