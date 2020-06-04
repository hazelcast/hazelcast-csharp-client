namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents messaging options.
    /// </summary>
    public class MessagingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingOptions"/> class.
        /// </summary>
        public MessagingOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingOptions"/> class.
        /// </summary>
        public MessagingOptions(MessagingOptions other)
        {
            MaxFastInvocationCount = other.MaxFastInvocationCount;
            MinRetryDelayMilliseconds = other.MinRetryDelayMilliseconds;
            DefaultTimeoutMilliseconds = other.DefaultTimeoutMilliseconds;
            DefaultOperationTimeoutMilliseconds = other.DefaultOperationTimeoutMilliseconds;
        }

        /// <summary>
        /// Gets or sets the max fast invocation count.
        /// </summary>
        public int MaxFastInvocationCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the min retry delay.
        /// </summary>
        public int MinRetryDelayMilliseconds { get; set; } = 1_000;

        /// <summary>
        /// Gets or sets the default timout.
        /// </summary>
        public int DefaultTimeoutMilliseconds { get; set; } = 120_000;

        /// <summary>
        /// Gets or sets the default operation timeout.
        /// </summary>
        public int DefaultOperationTimeoutMilliseconds { get; set; } = 60_000;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal MessagingOptions Clone() => new MessagingOptions(this);
    }
}
