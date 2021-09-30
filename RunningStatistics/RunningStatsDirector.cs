namespace RunningStatistics
{
    public class RunningStatsDirector
    {
        private IRunningStatsBuilder _builder;
        public IRunningStatsBuilder Builder { set { _builder = value; } }


        public void BuildMeanVariance()
        {
            this._builder.BuildMean();
            this._builder.BuildVariance();
        }


        public void BuildAllSimpleStats()
        {
            this._builder.BuildMean();
            this._builder.BuildVariance();
            this._builder.BuildSum();
            this._builder.BuildMoments();
            this._builder.BuildExtrema();
        }
    }
}
