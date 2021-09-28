using System;
using System.Collections.Generic;

namespace RunningStats
{
    public class Variance : TypedStatistic<double>
    {
        private double _variance;
        private double _mean;



        public override double Value
        {
            get
            {
                if (_n > 1)
                {
                    return _variance * Utils.BesselCorrection(_n);
                }
                else
                {
                    return double.IsFinite(_mean) ? 1.0 : double.NaN;
                }
            }
        }



        public Variance() : base()
        {
            _variance = 0.0;
            _mean = 0.0;
        }
        public Variance(Variance a) : base(a)
        {
            _variance = a._variance;
            _mean = a._mean;
        }



        public override void Fit(double y)
        {
            _n += 1;
            double mu = _mean;
            double g = 1.0 / _n;
            _mean = Utils.Smooth(_mean, y, g);
            _variance = Utils.Smooth(_variance, (y - _mean) * (y - mu), g);
        }
        public override void Fit(IList<double> ys)
        {
            _n += ys.Count;

            double mean = Utils.Mean(ys);
            double variance = Utils.Variance(ys);
            double g = ys.Count / _n;
            double delta = _mean - mean;

            _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
            _mean = Utils.Smooth(_mean, mean, g);
        }
        public void Merge(Variance b)
        {
            _n += b._n;

            double g = b._n / _n;
            double delta = _mean - b._mean;

            _variance = Utils.Smooth(_variance, b._variance, g) + delta * delta * g * (1 - g);
            _mean = Utils.Smooth(_mean, b._mean, g);
        }



        public static Variance Merge(Variance a, Variance b)
        {
            Variance merged = new(a);
            a.Merge(b);
            return merged;
        }
        public static Variance operator +(Variance a, Variance b)
        {
            return Merge(a, b);
        }

    }
}
