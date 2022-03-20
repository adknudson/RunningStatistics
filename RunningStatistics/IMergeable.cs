namespace RunningStatistics;

public interface IMergeable<in T>
{
    public void Merge(T other);
}