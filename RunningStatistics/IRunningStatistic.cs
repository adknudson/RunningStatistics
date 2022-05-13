using System.Collections.Generic;
using System.IO;

namespace RunningStatistics;

public interface IRunningStatistic<TObs>
{
    public nint Count { get; }

    public void Fit(IEnumerable<TObs> values);

    public void Fit(TObs value);

    public void Reset();
    
    public void Merge(IRunningStatistic<TObs> other);

    public void Print(StreamWriter stream);
}