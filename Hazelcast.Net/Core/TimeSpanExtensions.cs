using System;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="TimeSpan"/> struct.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Gets the value of the <see cref="TimeSpan"/> expressed in whole milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="infinite">The value of "infinite".</param>
        /// <returns>The value of the specified time span expressed in whole milliseconds.</returns>
        /// <remarks>
        /// <para>For timeouts, codecs expect infinite to be zero, but for lease times, or other
        /// durations, they expect infinite to be -1s or sometimes <see cref="long.MaxValue"/>.
        /// This method converts <see cref="TimeSpan.Zero"/> to zero, and
        /// <see cref="Timeout.InfiniteTimeSpan"/> to <paramref name="infinite"/>.</para>
        /// <para>This allows our public API to use <see cref="Timeout.InfiniteTimeSpan"/> (which
        /// has an actual value of -1ms) for everything infinite without bothering about
        /// other conventions.</para>
        /// </remarks>
        public static long CodecMilliseconds(this TimeSpan timeSpan, long infinite)
            => timeSpan == Timeout.InfiniteTimeSpan ? infinite : (long) timeSpan.TotalMilliseconds;

        /// <summary>
        /// Gets the value of the <see cref="TimeSpan"/> expressed in whole timeout milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="defaultTimeout">The default timeout if the time span is zero.</param>
        /// <param name="infiniteTimeout">The milliseconds value of the infinite timeout.</param>
        /// <returns>The value of the specified time span expressed in whole timeout milliseconds.</returns>
        /// <remarks>
        /// <para>The time span is expected to be negative (e.g. <see cref="Timeout.InfiniteTimeSpan"/> which
        /// is -1ms) for infinite, zero (e.g. <see cref="TimeSpan.Zero"/>) for default, or positive for
        /// an actual value.</para>
        /// </remarks>
        public static int TimeoutMilliseconds(this TimeSpan timeSpan, int defaultTimeout, int infiniteTimeout = -1)
        {
            var timeout = (int) timeSpan.TotalMilliseconds;
            return timeout > 0 ? timeout : timeout < 0 ? infiniteTimeout : defaultTimeout;
        }
    }
}
