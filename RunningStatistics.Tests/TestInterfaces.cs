using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RunningStatistics.Tests;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
/// Verifies the IRunningStatistic interface members through each of its type-parameter
/// shapes: the bare interface, IRunningStatistic<double> (TObs) and IRunningStatistic<double,
/// RunningStatistics.Mean> (TObs and TSelf).
public class TestInterfaces
{
    [Fact]
    public void Nobs_ViaBareInterface()
    {
        var mean = new RunningStatistics.Mean();
        IRunningStatistic stat = mean;
        Assert.Equal(0, stat.Nobs);
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        Assert.Equal(10, stat.Nobs);
    }

    [Fact]
    public void Reset_ViaBareInterface()
    {
        var mean = new RunningStatistics.Mean();
        IRunningStatistic stat = mean;
        Assert.Equal(0, stat.Nobs);
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        Assert.Equal(10, stat.Nobs);
        stat.Reset();
        Assert.Equal(0, stat.Nobs);
    }

    [Fact]
    public void CloneEmpty_ViaBareInterface()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        IRunningStatistic stat = mean;
        var statCloneEmpty = stat.CloneEmpty();
        Assert.IsType<IRunningStatistic>(statCloneEmpty, exactMatch: false);
        Assert.Equal(0, statCloneEmpty.Nobs);
        Assert.IsType<RunningStatistics.Mean>(statCloneEmpty);
    }

    [Fact]
    public void Clone_ViaBareInterface()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        IRunningStatistic stat = mean;
        var statClone = stat.Clone();
        Assert.IsType<IRunningStatistic>(statClone, exactMatch: false);
        Assert.Equal(10, statClone.Nobs);
        Assert.IsType<RunningStatistics.Mean>(statClone);
    }

    [Fact]
    public void UnsafeMerge_ViaBareInterface()
    {
        var m1 = new RunningStatistics.Mean();
        var m2 = new RunningStatistics.Mean();
        var cm = new CountMap<double>();
        
        m1.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        m2.Fit([-5, -4, -3, -2, -1, 1, 2, 3, 4, 5]);
        cm.Fit([0, 1, 1, 2, 3, 5, 8]);

        IRunningStatistic s1 = m1;
        IRunningStatistic s2 = m2;
        IRunningStatistic s3 = cm;

        s1.UnsafeMerge(s2);
        Assert.Equal(20, s1.Nobs);
        Assert.Equal(20, m1.Nobs);
        Assert.Equal(10, s2.Nobs);
        Assert.Equal(10, m2.Nobs);

        Assert.Throws<InvalidCastException>(() => s1.UnsafeMerge(s3));
    }

    [Fact]
    public void CloneEmpty_ViaTObsInterface()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        
        IRunningStatistic<double> stat = mean;
        var statCloneEmpty = stat.CloneEmpty();
        Assert.IsType<IRunningStatistic>(statCloneEmpty, exactMatch: false);
        Assert.IsType<IRunningStatistic<double>>(statCloneEmpty, exactMatch: false);
        Assert.Equal(0, statCloneEmpty.Nobs);
        Assert.IsType<RunningStatistics.Mean>(statCloneEmpty);
    }
    
    [Fact]
    public void Clone_ViaTObsInterface()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        IRunningStatistic<double> stat = mean;
        var statClone = stat.Clone();
        Assert.IsType<IRunningStatistic>(statClone, exactMatch: false);
        Assert.IsType<IRunningStatistic<double>>(statClone, exactMatch: false);
        Assert.Equal(10, statClone.Nobs);
        Assert.IsType<RunningStatistics.Mean>(statClone);
    }
    
    [Fact]
    public void UnsafeMerge_ViaTObsInterface()
    {
        var m1 = new RunningStatistics.Mean();
        var m2 = new RunningStatistics.Mean();
        var cm = new CountMap<double>();
        
        m1.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        m2.Fit([-5, -4, -3, -2, -1, 1, 2, 3, 4, 5]);
        cm.Fit([0, 1, 1, 2, 3, 5, 8]);

        IRunningStatistic<double> s1 = m1;
        IRunningStatistic<double> s2 = m2;
        IRunningStatistic<double> s3 = cm;

        s1.UnsafeMerge(s2);
        Assert.Equal(20, s1.Nobs);
        Assert.Equal(20, m1.Nobs);
        Assert.Equal(10, s2.Nobs);
        Assert.Equal(10, m2.Nobs);

        Assert.Throws<InvalidCastException>(() => s1.UnsafeMerge(s3));
    }
    
    [Fact]
    public void CloneEmpty_ViaTObsTSelfInterface()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        
        IRunningStatistic<double, RunningStatistics.Mean> stat = mean;
        var clone = stat.CloneEmpty();
        Assert.IsType<IRunningStatistic>(clone, exactMatch: false);
        Assert.IsType<IRunningStatistic<double>>(clone, exactMatch: false);
        Assert.Equal(0, clone.Nobs);
        Assert.IsType<RunningStatistics.Mean>(clone);
    }
    
    [Fact]
    public void Clone_ViaTObsTSelfInterface()
    {
        var mean = new RunningStatistics.Mean();
        mean.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        IRunningStatistic<double, RunningStatistics.Mean> stat = mean;
        var clone = stat.Clone();
        Assert.IsType<IRunningStatistic>(clone, exactMatch: false);
        Assert.IsType<IRunningStatistic<double>>(clone, exactMatch: false);
        Assert.IsType<IRunningStatistic<double, RunningStatistics.Mean>>(clone, exactMatch: false);
        Assert.Equal(10, clone.Nobs);
        Assert.IsType<RunningStatistics.Mean>(clone);
    }
    
    [Fact]
    public void Merge_ViaTObsTSelfInterface()
    {
        var m1 = new RunningStatistics.Mean();
        var m2 = new RunningStatistics.Mean();
        
        m1.Fit([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        m2.Fit([-5, -4, -3, -2, -1, 1, 2, 3, 4, 5]);

        IRunningStatistic<double, RunningStatistics.Mean> s1 = m1;
        IRunningStatistic<double, RunningStatistics.Mean> s2 = m2;

        s1.Merge(m1);
        Assert.Equal(20, s1.Nobs);
        Assert.Equal(20, m1.Nobs);
        Assert.Equal(10, s2.Nobs);
        Assert.Equal(10, m2.Nobs);
    }
}