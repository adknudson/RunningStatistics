using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public interface IStatistic<T>
    {
        public int Nobs { get; }

        public void Fit(T y);
        public void Fit(IList<T> ys);
        
        public void Reset();
        public void Write(StreamWriter stream);
    }
}
