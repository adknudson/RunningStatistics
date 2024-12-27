using RunningStatistics.Tests.Extras;
using Xunit;

namespace RunningStatistics.Tests;

public class TestExtremaGeneric
{
    [Fact]
    public void FitUpdatesMinAndMaxCorrectlyForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.Equal(new MyNum(3.0m), extrema.Min);
        Assert.Equal(new MyNum(7.0m), extrema.Max);
    }

    [Fact]
    public void FitUpdatesMinCountAndMaxCountCorrectlyForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));
        extrema.Fit(new MyNum(7.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.Equal(2, extrema.MinCount);
        Assert.Equal(3, extrema.MaxCount);
    }

    [Fact]
    public void FitUpdatesNobsCorrectlyForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(5.0m));
        extrema.Fit(new MyNum(3.0m));
        extrema.Fit(new MyNum(7.0m));

        Assert.Equal(3, extrema.Nobs);
    }
        
    [Fact]
    public void FitHandlesExtremeValuesForMyNum()
    {
        Extrema<MyNum> extrema = new();
        extrema.Fit(new MyNum(decimal.MaxValue));
        extrema.Fit(new MyNum(decimal.MinValue));

        Assert.Equal(new MyNum(decimal.MinValue), extrema.Min);
        Assert.Equal(new MyNum(decimal.MaxValue), extrema.Max);
        Assert.Equal(1, extrema.MinCount);
        Assert.Equal(1, extrema.MaxCount);
        Assert.Equal(2, extrema.Nobs);
    }

    [Fact]
    public void MergeCombinesMinAndMaxCorrectlyForMyNum()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));

        Extrema<MyNum> b = new();
        b.Fit(new MyNum(0.5m));
        b.Fit(new MyNum(3.0m));

        a.Merge(b);

        Assert.Equal(new MyNum(0.5m), a.Min);
        Assert.Equal(new MyNum(3.0m), a.Max);
    }

    [Fact]
    public void MergeCombinesMinCountAndMaxCountCorrectlyForMyNum()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));

        Extrema<MyNum> b = new();
        b.Fit(new MyNum(1.0m));
        b.Fit(new MyNum(3.0m));
        b.Fit(new MyNum(3.0m));

        a.Merge(b);

        Assert.Equal(3, a.MinCount);
        Assert.Equal(2, a.MaxCount);
    }

    [Fact]
    public void MergeCombinesNobsCorrectlyForMyNum()
    {
        Extrema<MyNum> a = new();
        a.Fit(new MyNum(1.0m));
        a.Fit(new MyNum(2.0m));

        Extrema<MyNum> b = new();
        b.Fit(new MyNum(0.5m));
        b.Fit(new MyNum(3.0m));

        a.Merge(b);

        Assert.Equal(4, a.Nobs);
    }
}