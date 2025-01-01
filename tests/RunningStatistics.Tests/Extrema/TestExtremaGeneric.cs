using System;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtremaGeneric()
    : AbstractRunningStatsTest<double, Extrema<double>>(
        () => new Extrema<double>(),
        () => Random.Shared.NextDouble());