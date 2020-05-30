// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Defines constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Defines constants for heartbeat.
        /// </summary>
        public static class Heartbeat
        {
            private static readonly Lazy<int> IntervalMillisecondsLazy
                 = new Lazy<int>(() => HazelcastEnvironment.Heartbeat.IntervalMilliseconds ?? 5_000);

            private static readonly Lazy<int> TimeoutMillisecondsLazy
                = new Lazy<int>(() => HazelcastEnvironment.Heartbeat.TimeoutMilliseconds ?? 60_000);

            /// <summary>
            /// Gets the interval.
            /// </summary>
            public static int IntervalMilliseconds => IntervalMillisecondsLazy.Value;

            /// <summary>
            /// Gets the timeout.
            /// </summary>
            public static int TimeoutMilliseconds => TimeoutMillisecondsLazy.Value;

            /// <summary>
            /// Gets the ping timeout.
            /// </summary>
            public static int PingTimeoutMilliseconds => 10_000;
        }

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

        /// <summary>
        /// Defines constants for distributed objects.
        /// </summary>
        public static class DistributedObjects
        {
            /// <summary>
            /// Default timeout for operations.
            /// </summary>
            public const int DefaultOperationTimeoutMilliseconds = 60_000;
        }
    }
}
