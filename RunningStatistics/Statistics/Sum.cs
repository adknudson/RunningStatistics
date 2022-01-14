using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    /// <summary>
    /// Tracks the overall sum, stored as a <see cref="double"/>.
    /// </summary>
    public class Sum : AbstractStatistic<double, double>
    {
        private double _sum;



        public override double Value { get => _sum; }

        public double Mean { get => _sum / (double)_nobs; }



        public Sum() : base()
        {
            _sum = 0.0;
        }

        public Sum(Sum a) : base(a)
        {
            _sum = a._sum;
        }



        public static Sum Merge(Sum a, Sum b)
        {
            Sum merged = new(a);
            a.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            _nobs += 1;
            _sum += y;
        }

        public override void Fit(IEnumerable<double> ys)
        {
            _nobs += ys.Count();
            _sum += ys.Sum();
        }

        public void Merge(Sum b)
        {
            _nobs += b._nobs;
            _sum += b._sum;
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            stream.WriteLine($"Sum\t{Value}");
        }

        public override void Reset()
        {
            base.Reset();
            _sum = 0.0;
        }

        public static Sum operator +(Sum a, Sum b)
        {
            return Merge(a, b);
        }
    }
}
