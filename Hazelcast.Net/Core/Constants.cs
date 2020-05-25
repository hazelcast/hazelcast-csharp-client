using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Defines constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Defines constants for invocations.
        /// </summary>
        public static class Invocation
        {
            private static readonly Lazy<int> MinRetryDelayMillisecondsLazy 
                = new Lazy<int>(() => HazelcastEnvironment.Invocation.MinRetryDelayMilliseconds ?? 1_000);

            private static readonly Lazy<int> DefaultTimeoutSecondsLazy
                = new Lazy<int>(() => HazelcastEnvironment.Invocation.DefaultTimeoutSeconds ?? 120);

            /// <summary>
            /// How many times a invocation should be retried without delay.
            /// </summary>
            public const int MaxFastInvocationCount = 5;

            /// <summary>
            /// Minimum delay before retrying an invocation.
            /// </summary>
            public static int MinRetryDelayMilliseconds => MinRetryDelayMillisecondsLazy.Value;

            /// <summary>
            /// Default timeout for an invocation.
            /// </summary>
            public static int DefaultTimeoutSeconds = DefaultTimeoutSecondsLazy.Value;
        }

        /// <summary>
        /// Defines constants for clusters.
        /// </summary>
        public static class Cluster
        {
            /// <summary>
            /// Maximum number of attempts per client, to obtain a cluster client.
            /// </summary>
            public const int MaxAttemptsPerClientForClusterClient = 3;

            /// <summary>
            /// Default timeout for connecting to a cluster.
            /// </summary>
            public const int DefaultConnectTimeoutMilliseconds = 60_000;

            /// <summary>
            /// Time to wait for a client to appear, when there is no client.
            /// </summary>
            public const int WaitForClientMilliseconds = 1_000;
        }
    }
}
