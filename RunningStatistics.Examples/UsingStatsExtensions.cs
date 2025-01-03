using RunningStatistics.Examples.Extras;

namespace RunningStatistics.Examples;

public static class UsingStatsExtensions
{
    public static void CountMapExample()
    {
        var iCountMap = new CountMap<int>();
        var lCountMap = new CountMap<long>();
        var dCountMap = new CountMap<double>();
        var mCountMap = new CountMap<decimal>();
        var xCountMap = new CountMap<MyNum>();

        var iMin = iCountMap.MinKey();
        var xMin = xCountMap.MinKey();

        var iMax = iCountMap.MaxKey();
        var xMax = xCountMap.MaxKey();
        
        var iS = iCountMap.Sum();
        var lS = lCountMap.Sum();
        var dS = dCountMap.Sum();
        var mS = mCountMap.Sum();
        var xS = xCountMap.Sum();
        
        var iM = iCountMap.Mean();
        var lM = lCountMap.Mean();
        var dM = dCountMap.Mean();
        var mM = mCountMap.Mean();
        var xM = xCountMap.Mean();

        var iV = iCountMap.Variance();
        var lV = lCountMap.Variance();
        var dV = dCountMap.Variance();
        
        var iSk = iCountMap.Skewness();
        var lSk = lCountMap.Skewness();
        var dSk = dCountMap.Skewness();
        
        var iK = iCountMap.Kurtosis();
        var lK = lCountMap.Kurtosis();
        var dK = dCountMap.Kurtosis();
        
        var iE = iCountMap.ExcessKurtosis();
        var lE = lCountMap.ExcessKurtosis();
        var dE = dCountMap.ExcessKurtosis();
    }

    public static void SumExample()
    {
        var sum = new Sum(); // defaults to observations of type 'double'
        var iSum = new Sum<int>();
        var lSum = new Sum<long>();
        var dSum = new Sum<double>();
        var mSum = new Sum<decimal>();
        var xSum = new Sum<MyNum>(); // custom number type that implements required methods

        var sM = sum.Mean();
        var iM = iSum.Mean();
        var lM = lSum.Mean();
        var dM = dSum.Mean();
        var mM = mSum.Mean();
        var xM = xSum.Mean();
    }
}