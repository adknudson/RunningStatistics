using System;
using Xunit;

namespace RunningStatistics.Tests.Histogram;

public partial class TestHistogram()
    : AbstractRunningStatsTest<double, RunningStatistics.Histogram>(
        () => Random.Shared.NextDouble(),
        () => new RunningStatistics.Histogram([0, 0.25, 0.5, 0.75, 1]))
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        var edges = new[] {0.0, 0.25, 0.5, 0.75, 1.0};
        
        var hist = new RunningStatistics.Histogram(edges, leftClosed: true, endsClosed: true);
        Assert.Equal(edges, hist.Edges);
        Assert.True(hist.LeftClosed);
        Assert.True(hist.EndsClosed);
        
        hist = new RunningStatistics.Histogram(edges, leftClosed: false, endsClosed: false);
        Assert.Equal(edges, hist.Edges);
        Assert.False(hist.LeftClosed);
        Assert.False(hist.EndsClosed);
        
        hist = new RunningStatistics.Histogram(edges, leftClosed: true, endsClosed: false);
        Assert.Equal(edges, hist.Edges);
        Assert.True(hist.LeftClosed);
        Assert.False(hist.EndsClosed);
        
        hist = new RunningStatistics.Histogram(edges, leftClosed: false, endsClosed: true);
        Assert.Equal(edges, hist.Edges);
        Assert.False(hist.LeftClosed);
        Assert.True(hist.EndsClosed);
    } 
}