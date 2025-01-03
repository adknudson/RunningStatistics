using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace RunningStatistics;

/// <summary>
/// Represents a Beta distribution for running statistics.
/// </summary>
public sealed class Beta : RunningStatisticBase<bool, Beta>
{
    private long _a, _b;


    /// <summary>
    /// Initializes a new instance of the <see cref="Beta"/> class.
    /// </summary>
    public Beta()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Beta"/> class with specified number of successes
    /// and failures.
    /// </summary>
    /// <param name="successes">The number of successes. Must be non-negative.</param>
    /// <param name="failures">The number of failures. Must be non-negative.</param>
    public Beta(long successes, long failures)
    {
        Require.NonNegative(successes);
        Require.NonNegative(failures);
        
        _a = successes;
        _b = failures;
    }


    /// <summary>
    /// The number of observed successes.
    /// </summary>
    public long Successes => _a;

    /// <summary>
    /// The number of observed failures.
    /// </summary>
    public long Failures => _b;

    /// <summary>
    /// The mean of the Beta distribution. Exists when there is at least one observation.
    /// </summary>
    public double Mean => Nobs > 0
        ? (double)_a / (_a + _b) 
        : double.NaN;

    /// <summary>
    /// The median of the Beta distribution. Exists when there is at least one observed success
    /// and at least one observed failure.
    /// </summary>
    public double Median => _a > 1 && _b > 1 
        ? this.Quantile(0.5)
        : double.NaN;

    /// <summary>
    /// The mode of the Beta distribution. Exists when there is at least one observed success
    /// and at least one observed failure.
    /// </summary>
    public double Mode => _a > 1 && _b > 1
        ? (double)(_a - 1) / (_a + _b - 2)
        : double.NaN;

    /// <summary>
    /// The variance of the Beta distribution. Exists when there is at least one observed success
    /// and at least one observed failure.
    /// </summary>
    public double Variance => _a > 0 && _b > 0
        ? _a * _b / (Math.Pow(_a + _b, 2) * (_a + _b + 1))
        : double.NaN;


    protected override long GetNobs() => _a + _b;

    /// <summary>
    /// Fits the Beta distribution with a new observation.
    /// </summary>
    /// <param name="value">The observation value (true for success, false for failure).</param>
    /// <param name="count">The count of the observation. Must be non-negative.</param>
    public override void Fit(bool value, long count)
    {
        Require.NonNegative(count);
        
        if (value)
        {
            _a += count;
        }
        else
        {
            _b += count;
        }
    }

    /// <summary>
    /// Fits the Beta distribution with specified number of successes and failures.
    /// </summary>
    /// <param name="successes">The number of successes. Must be non-negative.</param>
    /// <param name="failures">The number of failures. Must be non-negative.</param>
    public void Fit(long successes, long failures)
    {
        Require.NonNegative(successes);
        Require.NonNegative(failures);

        _a += successes;
        _b += failures;
    }

    /// <summary>
    /// Fits the Beta distribution with a collection of boolean values.
    /// </summary>
    /// <param name="values">The collection of boolean values. True for success, false for failure.</param>
    public override void Fit(IEnumerable<bool> values)
    {
        var bs = values.ToList();
        var n = bs.Count;
        var s = bs.Count(b => b);
        Fit(s, n - s);
    }

    /// <summary>
    /// Resets the Beta distribution.
    /// </summary>
    public override void Reset()
    {
        _a = 0;
        _b = 0;
    }

    /// <summary>
    /// Creates a new empty instance of the <see cref="Beta"/> class.
    /// </summary>
    /// <returns>A new empty instance of the <see cref="Beta"/> class.</returns>
    public override Beta CloneEmpty() => new();

    /// <summary>
    /// Merges another Beta distribution into this one.
    /// </summary>
    /// <param name="other">The other Beta distribution to merge.</param>
    public override void Merge(Beta other)
    {
        checked
        {
            _a += other.Successes;
            _b += other.Failures;
        }
    }
    
    protected override string GetStatsString() => $"α={Successes:N0}, β={Failures:N0}";
}