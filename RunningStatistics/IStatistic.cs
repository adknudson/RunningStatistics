using System.IO;
using System.Collections.Generic;

namespace RunningStatistics
{
    public interface IStatistic
    {
        public void Fit(double y);
        public void Fit(IList<double> ys);
        public void Reset();
        public void Write(StreamWriter stream);
    }
}
