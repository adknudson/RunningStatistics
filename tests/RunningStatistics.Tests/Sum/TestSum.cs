using System;

namespace RunningStatistics.Tests.Sum;

public partial class TestSum() 
    : AbstractRunningStatsTest<double, RunningStatistics.Sum>(
        () => new RunningStatistics.Sum(),
        () => Random.Shared.NextDouble());