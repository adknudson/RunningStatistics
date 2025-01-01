using System;

namespace RunningStatistics.Tests.Mean;

public partial class TestMean() 
    : AbstractRunningStatsTest<double, RunningStatistics.Mean>(
        () => new RunningStatistics.Mean(), 
        () => Random.Shared.NextDouble())
{
}