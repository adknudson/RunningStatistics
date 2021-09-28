using System;
using System.Collections.Generic;

namespace OnlineStatistics
{
    public class Mean : TypedStatistic<double>
    {
        private double _mean;
        public override double Value { get => _n == 0 ? double.NaN : _mean; }


        public Mean() : base()
        {
            _mean = 0.0;
        }
        public Mean(Mean a) : base(a)
        {
            _mean = a._mean;
        }



        public override void Fit(double y)
        {
            ++_n;
            _mean = Utils.Smooth(_mean, y, 1.0 / _n);
        }
        public override void Fit(IList<double> ys)
        {
            _n += ys.Count;
            _mean = Utils.Smooth(_mean, Utils.Mean(ys), ys.Count / _n);
        }
        public void Merge(Mean b)
        {
            _n += b._n;
            _mean = Utils.Smooth(_mean, b._mean, b._n / _n);
        }



        public static Mean Merge(Mean a, Mean b)
        {
            Mean merged = new(a);
            a.Merge(b);
            return merged;
        }
        public static Mean operator +(Mean a, Mean b)
        {
            return Merge(a, b);
        }
    }
}
