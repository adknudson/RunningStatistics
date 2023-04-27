using System;
using RunningStatistics.RootFinding;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace RunningStatistics;

public class Beta : AbstractRunningStatistic<bool, Beta>
{
    private long _a, _b;



    public Beta()
    {
        _a = 0;
        _b = 0;
    }

    public Beta(long numHits, long numMisses)
    {
        _a = numHits;
        _b = numMisses;
    }



    public long NumSuccesses => _a;

    public long NumFailures => _b;
    
    public double Mean => Nobs > 0 ? (double)_a / Nobs : double.NaN;

    public double Median => _a > 1 && _b > 1 ? (_a - 0.33333333333333331) / (Nobs - 0.66666666666666663) : double.NaN;

    public double Mode => ComputeMode();
    
    public double Variance => (double) _a * _b / (Nobs * Nobs * (Nobs + 1));

    
    
    protected override long GetNobs() => _a + _b;
    
    public void Fit(long numHits = 0, long numMisses = 0)
    {
        if (numHits < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numHits), "Number of hits must be non-negative");
        }

        if (numMisses < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numMisses), "Number of misses must be non-negative");
        }

        UncheckedFit(numHits, numMisses);
    }

    public override void Fit(bool success)
    {
        if (success)
        {
            _a += 1;
        }
        else
        {
            _b += 1;
        }
    }

    private void UncheckedFit(long numHits, long numMisses)
    {
        _a += numHits;
        _b += numMisses;
    }

    public override void Reset()
    {
        _a = 0;
        _b = 0;
    }

    public override Beta CloneEmpty() => new();

    public override Beta Clone() => new(_a, _b);

    public override void Merge(Beta other)
    {
        _a += other._a;
        _b += other._b;
    }

    private double ComputeMode()
    {
        if (_a >  1 && _b >  1) return (double) (_a - 1) / (Nobs - 2);
        if (_a == 1 && _b == 1) return double.NaN;
        if (_a == 0 && _b  > 1) return 0.0;
        if (_a >  1 && _b == 0) return 1.0;

        return double.NaN;
    }
    
    public double Pdf(double x)
    {
        if (x < 0.0 || x > 1.0)
        {
            return 0.0;
        }
            
        // x is now in [0, 1]
        
        if (_a == 0 && _b == 0)
        {
            if (x == 0.0 || x == 1.0)
            {
                return double.PositiveInfinity;
            }

            return 0.0;
        }

        if (_a == 0)
        {
            return x == 0.0 ? double.PositiveInfinity : 0.0;
        }

        if (_b == 0)
        {
            return x == 1.0 ? double.PositiveInfinity : 0.0;
        }

        if (_a == 1 && _b == 1)
        {
            return 1.0;
        }

        if (_a > 80.0 || _b > 80.0)
        {
            return Math.Exp(PdfLn(x));
        }

        var bb = SpecialFunctions.Gamma(_a + _b) / (SpecialFunctions.Gamma(_a) * SpecialFunctions.Gamma(_b));
        return bb * Math.Pow(x, _a - 1.0) * Math.Pow(1.0 - x, _b - 1.0);
    }
    
    private double PdfLn(double x)
    {
        if (x < 0.0 || x > 1.0)
        {
            return double.NegativeInfinity;
        }

        if (_a == 0 && _b == 0)
        {
            return x == 0.0 || x == 1.0 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (_a == 0)
        {
            return x == 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (_b == 0)
        {
            return x == 1.0 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (_a == 1 && _b == 1)
        {
            return 0.0;
        }

        var aa = SpecialFunctions.GammaLn(_a + _b) - SpecialFunctions.GammaLn(_a) - SpecialFunctions.GammaLn(_b);
        var bb = x == 0.0 ? (_a == 1 ? 0.0 : double.NegativeInfinity) : (_a - 1.0) * Math.Log(x);
        var cc = x == 1.0 ? (_b == 1 ? 0.0 : double.NegativeInfinity) : (_b - 1.0) * Math.Log(1.0 - x);

        return aa + bb + cc;
    }
    
    public double Cdf(double x)
    {
        if (x < 0.0)
        {
            return 0.0;
        }

        if (x >= 1.0)
        {
            return 1.0;
        }
            
        // x is now in [0, 1)

        if (_a == 0 && _b == 0)
        {
            return 0.5;
        }

        if (_a == 0)
        {
            return 1.0;
        }

        if (_b == 0)
        {
            return 0.0;
        }

        if (_a == 1 && _b == 1)
        {
            return x;
        }

        return SpecialFunctions.UnsafeBetaRegularized(_a, _b, x);
    }

    public double Quantile(double p)
    {
        if (p is < 0.0 or > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "Invalid parametrization for the distribution.");
        } 
        
        return Brent.FindRoot(x => SpecialFunctions.UnsafeBetaRegularized(_a, _b, x) - p, 0.0, 1.0, accuracy: 1e-12);
    }
}