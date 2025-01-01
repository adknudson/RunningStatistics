using System;

namespace RunningStatistics.Tests.EmpiricalCdf;

public partial class TestEmpiricalCdf()
    : AbstractRunningStatsTest<double, RunningStatistics.EmpiricalCdf>(() => new RunningStatistics.EmpiricalCdf(), () => Random.Shared.NextDouble());