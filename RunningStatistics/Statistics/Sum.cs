using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunningStatistics
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



        public override void Reset()
        {
            base.Reset();
            _sum = 0.0;
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

        public override void Write(StreamWriter stream)
        {
            base.Write(stream);
            stream.WriteLine($"Sum\t{Value}");
        }
    }
}
