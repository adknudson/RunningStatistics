using System.Collections.Generic;
using System.Linq;
using RunningStatistics.UnsafeStatistics;

namespace RunningStatistics;

/// <summary>
/// Tracks the univariate mean.
/// </summary>
public sealed class Mean : RunningStatisticBase<double, Mean>
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


    public double Value => Nobs == 0 ? double.NaN : _mean.Value;


    protected override long GetNobs()
    {
        return _mean.Nobs;
    }

    public override void Fit(double value)
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

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        ys.ForEach(Require.Finite);
        _mean.Fit(ys);
    }

    public override void Reset()
    {
        _mean.Reset();
    }

    public override Mean CloneEmpty()
    {
        return new Mean();
    }

    public override Mean Clone()
    {
        return new Mean(this);
    }

    public override void Merge(Mean other)
    {
        _mean.Merge(other._mean);
    }
    
    public static explicit operator double(Mean mean) => mean.Value;

    public override string ToString() => $"{typeof(Mean)} Nobs={Nobs} | μ={Value}";
}