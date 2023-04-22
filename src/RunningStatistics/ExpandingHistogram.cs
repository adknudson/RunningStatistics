using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public class ExpandingHistogram : AbstractRunningStatistic<double, ExpandingHistogram>
{
    private HistogramStepRange _edges;
    private IList<int> _counts;



    /// <summary>
    /// An adaptive histogram where the bin edges keep doubling in size in order to contain every observation. Bins are
    /// left-closed and the rightmost bin is closed, e.g. <c>[a, b), [b, c), [c, d]</c>.
    /// </summary>
    /// <param name="numBins">The initial number of bins. Must be a positive even number.</param>
    public ExpandingHistogram(int numBins = 200)
    {
        _edges = new HistogramStepRange(0, 0, numBins + 1);
        _counts = Enumerable.Repeat(0, numBins).ToList();
    }



    public int NumBins => _counts.Count;
    
    
    
    public override void Fit(double value)
    {
        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override ExpandingHistogram CloneEmpty()
    {
        throw new System.NotImplementedException();
    }

    public override ExpandingHistogram Clone()
    {
        throw new System.NotImplementedException();
    }

    public override void Merge(ExpandingHistogram other)
    {
        throw new System.NotImplementedException();
    }
    
    public override string ToString() => $"{typeof(ExpandingHistogram)} Nobs={Nobs} | NumBins={NumBins}";
}