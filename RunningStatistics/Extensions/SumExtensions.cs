using System;
using System.Numerics;

namespace RunningStatistics;

public static class SumExtensions
{
    extension<T>(Sum<T> sum) where T : INumberBase<T>
    {
        /// <summary>
        /// Calculate the average of the sum of any observation type that implements <see cref="INumberBase{T}"/>.
        /// The total widens to a <see cref="double"/> before the divide, so the answer keeps the fraction.
        /// An empty sum hands back <see cref="double.NaN" />.
        /// </summary>
        public double Mean => sum.Nobs > 0 ? double.CreateChecked(sum.Value) / sum.Nobs : double.NaN;
    }
    
    extension(Sum<float> sum)
    {
        /// <summary>
        /// Calculate the average of the sum of <see cref="float"/>s, kept in <see cref="float"/>.
        /// An empty sum hands back <see cref="float.NaN" />.
        /// </summary>
        public float Mean => sum.Nobs > 0 ? sum.Value / sum.Nobs : float.NaN;
    }
    
    extension(Sum<decimal> sum)
    {
        /// <summary>
        /// Calculate the average of the sum of <see cref="decimal"/>s, kept in <see cref="decimal"/>.
        /// </summary>
        /// <exception cref="DivideByZeroException">
        /// Thrown when <c>Nobs</c> is zero.
        /// </exception>
        public decimal Mean
        {
            get
            {
                if (sum.Nobs > 0) return sum.Value / sum.Nobs;
                // decimal doesn't have NaN values, so this is its proxy. Leave as DivideByZeroException.
                throw new DivideByZeroException("Average is undefined when the number of observations is zero.");
            }
        }
    }
}