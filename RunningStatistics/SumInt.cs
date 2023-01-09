using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class SumInt : Sum<int>
{
    public double Mean => (double) Value / Nobs;
    
    
    
    public override void Fit(IEnumerable<int> values)
    {
        var vs = values.ToList();
        Nobs += vs.Count;
        Value += vs.Sum();
    }
}