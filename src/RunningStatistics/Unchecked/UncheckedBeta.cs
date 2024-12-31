using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RunningStatistics.Unchecked;

public sealed class UncheckedBeta : RunningStatisticBase<bool, UncheckedBeta>
{
    private long _a, _b;
    
    
    public long Successes => _a;

    public long Failures => _b;

    public double Mean => (double)Successes / Nobs;

    public double Median => _a > 1 && _b > 1 ? (_a - 0.33333333333333331) / (Nobs - 0.66666666666666663) : double.NaN;
    
    public double Mode
    {
        get
        {
            if (_a >  1 && _b >  1) return (double) (_a - 1) / (Nobs - 2);
            if (_a == 0 && _b  > 1) return 0.0;
            if (_a >  1 && _b == 0) return 1.0;

            return double.NaN;
        }
    }
    
    public double Variance => (double) _a * _b / (Nobs * Nobs * (Nobs + 1));


    protected override long GetNobs()
    {
        return Successes + Failures;
    }

    public override void Fit(bool value, long count)
    {
        if (value)
        {
            _a += count;
        }
        else
        {
            _b += count;
        }
    }

    public void Fit(long successes, long failures)
    {
        _a += successes;
        _b += failures;
    }

    public override void Fit(IEnumerable<bool> values)
    {
        var bs = values.ToList();
        var n = bs.Count;
        var s = bs.Count(b => b);
        Fit(s, n - s);
    }

    public override void Reset()
    {
        _a = 0;
        _b = 0;
    }

    public override UncheckedBeta CloneEmpty()
    {
        return new UncheckedBeta();
    }
    
    public override void Merge(UncheckedBeta other)
    {
        _a += other.Successes;
        _b += other.Failures;
    }
    
    public double Pdf(double x)
    {
        if (x is < 0 or > 1)
        {
            return 0.0;
        }
            
        // x is now in [0, 1]
        
        if (_a == 0 && _b == 0)
        {
            return x is 0 or 1 ? double.PositiveInfinity : 0.0;
        }

        if (_a == 0)
        {
            return x == 0 ? double.PositiveInfinity : 0;
        }

        if (_b == 0)
        {
            return x == 1 ? double.PositiveInfinity : 0;
        }

        if (_a == 1 && _b == 1)
        {
            return 1;
        }

        if (_a > 80 || _b > 80)
        {
            return Math.Exp(PdfLn(x));
        }

        var bb = SpecialFunctions.Gamma(_a + _b) / (SpecialFunctions.Gamma(_a) * SpecialFunctions.Gamma(_b));
        return bb * Math.Pow(x, _a - 1.0) * Math.Pow(1.0 - x, _b - 1.0);
    }
    
    private double PdfLn(double x)
    {
        if (x is < 0 or > 1)
        {
            return double.NegativeInfinity;
        }

        if (_a == 0 && _b == 0)
        {
            return x is 0 or 1 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (_a == 0)
        {
            return x == 0 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (_b == 0)
        {
            return x == 1 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (_a == 1 && _b == 1)
        {
            return 0;
        }

        var aa = SpecialFunctions.GammaLn(_a + _b) - SpecialFunctions.GammaLn(_a) - SpecialFunctions.GammaLn(_b);
        var bb = x == 0.0 ? _a == 1 ? 0.0 : double.NegativeInfinity : (_a - 1.0) * Math.Log(x);
        var cc = x == 1.0 ? _b == 1 ? 0.0 : double.NegativeInfinity : (_b - 1.0) * Math.Log(1.0 - x);

        return aa + bb + cc;
    }
    
    public double Cdf(double x)
    {
        if (x < 0)
        {
            return 0;
        }

        if (x >= 1)
        {
            return 1;
        }
            
        // x is now in [0, 1)
        
        if (_a == 0)
        {
            return 1;
        }

        if (_b == 0)
        {
            return 0;
        }

        if (_a == 1 && _b == 1)
        {
            return x;
        }

        return SpecialFunctions.UnsafeBetaRegularized(_a, _b, x);
    }

    public double Quantile(double p)
    {
        return RootFinding.FindRoot(
            x => SpecialFunctions.UnsafeBetaRegularized(Successes, Failures, x) - p,
            0.0,
            1.0,
            accuracy: 1e-12);
    }

    protected override string GetStatsString() => $"α={Successes}, β={Failures}";
}