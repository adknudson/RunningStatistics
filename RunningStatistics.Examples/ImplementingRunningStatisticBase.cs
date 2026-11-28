namespace RunningStatistics.Examples;

/// <summary>
/// Tracks the unique observations and the number of observations.
/// </summary>
public class UniqueObservations : RunningStatisticBase<int, UniqueObservations>
{
    private long _nobs;
    private readonly HashSet<int> _uniqueValues = [];


    public override long Nobs => _nobs;
    
    /// <summary>
    /// The unique values observed.
    /// </summary>
    public IEnumerable<int> Values => _uniqueValues;

    /// <summary>
    /// The number of unique values observed.
    /// </summary>
    public int Count => _uniqueValues.Count;

    public override void Fit(int value)
    {
        UncheckedFit(value, 1);
    }

    public override void UncheckedFit(int value)
    {
        UncheckedFit(value, 1);
    }
    
    public override void Fit(int value, long count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    public override void UncheckedFit(int value, long count)
    {
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

    public override string ToString()
    {
        return base.ToString() + $" | Unique Observations: {Count:N0}";
    }
}