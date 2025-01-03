using System;

namespace RunningStatistics.Tests.CountMap;

public partial class TestCountMap()
    : AbstractRunningStatsTest<int, CountMap<int>>(
        () => Random.Shared.Next(0, 100), 
        () => new CountMap<int>());