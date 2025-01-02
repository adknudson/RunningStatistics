using System;

namespace RunningStatistics.Tests.Sum;

public partial class TestSum() 
    : AbstractRunningStatsTest<double, RunningStatistics.Sum>(
        () => Random.Shared.NextDouble(),
        () => new RunningStatistics.Sum());