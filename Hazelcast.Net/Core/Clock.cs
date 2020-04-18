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
        public static long Milliseconds
            => (long) (DateTime.UtcNow - Jan1St1970).TotalMilliseconds + Offset;
    }
}
