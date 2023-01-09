using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class SumLong : Sum<long>
{
    public double Mean => (double) Value / Nobs;
    
    
    
    public override void Fit(IEnumerable<long> values)
    {
        var vs = values.ToList();
        Nobs += vs.Count;
        Value += vs.Sum();
    }
}