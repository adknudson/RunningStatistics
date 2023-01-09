using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RunningStatistics;

public class CountmapNumeric<TObs> : Countmap<TObs> where TObs : INumber<TObs>
{
    public double Median => throw new NotImplementedException();

    public double Mean => throw new NotImplementedException();

    public KeyValuePair<TObs, long> MinPair => Value.MinBy(k => k.Key);

    public KeyValuePair<TObs, long> MaxPair => Value.MaxBy(k => k.Key);

    public TObs Min => MinPair.Key;

    public long MinCount => MinPair.Value;
    
    public TObs Max => MaxPair.Key;

    public long MaxCount => MaxPair.Value;
}