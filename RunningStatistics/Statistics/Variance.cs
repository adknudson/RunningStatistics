using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    /// <summary>
    /// Defines the <see cref="Variance" />.
    /// </summary>
    public class Variance : AbstractStatistic<double, double>
    {
        private double _mean;

        private double _variance;



        /// <summary>
        /// Returns the bias-corrected variance.
        /// </summary>
        public override double Value
        {
            get
            {
                if (_nobs > 1)
                {
                    return _variance * Utils.BesselCorrection(_nobs);
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



        public static Variance Merge(Variance a, Variance b)
        {
            Variance merged = new(a);
            a.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            _nobs += 1;
            double mu = _mean;
            double g = 1.0 / _nobs;
            _mean = Utils.Smooth(_mean, y, g);
            _variance = Utils.Smooth(_variance, (y - _mean) * (y - mu), g);
        }

        public override void Fit(IEnumerable<double> ys)
        {
            _nobs += ys.Count();

            double mean = ys.Average();
            double variance = Utils.Variance(ys);
            double g = (double)ys.Count() / _nobs;
            double delta = _mean - mean;

            _variance = Utils.Smooth(_variance, variance, g) + delta * delta * g * (1 - g);
            _mean = Utils.Smooth(_mean, mean, g);
        }

        public void Merge(Variance b)
        {
            _nobs += b._nobs;

            if (_nobs > 0)
            {
                double g = (double)b._nobs / _nobs;
                double delta = _mean - b._mean;

                _variance = Utils.Smooth(_variance, b._variance, g) + delta * delta * g * (1 - g);
                _mean = Utils.Smooth(_mean, b._mean, g);
            }
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            stream.WriteLine($"Variance\t{Value}");
        }

        public override void Reset()
        {
            base.Reset();
            _variance = 0.0;
            _mean = 0.0;
        }

        public static Variance operator +(Variance a, Variance b)
        {
            return Merge(a, b);
        }
    }
}
