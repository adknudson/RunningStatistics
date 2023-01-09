using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class SumDouble : Sum<double>
{
    public double Mean => Value / Nobs;
    
    
    
    public override void Fit(IEnumerable<double> values)
    {
        var vs = values.ToList();
        Nobs += vs.Count;
        Value += vs.Sum();
    }
}