using System;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtrema()
    : AbstractRunningStatsTest<double, RunningStatistics.Extrema>(
        () => new RunningStatistics.Extrema(),
        () => Random.Shared.NextDouble());