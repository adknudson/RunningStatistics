using System;
using MathNet.Numerics.Random;

namespace RunningStatistics.Tests.Beta;

public partial class TestBeta()
    : AbstractRunningStatsTest<bool, RunningStatistics.Beta>(() => new RunningStatistics.Beta(), () => Random.Shared.NextBoolean());