using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RunningStatistics
{
    /// <summary>
    /// A dictionary that maps unique values to its number of occurences.
    /// </summary>
    /// <typeparam name="TKey">The observation type.</typeparam>
    public class Countmap<TKey> : AbstractStatistic<TKey, Dictionary<TKey, int>>
    {
        private Dictionary<TKey, int> _counter;

        public override Dictionary<TKey, int> Value { get => _counter; }

        /// <summary>
        /// Return the normalized counts so that they sum to one.
        /// </summary>
        public Dictionary<TKey, double> Probabilities
        {
            get
            {
                Dictionary<TKey, double> _probs = new();
                foreach (var (key, obs) in _counter)
                {
                    _probs[key] = (double)obs / Count;
                }
                return _probs;
            }
        }

        /// <summary>
        /// Get the count for a specified key. Returns 0 if the key is not found.
        /// </summary>
        public int this[TKey key]
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

        /// <summary>
        /// Get the value with the most occurences.
        /// </summary>
        public TKey Mode { get => _counter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key; }

        public Countmap() : base()
        {
            _counter = new();
        }

        public Countmap(Countmap<TKey> a) : base(a)
        {
            _counter = new Dictionary<TKey, int>(a._counter);
        }

        /// <summary>
        /// Increment a count by one.
        /// </summary>
        public override void Fit(TKey y)
        {
            Fit(y, 1);
        }

        /// <summary>
        /// Increment a count by a value other than one.
        /// </summary>
        public void Fit(TKey y, int k)
        {
            _nobs += k;

            if (_counter.ContainsKey(y))
            {
                _counter[y] += k;
            }
            else
            {
                _counter[y] = k;
            }
        }

        public void Merge(Countmap<TKey> b)
        {
            foreach ((TKey key, int value) in b._counter)
            {
                Fit(key, value);
            }
        }

        public override void Reset()
        {
            base.Reset();
            _counter = new();
        }

        public static Countmap<TKey> Merge(Countmap<TKey> a, Countmap<TKey> b)
        {
            Countmap<TKey> merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static Countmap<TKey> operator +(Countmap<TKey> a, Countmap<TKey> b)
        {
            return Merge(a, b);
        }

        public override void Print(StreamWriter stream)
        {
            base.Print(stream);
            SortedDictionary<TKey, int> sortedCountmap = new(_counter);
            foreach ((TKey key, int value) in sortedCountmap)
            {
                stream.WriteLine($"{key}\t{value}");
            }
        }
    }
}
