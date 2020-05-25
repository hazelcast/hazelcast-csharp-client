using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="CancellationTokenSource"/> class.
    /// </summary>
    internal static class CancellationTokenSourceExtensions
    {
        /// <summary>
        /// Creates a cancellation source by combining a source and a timeout.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="timeoutMilliseconds">The timeout, in milliseconds.</param>
        /// <returns>The combined cancellation.</returns>
        /// <remarks>
        /// <para>The combined cancellation should be disposed after usage.</para>
        /// </remarks>
        public static TimeoutCancellationTokenSource WithTimeout(this CancellationTokenSource source, int timeoutMilliseconds)
        {
            return new TimeoutCancellationTokenSource(source, timeoutMilliseconds);
        }
    }
}
