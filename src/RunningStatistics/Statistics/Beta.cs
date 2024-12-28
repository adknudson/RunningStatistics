using System;
using System.Collections.Generic;
using RunningStatistics.UnsafeStatistics;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace RunningStatistics;

public sealed class Beta : IRunningStatistic<bool, Beta>
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


    public long Nobs => _beta.Nobs;
    
    public long Successes => _beta.Successes;

    public long Failures => _beta.Failures;

    public double Mean => Nobs == 0 ? double.NaN : _beta.Mean;

    public double Median => _beta.Median;

    public double Mode => _beta.Mode;

    public double Variance => _beta.Variance;


    public void Fit(bool success)
    {
        _beta.Fit(success);
    }
    
    public void Fit(long successes, long failures)
    {
        Require.NonNegative(successes);
        Require.NonNegative(failures);
        _beta.Fit(successes, failures);
    }

    public void Fit(IEnumerable<bool> values)
    {
        _beta.Fit(values);
    }

    public void Reset()
    {
        _beta.Reset();
    }

    IRunningStatistic<bool> IRunningStatistic<bool>.CloneEmpty()
    {
        return CloneEmpty();
    }

    IRunningStatistic<bool> IRunningStatistic<bool>.Clone()
    {
        return Clone();
    }

    public void Merge(IRunningStatistic<bool> other)
    {
        var beta = Require.Type<Beta>(other);
        Merge(beta);
    }

    public Beta CloneEmpty() => new();

    public Beta Clone() => new(Successes, Failures);

    public void Merge(Beta other)
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
}