using System.IO;

namespace RunningStatistics
{
    /// <summary>
    /// Minimum and maximum (and number of occurrences for each) for a data stream of type <see cref="double"/>.
    /// </summary>
    public class Extrema : AbstractStatistic<double, (double, double, int, int)>
    {
        private double _max;

        private double _min;

        private int _nMax;

        private int _nMin;



        public int CountMax { get => _nMax; }

        public int CountMin { get => _nMin; }

        public double Max { get => _nobs == 1 ? _min : _max; } // if only one value is fit, then only the min gets updated, whereas both min and max should equal the one observation.

        public double Min { get => _min; }

        /// <summary>
        /// Return the range of the fitted observations.
        /// </summary>
        public double Range { get => _max - _min; }

        /// <summary>
        /// Returns the minimum and maximum (and the number of occurences for each respectively).
        /// </summary>
        public override (double, double, int, int) Value { get => (Min, Max, CountMin, CountMax); }



        public Extrema() : base()
        {
            _min = double.PositiveInfinity;
            _max = double.NegativeInfinity;
            _nMin = 0;
            _nMax = 0;
        }

        public Extrema(Extrema a) : base(a)
        {
            _min = a._min;
            _max = a._max;
            _nMin = a._nMin;
            _nMax = a._nMax;
        }



        public static Extrema Merge(Extrema a, Extrema b)
        {
            Extrema merged = new(a);
            merged.Merge(b);
            return merged;
        }

        public override void Fit(double y)
        {
            ++_nobs;


            if (y < _min)
            {
                _min = y;
                _nMin = 0;
            }
            else if (y > _max)
            {
                _max = y;
                _nMax = 0;
            }


            if (y == _min)
            {
                ++_nMin;
            }

            if (y == _max)
            {
                ++_nMax;
            }
        }

        public void Merge(Extrema b)
        {
            _nobs += b._nobs;

            if (_min == b._min)
            {
                _nMin += b._nMin;
            }
            else if (b._min < _min)
            {
                _min = b._min;
                _nMin = b._nMin;
            }

            if (_max == b._max)
            {
                _nMax += b._nMax;
            }
            else if (b._max > _max)
            {
                _max = b._max;
                _nMax = b._nMax;
            }
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            stream.WriteLine($"Minimum\t{Min} (Count = {CountMin})");
            stream.WriteLine($"Maximum\t{Max} (Count = {CountMax})");
        }

        public override void Reset()
        {
            base.Reset();
            _min = double.PositiveInfinity;
            _max = double.NegativeInfinity;
            _nMin = 0;
            _nMax = 0;
        }

        public static Extrema operator +(Extrema a, Extrema b)
        {
            return Merge(a, b);
        }
    }
}
