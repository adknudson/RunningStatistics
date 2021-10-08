using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunningStatistics
{
    public class Extrema : TypedStatistic<(double, double, int, int)>
    {
        private double _min;
        private double _max;
        private int _nMin;
        private int _nMax;


        public double Min { get => _min; }
        public double Max { get => _max; }
        public int CountMin { get => _nMin; }
        public int CountMax { get => _nMax; }
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



        public override void Fit(double y)
        {
            ++_n;

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
            _n += b._n;

            if (_min == b._min)
            {
                _nMin += b._nMin;
            }
            else if(b._min < _min)
            {
                _min = b._min;
                _nMin = b._nMin;
            }

            if (_max == b._max)
            {
                _nMax += b._nMax;
            }
            else if(b._max > _max)
            {
                _max = b._max;
                _nMax = b._nMax;
            }
        }



        public override void Reset()
        {
            base.Reset();
            _min = double.PositiveInfinity;
            _max = double.NegativeInfinity;
            _nMin = 0;
            _nMax = 0;
        }



        public static Extrema Merge(Extrema a, Extrema b)
        {
            Extrema merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static Extrema operator +(Extrema a, Extrema b)
        {
            return Merge(a, b);
        }

        public override void Write(StreamWriter stream)
        {
            base.Write(stream);
            stream.WriteLine($"Minimum\t{Min}");
            stream.WriteLine($"Maximum\t{Max}");
            stream.WriteLine($"MinimumCount\t{CountMin}");
            stream.WriteLine($"MaximumCount\t{CountMax}");
        }
    }
}
