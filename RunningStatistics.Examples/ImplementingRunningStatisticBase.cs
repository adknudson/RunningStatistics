namespace RunningStatistics.Examples;

/// <summary>
/// Tracks the unique observations and the number of observations.
/// </summary>
public class UniqueObservations : RunningStatisticBase<int, UniqueObservations>
{
    private long _nobs;
    private readonly HashSet<int> _uniqueValues = [];

    
    /// <summary>
    /// The unique values observed.
    /// </summary>
    public IEnumerable<int> Values => _uniqueValues;

    /// <summary>
    /// The number of unique values observed.
    /// </summary>
    public int Count => _uniqueValues.Count;
    

    protected override long GetNobs() => _nobs;

    public override void Fit(int value, long count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
        }
        
        if (count == 0) return;
        
        _nobs += count;
        _uniqueValues.Add(value);
    }

    public override void Reset()
    {
        _nobs = 0;
        _uniqueValues.Clear();
    }

    public override UniqueObservations CloneEmpty() => new();

    public override void Merge(UniqueObservations other)
    {
        _nobs += other._nobs;
        _uniqueValues.UnionWith(other._uniqueValues);
    }

    protected override string GetStatsString()
    {
        return $"Unique Observations: {_uniqueValues.Count:N0}";
    }
}