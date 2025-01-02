using System;

namespace RunningStatistics.Tests.Histogram;

public partial class TestHistogram()
    : AbstractRunningStatsTest<double, RunningStatistics.Histogram>(
        () => Random.Shared.NextDouble(), 
        () => new RunningStatistics.Histogram([0, 0.25, 0.5, 0.75, 1]));