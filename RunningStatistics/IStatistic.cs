using System.Collections.Generic;

namespace RunningStatistics
{
    /// <summary>
    /// The <see cref="IStatistic{T}" /> interface for online statistics algorithms.
    /// </summary>
    /// <typeparam name="T">The observation type that the statistic can fit.</typeparam>
    public interface IStatistic<T>
    {
        /// <summary>
        /// The number of observations that have been fit.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Fit multiple observations.
        /// </summary>
        /// <param name="ys">The collection of observations.</param>
        public void Fit(IEnumerable<T> ys);

        /// <summary>
        /// Fit a single observation.
        /// </summary>
        /// <param name="y">A single observation of type <see cref="T"/>.</param>
        public void Fit(T y);

        /// <summary>
        /// Print the fitted statistic to a stream.
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.StreamWriter"/>.</param>
        public void Print(System.IO.StreamWriter stream);

        /// <summary>
        /// Reset the running statistic to zero observations.
        /// </summary>
        public void Reset();
    }
}
