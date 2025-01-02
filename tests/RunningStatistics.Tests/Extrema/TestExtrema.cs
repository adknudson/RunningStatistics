using System;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtrema()
    : AbstractRunningStatsTest<double, RunningStatistics.Extrema>(
        () => Random.Shared.NextDouble(),
        () => new RunningStatistics.Extrema());