using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    /// <summary>
    /// Tracks the univariate mean, stored as a <see cref="double"/>.
    /// </summary>
    public class Mean : AbstractStatistic<double, double>
    {
        private double _mean;



        public override double Value { get => _nobs == 0 ? double.NaN : _mean; }



        public Mean() : base()
        {
            _mean = 0.0;
        }

        public Mean(Mean a) : base(a)
        {
            _mean = a._mean;
        }



        public static Mean Merge(Mean a, Mean b)
        {
            Mean merged = new(a);
            a.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            ++_nobs;
            _mean = Utils.Smooth(_mean, y, 1.0 / _nobs);
        }

        public override void Fit(IEnumerable<double> ys)
        {
            _nobs += ys.Count();
            _mean = Utils.Smooth(_mean, ys.Average(), (double)ys.Count() / _nobs);
        }

        public void Merge(Mean b)
        {
            _nobs += b._nobs;
            _mean = _nobs == 0 ? double.NaN : Utils.Smooth(_mean, b._mean, (double)b._nobs / _nobs);
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            stream.WriteLine($"Mean\t{Value}");
        }

        public override void Reset()
        {
            base.Reset();
            _mean = 0.0;
        }

        public static Mean operator +(Mean a, Mean b)
        {
            return Merge(a, b);
        }
    }
}
