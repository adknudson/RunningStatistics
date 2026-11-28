using System.Numerics;

namespace RunningStatistics;

public static partial class CountMapExtensions
{
    extension<T>(CountMap<T> countMap) where T : INumberBase<T>
    {       
        /// <summary>
        /// Computes the mean of the observations. The values widen to double through
        /// double.CreateChecked, so the answer is a double whatever the observation type.
        /// </summary>
        public double Mean()
        {
            var m = new Mean();
            foreach (var (x, k) in countMap)
            {
                m.Fit(double.CreateChecked(x), k);
            }
            return m.Value;
        }

        /// <summary>
        /// Computes the variance of the observations. The corrected form needs at least
        /// two observations, the uncorrected form at least one. Below that limit the answer
        /// is NaN, since the result widens to a double before it is handed back.
        /// </summary>
        /// <param name="corrected">Whether the unbiased (sample) variance is returned or the
        /// biased (population) variance.</param>
        public double Variance(bool corrected = true)
        {
            if (countMap.Nobs < (corrected ? 2 : 1)) return double.NaN;

            var v = new Variance();
            foreach (var (x, k) in countMap)
            {
                v.Fit(double.CreateChecked(x), k);
            }

            return corrected ? v.RawVariance * Utils.Bessel(countMap.Nobs) : v.RawVariance;
        }

        /// <summary>
        /// Computes the corrected sample standard deviation: the square root of the corrected
        /// variance. It answers NaN below two observations, the limit the variance itself has.
        /// </summary>
        public double StandardDeviation() => double.Sqrt(countMap.Variance(corrected: true));

        /// <summary>
        /// Computes the standardized sample skewness, the lopsidedness of the distribution.
        /// The widened values go through Moments, so this needs at least three observations
        /// and answers NaN below that limit.
        /// </summary>
        public double Skewness()
        {
            var m = new Moments();
            foreach (var (x, k) in countMap)
            {
                m.Fit(double.CreateChecked(x), k);
            }
            return m.Skewness;
        }

        /// <summary>
        /// Computes the non-excess (historical) kurtosis, the heaviness of the distribution's
        /// tails. The widened values go through Moments, so this needs at least four
        /// observations and answers NaN below that limit.
        /// </summary>
        public double Kurtosis()
        {
            var m = new Moments();
            foreach (var (x, k) in countMap)
            {
                m.Fit(double.CreateChecked(x), k);
            }
            return m.Kurtosis;
        }

        /// <summary>
        /// Computes the kurtosis relative to a Gaussian distribution, three below the Kurtosis
        /// answer. A Gaussian curve sits at zero, and NaN carries through from Kurtosis.
        /// </summary>
        public double ExcessKurtosis() => countMap.Kurtosis() - 3.0;
    }

    extension<T>(CountMap<T> countMap) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        /// <summary>
        /// Computes the sum of all the observations, each one counted as many times as its
        /// count.
        /// </summary>
        public T Sum()
        {
            var s = new Sum<T>();
            foreach (var (x, k) in countMap)
            {
                s.Fit(x, k);
            }
            return s.Value;
        }
    }
}