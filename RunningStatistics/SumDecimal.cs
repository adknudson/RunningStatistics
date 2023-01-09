using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class SumDecimal : Sum<decimal>
{
    public decimal Mean => Value / Nobs;
    
    
    
    public override void Fit(IEnumerable<decimal> values)
    {
        var vs = values.ToList();
        Nobs += vs.Count;
        Value += vs.Sum();
    }
}