using System;

namespace RunningStatistics.Tests.EmpiricalCdf;

public partial class TestEmpiricalCdf()
    : AbstractRunningStatsTest<double, RunningStatistics.EmpiricalCdf>(
        () => Random.Shared.NextDouble(), 
        () => new RunningStatistics.EmpiricalCdf());