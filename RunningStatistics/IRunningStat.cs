using System.Collections.Generic;
using System.IO;

namespace RunningStatistics;

public interface IRunningStat<in TObs, in TSelf> : IMergeable<TSelf>
{
    public long Count { get; }
    
    public void Fit(IEnumerable<TObs> values);
    
    public void Fit(TObs value);
    
    public void Reset();
    
    public void Print(StreamWriter stream);
}