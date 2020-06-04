namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the heartbeat options
    /// </summary>
    public class HeartbeatOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatOptions"/> class.
        /// </summary>
        public HeartbeatOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatOptions"/> class.
        /// </summary>
        private HeartbeatOptions(HeartbeatOptions other)
        {
            PeriodMilliseconds = other.PeriodMilliseconds;
            TimeoutMilliseconds = other.TimeoutMilliseconds;
            PingTimeoutMilliseconds = other.PingTimeoutMilliseconds;
        }

        /// <summary>
        /// Gets or sets the heartbeat period.
        /// </summary>
        public int PeriodMilliseconds { get; set; } = 5_000;

        /// <summary>
        /// Gets or sets the timeout (how long to wait before declaring a connection down).
        /// </summary>
        /// <remarks>
        /// <para>The timeout should be longer than the period.</para>
        /// </remarks>
        public int TimeoutMilliseconds { get; set; } = 60_000;

        /// <summary>
        /// Gets or sets the ping timeout (how long to wait when pinging a member).
        /// </summary>
        public int PingTimeoutMilliseconds { get; set; } = 10_000;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal HeartbeatOptions Clone() => new HeartbeatOptions(this);
    }
}
