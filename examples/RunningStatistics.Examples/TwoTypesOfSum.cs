namespace RunningStatistics.Examples;

public static class TwoTypesOfSum
{
    public static void Example()
    {
        var sum = new Sum(); // defaults to observations of type 'double'
        var genericSum = new Sum<int>();

        sum.Mean(); // member of the Sum class
        genericSum.Mean(); // provided via extension methods
    }
}