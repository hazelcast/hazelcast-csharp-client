using System;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Utilities for managing time-to-live.
    /// </summary>
    public static class TimeToLive
    {
        /// <summary>
        /// A constants used to specify an infinite time-to-live.
        /// </summary>
        public static readonly TimeSpan InfiniteTimeSpan = Timeout.InfiniteTimeSpan;
    }
}
