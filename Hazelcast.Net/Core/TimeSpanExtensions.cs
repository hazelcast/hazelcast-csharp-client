using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="TimeSpan"/> struct.
    /// </summary>
    public static class TimeSpanExtensions
    {
        // NOTES
        //
        // Timeout.InfiniteTimeSpan is "-1ms"

        // TODO cleanup maybe we don't need all of them

        /// <summary>
        /// Gets the value of the <see cref="TimeSpan"/> expressed in whole milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="infinite">The value of "infinite".</param>
        /// <returns>The value of the specified time span expressed in whole milliseconds.</returns>
        /// <remarks>
        /// <para>For timeouts, codecs expect infinite to be zero, but for lease times, they
        /// expect infinite to be MaxValue. This method converts <see cref="TimeSpan.Zero"/>
        /// to zero, and <see cref="Timeout.InfiniteTimeSpan"/> to <paramref name="infinite"/>.</para>
        /// </remarks>
        public static int CodecMilliseconds(this TimeSpan timeSpan, int infinite)
            => timeSpan == Timeout.InfiniteTimeSpan ? infinite : (int) timeSpan.TotalMilliseconds;

        /// <summary>
        /// Gets the value of the <see cref="TimeSpan"/> expressed in whole milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="infinite">The value of "infinite".</param>
        /// <returns>The value of the specified time span expressed in whole milliseconds.</returns>
        /// <remarks>
        /// <para>For timeouts, codecs expect infinite to be zero, but for lease times, they
        /// expect infinite to be MaxValue. This method converts <see cref="TimeSpan.Zero"/>
        /// to zero, and <see cref="Timeout.InfiniteTimeSpan"/> to <paramref name="infinite"/>.</para>
        /// </remarks>
        public static long CodecMilliseconds(this TimeSpan timeSpan, long infinite)
            => timeSpan == Timeout.InfiniteTimeSpan ? infinite : (long) timeSpan.TotalMilliseconds;

        /// <summary>
        /// Gets the value of the timeout <see cref="TimeSpan"/> expressed in whole milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>The value of the specified time span considered as a timeout and expressed in whole milliseconds.</returns>
        /// <remarks>
        /// <para>For timeouts, codecs expect infinite to be zero. This method converts both <see cref="TimeSpan.Zero"/>
        /// and <see cref="Timeout.InfiniteTimeSpan"/> to zero.</para>
        /// </remarks>
        public static long CodecTimeoutMilliseconds(this TimeSpan timeSpan)
            => timeSpan.CodecMilliseconds(0L);

        /// <summary>
        /// Gets the value of the duration <see cref="TimeSpan"/> expressed in whole milliseconds.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>The value of the specified time span considered as a duration and expressed in whole milliseconds.</returns>
        /// <remarks>
        /// <para>For durations, codecs expect infinite to be <see cref="long.MaxValue"/>. This method converts
        /// <see cref="TimeSpan.Zero"/> to zero, and <see cref="Timeout.InfiniteTimeSpan"/> to <see cref="long.MaxValue"/>.</para>
        /// </remarks>
        public static long CodecDurationMilliseconds(this TimeSpan timeSpan)
            => timeSpan.CodecMilliseconds(long.MaxValue);
    }
}
