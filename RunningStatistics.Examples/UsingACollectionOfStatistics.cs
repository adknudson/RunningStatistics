namespace RunningStatistics.Examples;

public static class Program
{
    public static void Main()
    {
        var stats = new StatsCollection<double>
        {
            new Mean(),
            new Moments(),
            new CountMap<double>(),
            new Extrema()
        };

        var rng = new Random();
        
        for (var i = 0; i < 100; i++)
        {
            var x = rng.NextDouble();
            stats.Fit(x);
        }

        foreach (var stat in stats)
        {
            Console.WriteLine(stat);
        }
    }
}