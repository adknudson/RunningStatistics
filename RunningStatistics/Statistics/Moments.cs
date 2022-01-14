using System;
using System.IO;

namespace RunningStatistics
{
    /// <summary>
    /// Tracks the first four non-central moments, stored as a <see cref="double"/>.
    /// </summary>
    public class Moments : AbstractStatistic<double, (double, double, double, double)>
    {
        private double _kurtosis;

        private double _mean;

        private double _skewness;

        private double _variance;

        /// <summary>
        /// Returns the (excess) kurtosis.
        /// </summary>
        public double Kurtosis
        {
            get
            {
                double meanSquared = _mean * _mean;
                double variance = _variance - meanSquared;
                return _nobs == 0 ? double.NaN : (_kurtosis - 4.0 * _mean * _skewness + 6.0 * meanSquared * _variance - 3.0 * meanSquared * meanSquared) / (variance * variance) - 3.0;
            }
        }

        public double Mean { get => _nobs == 0 ? double.NaN : _mean; }

        public double Skewness
        {
            get
            {
                double vr = _variance - _mean * _mean;
                return _nobs == 0 ? double.NaN : (_skewness - 3.0 * _mean * vr - _mean * _mean * _mean) / Math.Pow(vr, 1.5);
            }
        }

        /// <summary>
        /// Returns the mean, variance, skewness, and (excess) kurtosis.
        /// </summary>
        public override (double, double, double, double) Value { get => (Mean, Variance, Skewness, Kurtosis); }

        /// <summary>
        /// The bias-corrected variance.
        /// </summary>
        public double Variance
        {
            get
            {
                double v = _variance - _mean * _mean;
                return _nobs == 0 ? double.NaN : Utils.BesselCorrection(_nobs) * v;
            }
        }

        /// <summary>
        /// The sample standard deviation.
        /// </summary>
        public double StdDev { get => Math.Sqrt(Variance); }



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



        public static Moments Merge(Moments a, Moments b)
        {
            Moments merged = new(a);
            merged.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            ++_nobs;

            double g = 1.0 / _nobs;
            double y2 = y * y;

            _mean = Utils.Smooth(_mean, y, g);
            _variance = Utils.Smooth(_variance, y2, g);
            _skewness = Utils.Smooth(_skewness, y * y2, g);
            _kurtosis = Utils.Smooth(_kurtosis, y2 * y2, g);
        }

        public void Merge(Moments b)
        {
            _nobs += b._nobs;

            if (_nobs > 0)
            {
                double g = (double)b._nobs / _nobs;

                _mean = Utils.Smooth(_mean, b._mean, g);
                _variance = Utils.Smooth(_variance, b._variance, g);
                _skewness = Utils.Smooth(_skewness, b._skewness, g);
                _kurtosis = Utils.Smooth(_kurtosis, b._kurtosis, g);
            }
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            stream.WriteLine($"Mean\t{Mean}");
            stream.WriteLine($"Variance\t{Variance}");
            stream.WriteLine($"Skewness\t{Skewness}");
            stream.WriteLine($"Kurtosis\t{Kurtosis}");
        }

        public override void Reset()
        {
            base.Reset();
            _mean = 0.0;
            _variance = 0.0;
            _skewness = 0.0;
            _kurtosis = 0.0;
        }

        public static Moments operator +(Moments a, Moments b)
        {
            return Merge(a, b);
        }
    }
}
