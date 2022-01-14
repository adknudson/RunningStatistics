using System.Collections.Generic;

namespace RunningStatistics
{
    /// <summary>
    /// Defines the <see cref="AbstractStatistic{T, S}" /> with a strong return type.
    /// </summary>
    /// <typeparam name="TObs">The observation type.</typeparam>
    /// <typeparam name="TReturn">The return type for the underlying collection.</typeparam>
    public abstract class AbstractStatistic<TObs, TReturn> : IStatistic<TObs>
    {
        protected int _nobs;

        public int Count { get => _nobs; }

        public abstract TReturn Value { get; }


        public AbstractStatistic()
        {
            _nobs = 0;
        }

        public AbstractStatistic(AbstractStatistic<TObs, TReturn> a)
        {
            _nobs = a._nobs;
        }

        public virtual void Fit(IEnumerable<TObs> ys)
        {
            foreach (var y in ys)
            {
                Fit(y);
            }
        }

        public abstract void Fit(TObs y);

        public virtual void Print(System.IO.StreamWriter stream)
        {
            stream.WriteLine($"Count={_nobs} ({_nobs:n0})");
        }

        public virtual void Reset()
        {
            _nobs = 0;
        }
    }
}
