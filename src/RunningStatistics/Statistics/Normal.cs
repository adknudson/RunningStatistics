using System;
using System.Collections.Generic;
using System.Linq;

namespace RunningStatistics;

public sealed class Normal : RunningStatisticBase<double, Normal>
{
    private readonly Mean _mean = new();
    private readonly Variance _variance = new();


    public double Mean => _mean.Value;
    
    public double Variance => _variance.Value;
    
    public double StandardDeviation => Math.Sqrt(Variance);
    
    
    protected override long GetNobs() => _mean.Nobs;

    public override void Fit(double value, long count)
    {
        Require.Finite(value);
        Require.NonNegative(count);
        if (count == 0) return;
        UncheckedFit(value, count);
    }

    public override void Fit(IEnumerable<double> values)
    {
        var ys = values.ToList();
        ys.ForEach(Require.Finite);
        UncheckedFit(ys);
    }
    
    private void UncheckedFit(double value, long count)
    {
        _mean.UncheckedFit(value, count);
        _variance.UncheckedFit(value, count);
    }

    private void UncheckedFit(List<double> values)
    {
        _mean.UncheckedFit(values);
        _variance.UncheckedFit(values);
    }

    public override void Reset()
    {
        _mean.Reset();
        _variance.Reset();
    }

    public override Normal CloneEmpty() => new();
    
    public override void Merge(Normal normal)
    {
        _mean.Merge(normal._mean);
        _variance.Merge(normal._variance);
    }

    public double Pdf(double x)
    {
        var d = (x - Mean) / StandardDeviation;
        return Math.Exp(-0.5 * d * d) / (Constants.Sqrt2Pi * StandardDeviation);
    }

    public double Cdf(double x)
    {
        return 0.5 * SpecialFunctions.Erfc((Mean - x) / (StandardDeviation * Constants.Sqrt2));
    }

    public double Quantile(double p)
    {
        Require.ValidProbability(p);
        return Mean - StandardDeviation * Constants.Sqrt2 * SpecialFunctions.ErfcInv(2.0 * p);
    }

    protected override string GetStatsString() => $"μ={Mean}, σ²={Variance}";
}