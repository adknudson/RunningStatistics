using System;

namespace RunningStatistics.Tests.Moments;

public partial class TestMoments() 
    : AbstractRunningStatsTest<double, RunningStatistics.Moments>(
        () => Random.Shared.NextDouble(), 
        () => new RunningStatistics.Moments());