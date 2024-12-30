using System;
using System.Collections.Generic;
using RunningStatistics.UnsafeStatistics;

namespace RunningStatistics;

public sealed class Beta : RunningStatisticBase<bool, Beta>
{
    private readonly UnsafeBeta _beta = new();


    public Beta()
    {
    }
    
    public Beta(long successes, long failures)
    {
        Require.NonNegative(successes);
        Require.NonNegative(failures);
        _beta.Fit(successes, failures);
    }

    
    public long Successes => _beta.Successes;

    public long Failures => _beta.Failures;

    public double Mean => Nobs == 0 ? double.NaN : _beta.Mean;

    public double Median => _beta.Median;

    public double Mode => _beta.Mode;

    public double Variance => _beta.Variance;


    protected override long GetNobs()
    {
        return _beta.Nobs;
    }

    public override void Fit(bool success)
    {
        _beta.Fit(success);
    }
    
    public void Fit(long successes, long failures)
    {
        Require.NonNegative(successes);
        Require.NonNegative(failures);
        _beta.Fit(successes, failures);
    }

    public override void Fit(IEnumerable<bool> values)
    {
        _beta.Fit(values);
    }

    public override void Reset()
    {
        _beta.Reset();
    }

    public override Beta CloneEmpty() => new();

    public override Beta Clone() => new(Successes, Failures);

    public override void Merge(Beta other)
    {
        _beta.Merge(other._beta);
    }
    
    public double Pdf(double x)
    {
        return _beta.Pdf(x);
    }

    public double Cdf(double x)
    {
        if (Successes == 0 && Failures == 0)
        {
            throw new ArgumentException("CDF is undefined for both a = 0 and b = 0"); 
        }

        return _beta.Cdf(x);
    }

    public double Quantile(double p)
    {
        Require.ValidProbability(p);
        return _beta.Quantile(p);
    }

    protected override string GetStatsString()
    {
        return $"α={Successes}, β={Failures}";
    }
}