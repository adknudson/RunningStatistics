namespace RunningStatistics.Tests.Normal;

public partial class TestNormal()
    : AbstractRunningStatsTest<double, RunningStatistics.Normal>(
        () => MathNet.Numerics.Distributions.Normal.Sample(0, 1), 
        () => new RunningStatistics.Normal());