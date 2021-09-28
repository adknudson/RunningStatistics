using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunningStats
{
    public class Moments : TypedStatistic<(double, double, double, double)>
    {
        private double _mean;
        private double _variance;
        private double _skewness;
        private double _kurtosis;



        public double Mean { get => _n == 0 ? double.NaN : _mean; }
        public double Variance
        {
            get
            {
                double v = _variance - _mean * _mean;
                return Utils.BesselCorrection(_n) * v;
            }
        }
        public double Skewness
        {
            get
            {
                double vr = _variance - _mean * _mean;
                return (_skewness - 3.0 * _mean * vr - _mean * _mean * _mean) / Math.Pow(vr, 1.5);
            }
        }
        public double Kurtosis
        {
            get
            {
                double meanSquared = _mean * _mean;
                double variance = _variance - meanSquared;
                return (_kurtosis - 4.0 * _mean * _skewness + 6.0 * meanSquared * _variance - 3.0 * meanSquared * meanSquared) / (variance * variance) - 3.0;
            }
        }
        public double ExcessKurtosis { get => Kurtosis - 3.0; }
        public override (double, double, double, double) Value { get => (Mean, Variance, Skewness, Kurtosis); }



        public Moments() : base()
        {
            _mean = 0.0;
            _variance = 0.0;
            _skewness = 0.0;
            _kurtosis = 0.0;
        }
        public Moments(Moments a) : base(a)
        {
            _mean = a._mean;
            _variance = a._variance;
            _skewness = a._skewness;
            _kurtosis = a._kurtosis;
        }



        public override void Fit(double y)
        {
            ++_n;

            double g = 1.0 / _n;
            double y2 = y * y;

            _mean = Utils.Smooth(_mean, y, g);
            _variance = Utils.Smooth(_variance, y2, g);
            _skewness = Utils.Smooth(_skewness, y * y2, g);
            _kurtosis = Utils.Smooth(_kurtosis, y2 * y2, g);
        }
        public void Merge(Moments b)
        {
            _n += b._n;

            double g = b._n / _n;

            _mean = Utils.Smooth(_mean, b._mean, g);
            _variance = Utils.Smooth(_variance, b._variance, g);
            _skewness = Utils.Smooth(_skewness, b._skewness, g);
            _kurtosis = Utils.Smooth(_kurtosis, b._kurtosis, g);
        }



        public static Moments Merge(Moments a, Moments b)
        {
            Moments merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static Moments operator +(Moments a, Moments b)
        {
            return Merge(a, b);
        }
    }
}
