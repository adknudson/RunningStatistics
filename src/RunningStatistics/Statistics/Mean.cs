using System;
using System.Collections.Generic;
using System.Linq;
using RunningStatistics.UnsafeStatistics;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean.
/// </summary>
public sealed class Mean : IRunningStatistic<double, Mean>
{
    private readonly UnsafeMean _mean;


    public Mean()
    {
        _mean = new UnsafeMean();
    }

    private Mean(Mean other)
    {
        _mean = other._mean.Clone();
    }


    public long Nobs => _mean.Nobs;

    public double Value => Nobs == 0 ? double.NaN : _mean.Value;
    
    
    public void Fit(double value)
    {
        Require.Finite(value);
        _mean.Fit(value);
    }

    public void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        _mean.Fit(value, count);
    }

    public void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        ys.ForEach(Require.Finite);
        _mean.Fit(ys);
    }

    public void Reset()
    {
        _mean.Reset();
    }

    public Mean CloneEmpty()
    {
        return new Mean();
    }

    public Mean Clone()
    {
        return new Mean(this);
    }

    public void Merge(Mean other)
    {
        _mean.Merge(other._mean);
    }

    IRunningStatistic<double> IRunningStatistic<double>.CloneEmpty()
    {
        return CloneEmpty();
    }

    IRunningStatistic<double> IRunningStatistic<double>.Clone()
    {
        return Clone();
    }

    public void Merge(IRunningStatistic<double> other)
    {
        if (other is not Mean mean)
        {
            throw new ArgumentException("The other stat must be of type 'Mean'", nameof(other));
        }

        Merge(mean);
    }
    
    public static explicit operator double(Mean mean) => mean.Value;

    public override string ToString() => $"{typeof(Mean)} Nobs={Nobs} | μ={Value}";
}