using System;

namespace RunningStatistics.Tests.Mean;

public partial class TestMean() 
    : AbstractRunningStatsTest<double, RunningStatistics.Mean>(
        () => Random.Shared.NextDouble(), 
        () => new RunningStatistics.Mean());