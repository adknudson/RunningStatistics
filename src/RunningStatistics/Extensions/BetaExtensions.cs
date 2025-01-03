using System;
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RunningStatistics;

public static class BetaExtensions
{
    public static double Pdf(this Beta beta, double x)
    {
        return BetaPdf(beta.Successes, beta.Failures, x);
    }
    
    private static double BetaPdf(double a, double b, double x)
    {
        if (x is < 0 or > 1)
        {
            return 0.0;
        }
            
        // x is now in [0, 1]
        
        if (a == 0 && b == 0)
        {
            return x is 0 or 1 ? double.PositiveInfinity : 0.0;
        }

        if (a == 0)
        {
            return x == 0 ? double.PositiveInfinity : 0;
        }

        if (b == 0)
        {
            return x == 1 ? double.PositiveInfinity : 0;
        }

        if (a == 1 && b == 1)
        {
            return 1;
        }

        if (a > 80 || b > 80)
        {
            return Math.Exp(BetaPdfLn(a, b, x));
        }

        var bb = SpecialFunctions.Gamma(a + b) / (SpecialFunctions.Gamma(a) * SpecialFunctions.Gamma(b));
        return bb * Math.Pow(x, a - 1.0) * Math.Pow(1.0 - x, b - 1.0);
    }
    
    private static double BetaPdfLn(double a, double b, double x)
    {
        if (x is < 0 or > 1)
        {
            return double.NegativeInfinity;
        }

        if (a == 0 && b == 0)
        {
            return x is 0 or 1 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (a == 0)
        {
            return x == 0 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (b == 0)
        {
            return x == 1 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        if (a == 1 && b == 1)
        {
            return 0;
        }

        var aa = SpecialFunctions.GammaLn(a + b) - SpecialFunctions.GammaLn(a) - SpecialFunctions.GammaLn(b);
        var bb = x == 0.0 ? a == 1 ? 0.0 : double.NegativeInfinity : (a - 1.0) * Math.Log(x);
        var cc = x == 1.0 ? b == 1 ? 0.0 : double.NegativeInfinity : (b - 1.0) * Math.Log(1.0 - x);

        return aa + bb + cc;
    }
    
    public static double Cdf(this Beta beta, double x)
    {
        if (beta is { Successes: 0, Failures: 0 })
        {
            throw new ArgumentException("CDF is undefined for both a = 0 and b = 0"); 
        }
        return BetaCdf(beta.Successes, beta.Failures, x);
    }
    
    private static double BetaCdf(double a, double b, double x)
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
        
        if (a == 0)
        {
            return 1;
        }

        if (b == 0)
        {
            return 0;
        }

        if (a == 1 && b == 1)
        {
            return x;
        }

        return SpecialFunctions.UnsafeBetaRegularized(a, b, x);
    }
    
    public static double Quantile(this Beta beta, double p)
    {
        Require.ValidProbability(p);
        return BetaInvCdf(beta.Successes, beta.Failures, p);
    }
    
    private static double BetaInvCdf(double a, double b, double p)
    {
        if (a == 0 && b > 0)
        {
            return 0;
        }
        
        if (b == 0 && a > 0)
        {
            return p == 0 ? 0 : 1;
        }
        
        return RootFinding.FindRoot(
            x => SpecialFunctions.UnsafeBetaRegularized(a, b, x) - p,
            lowerBound: 0.0,
            upperBound: 1.0,
            accuracy: 1e-12,
            maxIterations: 100);
    }
}