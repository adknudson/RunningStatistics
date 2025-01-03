namespace RunningStatistics.Examples;

public static class TwoTypesOfSum
{
    public static void Example()
    {
        var sum = new Sum(); // stores observations of type 'double'
        var dSum = new Sum<double>(); // explicitly stores observations of type 'double'. Requires .NET 7 or greater
    }
}