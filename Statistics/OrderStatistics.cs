using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineStatistics.Statistics
{
    public class OrderStatistics : TypedStatistic<IList<double>>
    {
        private IList<double> _values;
        private IList<double> _buffer;
        private Extrema _extrema;
    }
}
