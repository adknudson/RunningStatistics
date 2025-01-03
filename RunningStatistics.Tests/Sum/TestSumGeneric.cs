using System;

namespace RunningStatistics.Tests.Sum;

public partial class TestSumGeneric() 
    : AbstractRunningStatsTest<double, Sum<double>>(
        () => Random.Shared.NextDouble(), 
        () => new Sum<double>());