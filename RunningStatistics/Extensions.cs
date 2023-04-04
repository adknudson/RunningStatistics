using System.Linq;

namespace RunningStatistics;

public static class Extensions
{
    #region Sum
    
    public static double Mean(this Sum<double> sum)
    {
        return sum.Value / sum.Nobs;
    }
    
    public static double Mean(this Sum<int> sum)
    {
        return (double) sum.Value / sum.Nobs;
    }
    
    public static double Mean(this Sum<long> sum)
    {
        return (double) sum.Value / sum.Nobs;
    }
    
    public static decimal Mean(this Sum<decimal> sum)
    {
        return sum.Value / sum.Nobs;
    }
    
    #endregion

    
    #region CountMap
    
    public static T Mode<T>(this CountMap<T> countMap) where T : notnull
    {
        return countMap.MaxBy(b => b.Value).Key;
    }

    public static double Mean(this CountMap<double> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }

    public static double Mean(this CountMap<int> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }
    
    public static double Mean(this CountMap<long> countMap)
    {
        return countMap.Sum(kvp => (double) kvp.Key * kvp.Value / countMap.Nobs);
    }

    public static decimal Mean(this CountMap<decimal> countMap)
    {
        return countMap.Sum(kvp => kvp.Key * kvp.Value / countMap.Nobs);
    }
    
    #endregion
}