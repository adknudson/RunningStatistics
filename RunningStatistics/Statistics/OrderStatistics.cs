using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    /// <summary>
    /// Approximate order statistics (CDF) with batches of a given size.
    /// </summary>
    public class OrderStatistics : AbstractStatistic<double, IList<double>>
    {
        private readonly int _b;

        private readonly IList<double> _defaultQuantiles;

        private IList<double> _buffer;

        private Extrema _extrema;

        private IList<double> _values;



        public double Median { get => Quantile(0.5); }

        public override IList<double> Value { get => _values; }



        public OrderStatistics(int b, IList<double> defaultQuantiles = null) : base()
        {
            if (defaultQuantiles == null)
            {
                _defaultQuantiles = Constant.defaultQuantiles;
            }
            else
            {
                _defaultQuantiles = new List<double>(defaultQuantiles);
            }

            _b = b;
            _values = Utils.Fill<double>(0.0, b);
            _buffer = Utils.Fill<double>(0.0, b);
            _extrema = new();
        }

        public OrderStatistics(OrderStatistics a) : base(a)
        {
            _b = a._b;
            _values = new List<double>(a._values);
            _buffer = new List<double>(a._buffer);
            _extrema = new Extrema(a._extrema);
        }



        public static OrderStatistics Merge(OrderStatistics a, OrderStatistics b)
        {
            OrderStatistics merged = new(a);
            merged.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            int i = (_nobs % _b);

            _nobs += 1;
            _extrema.Fit(y);
            _buffer[i] = y;

            if (i + 1 == _b)
            {
                _buffer = _buffer.OrderBy(t => t).ToList();
                for (int k = 0; k < _b; k++)
                {
                    _values[k] = Utils.Smooth(_values[k], _buffer[k], (double)_b / _nobs);
                }
            }
        }

        public override void Fit(IEnumerable<double> ys)
        {
            base.Fit(ys);
        }

        public void Merge(OrderStatistics b)
        {
            if (_b != b._b)
            {
                throw new Exception($"The two OrderStatistics objects must have the same batch size. Got {_b} and {b._b}.");
            }

            _nobs += b._nobs;
            _extrema.Merge(b._extrema);
            for (int k = 0; k < _b; k++)
            {
                _values[k] = Utils.Smooth(_values[k], b._values[k], (double)_b / b._b);
            }
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            foreach (double p in _defaultQuantiles)
            {
                stream.WriteLine($"{p}\t{Quantile(p)}");
            }
        }

        /// <summary>
        /// Get the nearest quantile.
        /// </summary>
        public double Quantile(double p)
        {
            return Utils.SortedQuantile(_values, p);
        }

        public override void Reset()
        {
            base.Reset();
            _values = Utils.Fill<double>(0.0, _b);
            _buffer = Utils.Fill<double>(0.0, _b);
            _extrema = new();
        }

        public static OrderStatistics operator +(OrderStatistics a, OrderStatistics b)
        {
            return Merge(a, b);
        }
    }
}
