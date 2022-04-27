using System.Collections.Generic;

namespace RunningStatistics;

public interface IRunningStatistic<in TObs, in TSelf> : IMergeable<TSelf>
{
    public long Count { get; }
    
    public void Fit(IEnumerable<TObs> values);
    
    public void Fit(TObs value);
    
    public void Reset();
}