using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineStatistics
{
    public class Sum : TypedStatistic<double>
    {
        private double _sum;

        public override double Value { get => _sum; }



        public Sum() : base()
        {
            _sum = 0.0;
        }
        public Sum(Sum a) : base(a)
        {
            _sum = a._sum;
        }



        public override void Fit(double y)
        {
            _n += 1;
            _sum += y;
        }
        public override void Fit(IList<double> ys)
        {
            _n += ys.Count;
            _sum += Utils.Sum(ys);
        }
        public void Merge(Sum b)
        {
            _n += b._n;
            _sum += b._sum;
        }




        public static Sum Merge(Sum a, Sum b)
        {
            Sum merged = new(a);
            a.Merge(b);
            return merged;
        }
        public static Sum operator +(Sum a, Sum b)
        {
            return Merge(a, b);
        }
    }
}
