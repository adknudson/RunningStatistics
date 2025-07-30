using System;
using MathNet.Numerics.Random;

namespace RunningStatistics.Tests.Extrema;

public partial class TestExtremaGenericComparableType()
    : AbstractRunningStatsTest<MyComparable, Extrema<MyComparable>>(
        () => new MyComparable(Random.Shared.NextDecimal()),
        () => new Extrema<MyComparable>());