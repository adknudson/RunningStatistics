using System;
using System.Collections.Generic;
using Xunit;

namespace RunningStatistics.Tests;

/// <summary>
/// Test the common properties of all running statistics.
/// </summary>
/// <param name="observationFactory">Method to generate an observation for the running statistic.</param>
/// <param name="generateStatistic">Method to generate a new instance of the running statistic.</param>
/// <typeparam name="TObs">The type of observations.</typeparam>
/// <typeparam name="TSelf">The concrete type of the running statistic.</typeparam>
public abstract class AbstractRunningStatsTest<TObs, TSelf>(
    Func<TObs> observationFactory,
    Func<TSelf> generateStatistic)
    where TSelf : IRunningStatistic<TObs, TSelf>
{
    private Func<TSelf> GenerateStatistic { get; } = generateStatistic;

    private Func<TObs> GenerateObservation { get; } = observationFactory;


    [Fact]
    public void NewStatistic_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void FitSingleObservation_NobsIsOne()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        var obs = GenerateObservation();
        stat.Fit(obs);
        Assert.Equal(1, stat.Nobs);
    }
    
    [Fact]
    public void FitSingleObservation_WithZeroCount_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        var obs = GenerateObservation();
        stat.Fit(obs, 0);
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void FitSingleObservation_WithCount_NobsIsCount()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        var obs = GenerateObservation();
        stat.Fit(obs, 10);
        Assert.Equal(10, stat.Nobs);
    }
    
    [Fact]
    public void FitMultipleObservations_NobsIsCount()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        List<TObs> obs =
        [
            GenerateObservation(),
            GenerateObservation(),
            GenerateObservation()
        ];
        
        stat.Fit(obs);
        Assert.Equal(3, stat.Nobs);
    }
    
    [Fact]
    public void FitMultipleObservations_WithCounts_NobsIsSumOfCounts()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        List<KeyValuePair<TObs, long>> obs =
        [
            new(GenerateObservation(), 1),
            new(GenerateObservation(), 2),
            new(GenerateObservation(), 3)
        ];
        
        stat.Fit(obs);
        Assert.Equal(6, stat.Nobs);
    }
    
    [Fact]
    public void FitMultipleObservations_WithSomeZeroCounts_NobsIsSumOfNonZeroCounts()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        List<KeyValuePair<TObs, long>> obs =
        [
            new(GenerateObservation(), 1),
            new(GenerateObservation(), 0),
            new(GenerateObservation(), 5)
        ];
        
        stat.Fit(obs);
        Assert.Equal(6, stat.Nobs);
    }
    
    [Fact]
    public void Reset_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
        Assert.Equal(3, stat.Nobs);
        
        stat.Reset();
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void CloneEmpty_InterfaceMethodReturnsInterfaceType()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        // pre-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(stat);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(clone);
    }
    
    [Fact]
    public void CloneEmpty_NobsIsZero()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.Equal(0, clone.Nobs);
    }
    
    [Fact]
    public void CloneEmpty_IsNotSameInstance()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        var clone = stat.CloneEmpty();
        Assert.NotSame(stat, clone);
    }
    
    [Fact]
    public void CloneEmpty_DoesNotShareState()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        var clone = stat.CloneEmpty();
        
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(0, clone.Nobs);
        
        clone.Fit(GenerateObservation());
        clone.Fit(GenerateObservation());
        
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(2, clone.Nobs);
    }
    
    [Fact]
    public void Clone_InterfaceMethodReturnsInterfaceType()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        // pre-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(stat);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.IsAssignableFrom<IRunningStatistic<TObs>>(clone);
    }
    
    [Fact]
    public void Clone_NobsIsSame()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
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
        IRunningStatistic<TObs> stat = GenerateStatistic();
        var clone = stat.Clone();
        Assert.NotSame(stat, clone);
    }
    
    [Fact]
    public void Clone_DoesNotShareState()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
        var clone = stat.Clone();
        
        // pre-conditions
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(3, clone.Nobs);
        
        clone.Fit(GenerateObservation());
        clone.Fit(GenerateObservation());
        
        // post-conditions
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(5, clone.Nobs);
    }
    
    [Fact]
    public void UnsafeMerge_NobsIsSumOfCounts()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        IRunningStatistic<TObs> other = GenerateStatistic();
        
        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
        other.Fit(GenerateObservation());
        other.Fit(GenerateObservation());
        other.Fit(GenerateObservation());
        
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
        IRunningStatistic<TObs> stat = GenerateStatistic();
        IRunningStatistic<TObs> other = GenerateStatistic();

        stat.Fit(GenerateObservation());
        stat.Fit(GenerateObservation());
        
        other.Fit(GenerateObservation());
        other.Fit(GenerateObservation());
        other.Fit(GenerateObservation());
        
        // pre-conditions
        Assert.Equal(3, other.Nobs);
        
        stat.UnsafeMerge(other);
        
        // post-conditions
        Assert.Equal(3, other.Nobs);
    }
    
    [Fact]
    public void CLoneEmpty_ConcreteTypeIsSame()
    {
        var stat = GenerateStatistic();
        
        // pre-condition
        Assert.IsAssignableFrom<TSelf>(stat);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.IsAssignableFrom<TSelf>(clone);
    }
    
    [Fact]
    public void Clone_ConcreteTypeIsSame()
    {
        var stat = GenerateStatistic();
        
        // pre-condition
        Assert.IsAssignableFrom<TSelf>(stat);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.IsAssignableFrom<TSelf>(clone);
    }
    
    [Fact]
    public void Merge_ConcreteTypeIsSame()
    {
        var stat = GenerateStatistic();
        var other = GenerateStatistic();
        
        // pre-condition
        Assert.IsAssignableFrom<TSelf>(stat);
        Assert.IsAssignableFrom<TSelf>(other);
        
        stat.Merge(other);
    }
}