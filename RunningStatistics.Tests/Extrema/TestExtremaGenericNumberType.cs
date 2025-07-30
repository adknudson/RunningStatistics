using System;
using MathNet.Numerics.Random;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtremaGenericNumberType()
    : AbstractRunningStatsTest<MyNum, Extrema<MyNum>>(
        () => new MyNum(Random.Shared.NextDecimal()),
        () => new Extrema<MyNum>());