using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the system clock.
    /// </summary>
    public static class Clock
    {
        // unix epoch is 00:00:00 UTC on January 1st, 1970
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly long Offset;

        /// <summary>
        /// Initializes the <see cref="Clock"/> class.
        /// </summary>
        static Clock()
        {
            Offset = HazelcastEnvironment.Clock.Offset ?? 0;
        }

        /// <summary>
        /// Gets the epoch time in milliseconds, i.e. the number of milliseconds that have elapsed since the epoch (00:00:00 UTC on January 1st, 1970).
        /// </summary>
        /// <remarks>The epoch time in milliseconds.</remarks>
        public static long Milliseconds
            => ToEpoch(DateTime.UtcNow);

        /// <summary>
        /// Gets a number (-1) representing 'never'.
        /// </summary>
        public static long Never
            => -1L;

        /// <summary>
        /// Gets the UTC <see cref="DateTime"/> corresponding to an epoch time.
        /// </summary>
        /// <param name="milliseconds">The epoch time in milliseconds.</param>
        /// <returns>The corresponding UTC <see cref="DateTime"/>.</returns>
        public static DateTime ToDateTime(long milliseconds)
            => Jan1St1970.AddMilliseconds(milliseconds - Offset);

        /// <summary>
        /// Gets the epoch time in milliseconds corresponding to an UTC <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/>.</param>
        /// <returns>The epoch time in milliseconds corresponding to the <see cref="DateTime"/>.</returns>
        public static long ToEpoch(DateTime dateTime)
            => (long) (dateTime - Jan1St1970).TotalMilliseconds + Offset;
    }
}
