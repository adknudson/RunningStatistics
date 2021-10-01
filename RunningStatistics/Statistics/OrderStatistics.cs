using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    public class OrderStatistics : TypedStatistic<IList<double>>
    {
        private readonly IList<double> _values;
        private IList<double> _buffer;
        private readonly Extrema _extrema;
        private readonly int _b;
        private readonly IList<double> _defaultQuantiles;



        public override IList<double> Value { get => _values; }
        public double Quantile(double p)
        {
            return Utils.SortedQuantile(_values, p);
        }



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



        public override void Fit(double y)
        {
            int i = (_n % _b);

            _n += 1;
            _extrema.Fit(y);
            _buffer[i] = y;
            
            if (i + 1 == _b)
            {
                _buffer = _buffer.OrderBy(t => t).ToList();
                for (int k = 0; k < _b; k++)
                {
                    _values[k] = Utils.Smooth(_values[k], _buffer[k], _b / _n);
                }
            }
        }
        public override void Fit(IList<double> ys)
        {
            base.Fit(ys);
        }
        public void Merge(OrderStatistics b)
        {
            if (_b != b._b)
            {
                throw new Exception($"The two OrderStatistics objects must have the same batch size. Got {_b} and {b._b}.");
            }

            _extrema.Merge(b._extrema);
            for (int k = 0; k < _b; k++)
            {
                _values[k] = Utils.Smooth(_values[k], b._values[k], _b / b._b);
            }
        }



        public static OrderStatistics Merge(OrderStatistics a, OrderStatistics b)
        {
            OrderStatistics merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static OrderStatistics operator +(OrderStatistics a, OrderStatistics b)
        {
            return Merge(a, b);
        }



        public void Write(StreamWriter stream, IList<double> quantiles)
        {
            foreach (double p in quantiles)
            {
                stream.WriteLine($"{p}\t{Quantile(p)}");
            }
        }
        public override void Write(StreamWriter stream)
        {
            Write(stream, _defaultQuantiles);
        }
    }
}
