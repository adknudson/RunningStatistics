using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        Assert.IsType<IRunningStatistic<TObs>>(stat, exactMatch: false);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.IsType<IRunningStatistic<TObs>>(clone, exactMatch: false);
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
        Assert.IsType<IRunningStatistic<TObs>>(stat, exactMatch: false);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.IsType<IRunningStatistic<TObs>>(clone, exactMatch: false);
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
    public void CloneEmpty_ConcreteTypeIsSame()
    {
        var stat = GenerateStatistic();
        
        // pre-condition
        Assert.IsType<TSelf>(stat, exactMatch: false);
        
        var clone = stat.CloneEmpty();
        
        // post-condition
        Assert.IsType<TSelf>(clone, exactMatch: false);
    }
    
    [Fact]
    public void Clone_ConcreteTypeIsSame()
    {
        var stat = GenerateStatistic();
        
        // pre-condition
        Assert.IsType<TSelf>(stat, exactMatch: false);
        
        var clone = stat.Clone();
        
        // post-condition
        Assert.IsType<TSelf>(clone, exactMatch: false);
    }
    
    [Fact]
    public void Merge_ConcreteTypeIsSame()
    {
        var stat = GenerateStatistic();
        var other = GenerateStatistic();
        
        // pre-condition
        Assert.IsType<TSelf>(stat, exactMatch: false);
        Assert.IsType<TSelf>(other, exactMatch: false);
        
        stat.Merge(other);
    }
    
    [Fact]
    public void ToString_ReturnsCorrectValue()
    {
        var stat = GenerateStatistic();
        var re = new Regex($@"^\S+{{{typeof(TObs).Name}}}\(Nobs=0\).*");
        Assert.Matches(re, stat.ToString());
    }
    
    // These tests assert the algebraic rules about counts at the Nobs level, because
    // each statistic exposes its readouts in a different way. The counts in these
    // tests are small, so even statistics whose weighted Fit processes one
    // observation at a time stay fast.
    [Fact]
    public void FitExistingState_WithZeroCount_NoOp()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation(), 3);
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        stat.Fit(GenerateObservation(), 0);
        
        // post-condition
        Assert.Equal(3, stat.Nobs);
    }
    
    [Fact]
    public void FitWeightedOne_EqualsFitSingle_Nobs()
    {
        IRunningStatistic<TObs> single = GenerateStatistic();
        IRunningStatistic<TObs> weighted = GenerateStatistic();
        
        var obs = GenerateObservation();
        single.Fit(obs);
        weighted.Fit(obs, 1);
        
        // post-condition
        Assert.Equal(1, single.Nobs);
        Assert.Equal(single.Nobs, weighted.Nobs);
    }
    
    [Fact]
    public void Merge_FreshWithPopulated_CopiesNobs()
    {
        var empty = GenerateStatistic();
        var filled = GenerateStatistic();
        
        filled.Fit(GenerateObservation(), 3);
        
        // pre-conditions
        Assert.Equal(0, empty.Nobs);
        Assert.Equal(3, filled.Nobs);
        
        empty.Merge(filled);
        
        // post-conditions
        Assert.Equal(3, empty.Nobs);
        Assert.Equal(3, filled.Nobs);
    }
    
    [Fact]
    public void Merge_PopulatedWithFresh_LeavesNobs()
    {
        var filled = GenerateStatistic();
        var empty = GenerateStatistic();
        
        filled.Fit(GenerateObservation(), 3);
        
        // pre-conditions
        Assert.Equal(3, filled.Nobs);
        Assert.Equal(0, empty.Nobs);
        
        filled.Merge(empty);
        
        // post-conditions
        Assert.Equal(3, filled.Nobs);
        Assert.Equal(0, empty.Nobs);
    }
    
    [Fact]
    public void Merge_IntoSelf_DoublesNobs()
    {
        var stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation(), 3);
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        stat.Merge(stat);
        
        // post-condition
        Assert.Equal(6, stat.Nobs);
    }
    
    [Fact]
    public void Merge_TwoPopulated_NobsAdditiveAndCommutative()
    {
        var first = GenerateStatistic();
        var second = GenerateStatistic();
        var firstCopy = GenerateStatistic();
        var secondCopy = GenerateStatistic();
        
        first.Fit(GenerateObservation(), 3);
        second.Fit(GenerateObservation(), 2);
        firstCopy.Fit(GenerateObservation(), 3);
        secondCopy.Fit(GenerateObservation(), 2);
        
        // pre-conditions
        Assert.Equal(3, first.Nobs);
        Assert.Equal(2, second.Nobs);
        Assert.Equal(3, firstCopy.Nobs);
        Assert.Equal(2, secondCopy.Nobs);
        
        first.Merge(second);
        secondCopy.Merge(firstCopy);
        
        // post-conditions
        Assert.Equal(5, first.Nobs);
        Assert.Equal(first.Nobs, secondCopy.Nobs);
    }
    
    [Fact]
    public void CloneEmpty_NobsIndependence()
    {
        var stat = GenerateStatistic();
        var clone = stat.CloneEmpty();
        
        // pre-conditions
        Assert.Equal(0, clone.Nobs);
        
        stat.Fit(GenerateObservation(), 3);
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(0, clone.Nobs);
        
        clone.Fit(GenerateObservation(), 2);
        Assert.Equal(3, stat.Nobs);
        Assert.Equal(2, clone.Nobs);
        
        stat.Merge(stat);
        clone.Merge(clone);
        
        // post-conditions: each merge into self doubles that object's own Nobs only
        Assert.Equal(6, stat.Nobs);
        Assert.Equal(4, clone.Nobs);
        
        clone.Merge(stat);
        
        // Merging adds the other statistic's counts to the receiver, and the statistic
        // being merged keeps its own state unchanged.
        Assert.Equal(6, stat.Nobs);
        Assert.Equal(10, clone.Nobs);
    }
    
    // Every running statistic shares three guard contracts. The weighted Fit rejects
    // negative counts. The base class rejects null collections. The code checks
    // every addition to the observation count for overflow. Because every statistic
    // inherits this behavior, one test here covers every statistic, instead of a
    // copy in each class's test file.
    [Fact]
    public void Fit_NegativeCountThrowsStateUntouched()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation(), 3);
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => stat.Fit(GenerateObservation(), -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => stat.Fit(GenerateObservation(), long.MinValue));
        
        // post-condition: rejected counts never touch state
        Assert.Equal(3, stat.Nobs);
    }

    [Fact]
    public void FitEmptyIterable_FreshIsNoOp()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(new List<TObs>());
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void FitEmptyIterable_FittedKeepsState()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation(), 3);
        
        // pre-condition
        Assert.Equal(3, stat.Nobs);
        
        stat.Fit(new List<TObs>());
        
        // post-condition: an empty batch leaves the existing state alone
        Assert.Equal(3, stat.Nobs);
    }
    
    // The tests check overflow of the observation count through the merge path.
    // Every Merge adds two counts with arithmetic that reports overflow, so one
    // shared fact covers every statistic. The individual class tests keep the
    // overflow check for the Fit path. Statistics such as EmpiricalCdf replay
    // observations one at a time, so they could never process a batch that large
    // in the first place.
    [Fact]
    public void MergeSelf_NobsOverflowThrows()
    {
        var stat = GenerateStatistic();
        
        stat.Fit(GenerateObservation());
        Assert.Equal(1, stat.Nobs);
        
        // Merging a statistic into itself doubles its observation count each round.
        // 62 rounds take the count from 1 to 2 to the power of 62, and one more
        // merge would overflow.
        for (var i = 0; i < 62; i++)
        {
            stat.Merge(stat);
            Assert.Equal(1L << i + 1, stat.Nobs);
        }
        
        var big = long.MaxValue / 2 + 1;
        
        Assert.Throws<OverflowException>(() => stat.Merge(stat));
        
        // post-condition: the overflowed merge leaves Nobs at its pre-merge value
        Assert.Equal(big, stat.Nobs);
    }
    
    [Fact]
    public void Reset_FreshIsNoOp()
    {
        IRunningStatistic<TObs> stat = GenerateStatistic();
        
        Assert.Equal(0, stat.Nobs);
        
        stat.Reset();
        Assert.Equal(0, stat.Nobs);
    }
    
    [Fact]
    public void ToString_NobsMatchesLiveState()
    {
        var stat = GenerateStatistic();
        
        Assert.Matches(ToStringWithNobs(0), stat.ToString());
        
        stat.Fit(GenerateObservation(), 3);
        Assert.Matches(ToStringWithNobs(3), stat.ToString());
        
        var clone = stat.Clone();
        clone.Fit(GenerateObservation(), 2);
        Assert.Matches(ToStringWithNobs(5), clone.ToString());
        Assert.DoesNotMatch(ToStringWithNobs(5), stat.ToString());
    }
    
    private static Regex ToStringWithNobs(long nobs)
    {
        return new Regex($@"^\S+{{{typeof(TObs).Name}}}\(Nobs={nobs:N0}\).*");
    }
}