using System;

namespace RunningStatistics.Tests.Sum;

public partial class TestSumGeneric() 
    : AbstractRunningStatsTest<double, Sum<double>>(
        () => new Sum<double>(),
        () => Random.Shared.NextDouble());