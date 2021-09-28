﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineStatistics
{
    public class Countmap : TypedStatistic<Dictionary<double, int>>
    {
        private readonly Dictionary<double, int> _counter;



        public override Dictionary<double, int> Value { get => _counter; }



        public Countmap() : base()
        {
            _counter = new();
        }
        public Countmap(Countmap a) : base(a)
        {
            _counter = new Dictionary<double, int>(a._counter);
        }



        public override void Fit(double y)
        {
            Fit(y, 1);
        }
        private void Fit(double y, int k)
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
        public void Merge(Countmap b)
        {
            foreach (KeyValuePair<double, int> kvp in b._counter)
            {
                Fit(kvp.Key, kvp.Value);
            }
        }



        public static Countmap Merge(Countmap a, Countmap b)
        {
            Countmap merged = new(a);
            merged.Merge(b);
            return merged;
        }
        public static Countmap operator +(Countmap a, Countmap b)
        {
            return Merge(a, b);
        }
    }
}
