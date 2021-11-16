using System;
using System.Collections.Generic;
using System.IO;

namespace RunningStatistics
{
    public class Countmap<T> : AbstractStatistic<T, Dictionary<T, int>>
    {
        private Dictionary<T, int> _counter;



        public override Dictionary<T, int> Value { get => _counter; }
        public int this[T key]
        {
            get
            {
                if (_counter.TryGetValue(key, out int count))
                {
                    return count;
                }
                else
                {
                    _counter[key] = 0;
                    return 0;
                }
            }
        }



        public Countmap() : base()
        {
            _counter = new();
        }
        public Countmap(Countmap<T> a) : base(a)
        {
            _counter = new Dictionary<T, int>(a._counter);
        }



        public override void Fit(T y)
        {
            Fit(y, 1);
        }
        private void Fit(T y, int k)
        {
            _n += k;

            if (_counter.ContainsKey(y))
            {
                _counter[y] += k;
            }
            else
            {
                _counter[y] = k;
            }
        }



        public void Merge(Countmap<T> b)
        {
            foreach (KeyValuePair<T, int> kvp in b._counter)
            {
                Fit(kvp.Key, kvp.Value);
            }
        }



        public override void Reset()
        {
            base.Reset();
            _counter = new();
        }



        public static Countmap<T> Merge(Countmap<T> a, Countmap<T> b)
        {
            Countmap<T> merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static Countmap<T> operator +(Countmap<T> a, Countmap<T> b)
        {
            return Merge(a, b);
        }

        public override void Write(StreamWriter stream)
        {
            base.Write(stream);
            SortedDictionary<T, int> sortedCountmap = new(_counter);
            foreach (var kvp in sortedCountmap)
            {
                stream.WriteLine($"{kvp.Key}\t{kvp.Value}");
            }
        }
    }
}
