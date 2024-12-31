using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests;

public abstract class AbstractRunningStatsTest<TObs, TSelf> where TSelf : IRunningStatistic<TObs, TSelf>
{
    protected AbstractRunningStatsTest(
        Func<TSelf> runningStatFactory,
        Func<TObs> observationFactory)
    {
        RunningStatFactory = runningStatFactory;
        GenerateNextObservation = observationFactory;
    }

    
    private Func<TSelf> RunningStatFactory { get; }
    
    private Func<TObs> GenerateNextObservation { get; }
    
    
    [Fact]
    public void NewStatistic_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void FitSingleObservation_NobsIsOne()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        var obs = GenerateNextObservation();
        stat.Fit(obs);
        Assert.Equal(1, stat.Nobs);
    }
    
    [Fact]
    public void FitSingleObservation_WithZeroCount_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        var obs = GenerateNextObservation();
        stat.Fit(obs, 0);
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void FitSingleObservation_WithCount_NobsIsCount()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        var obs = GenerateNextObservation();
        stat.Fit(obs, 10);
        Assert.Equal(10, stat.Nobs);
    }
    
    [Fact]
    public void FitMultipleObservations_NobsIsCount()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        List<TObs> obs =
        [
            GenerateNextObservation(),
            GenerateNextObservation(),
            GenerateNextObservation()
        ];
        
        stat.Fit(obs);
        Assert.Equal(3, stat.Nobs);
    }
    
    [Fact]
    public void FitMultipleObservations_WithCounts_NobsIsSumOfCounts()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        List<KeyValuePair<TObs, long>> obs =
        [
            new(GenerateNextObservation(), 1),
            new(GenerateNextObservation(), 2),
            new(GenerateNextObservation(), 3)
        ];
        
        stat.Fit(obs);
        Assert.Equal(6, stat.Nobs);
    }
    
    [Fact]
    public void FitMultipleObservations_WithSomeZeroCounts_NobsIsSumOfNonZeroCounts()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        List<KeyValuePair<TObs, long>> obs =
        [
            new(GenerateNextObservation(), 1),
            new(GenerateNextObservation(), 0),
            new(GenerateNextObservation(), 5)
        ];
        
        stat.Fit(obs);
        Assert.Equal(6, stat.Nobs);
    }
    
    [Fact]
    public void Reset_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        Assert.Equal(3, stat.Nobs);
        
        stat.Reset();
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void CloneEmpty_InterfaceMethodReturnsInterfaceType()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        // pre-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(stat);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(clone);
    }
    
    [Fact]
    public void CloneEmpty_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.Equal(0, clone.Nobs);
    }
    
    [Fact]
    public void CloneEmpty_IsNotSameInstance()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        var clone = stat.CloneEmpty();
        Assert.NotSame(stat, clone);
    }
    
    [Fact]
    public void CloneEmpty_DoesNotShareState()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        var clone = stat.CloneEmpty();
        
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(0, clone.Nobs);
        
        clone.Fit(GenerateNextObservation());
        clone.Fit(GenerateNextObservation());
        
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(2, clone.Nobs);
    }
    
    [Fact]
    public void Clone_InterfaceMethodReturnsInterfaceType()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        // pre-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(stat);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(clone);
    }
    
    [Fact]
    public void Clone_NobsIsSame()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(3, clone.Nobs);
    }
    
    [Fact]
    public void Clone_IsNotSameInstance()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        var clone = stat.Clone();
        Assert.NotSame(stat, clone);
    }
    
    [Fact]
    public void Clone_DoesNotShareState()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        var clone = stat.Clone();
        
        // pre-conditions
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(3, clone.Nobs);
        
        clone.Fit(GenerateNextObservation());
        clone.Fit(GenerateNextObservation());
        
        // post-conditions
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(5, clone.Nobs);
    }
    
    [Fact]
    public void UnsafeMerge_NobsIsSumOfCounts()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        IRunningStatistic<TObs> other = RunningStatFactory();
        
        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        other.Fit(GenerateNextObservation());
        other.Fit(GenerateNextObservation());
        other.Fit(GenerateNextObservation());
        
        // pre-conditions
        Assert.Equal(2, stat.Nobs);
        Assert.Equal(3, other.Nobs);
        
        stat.UnsafeMerge(other);
        
        // post-conditions
        Assert.Equal(5, stat.Nobs);
    }
    
    [Fact]
    public void UnsafeMerge_DoesNotAffectOther()
    {
        IRunningStatistic<TObs> stat = RunningStatFactory();
        IRunningStatistic<TObs> other = RunningStatFactory();

        stat.Fit(GenerateNextObservation());
        stat.Fit(GenerateNextObservation());
        
        other.Fit(GenerateNextObservation());
        other.Fit(GenerateNextObservation());
        other.Fit(GenerateNextObservation());
        
        // pre-conditions
        Assert.Equal(3, other.Nobs);
        
        stat.UnsafeMerge(other);
        
        // post-conditions
        Assert.Equal(3, other.Nobs);
    }
    
    [Fact]
    public void CLoneEmpty_ConcreteTypeIsSame()
    {
        var stat = RunningStatFactory();
        
        // pre-condition
        Assert.IsAssignableFrom<TSelf>(stat);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.IsAssignableFrom<TSelf>(clone);
    }
    
    [Fact]
    public void Clone_ConcreteTypeIsSame()
    {
        var stat = RunningStatFactory();
        
        // pre-condition
        Assert.IsAssignableFrom<TSelf>(stat);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.IsAssignableFrom<TSelf>(clone);
    }
    
    [Fact]
    public void Merge_ConcreteTypeIsSame()
    {
        var stat = RunningStatFactory();
        var other = RunningStatFactory();
        
        // pre-condition
        Assert.IsAssignableFrom<TSelf>(stat);
        Assert.IsAssignableFrom<TSelf>(other);
        
        stat.Merge(other);
    }
}