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
        /// <remarks>
        /// <para>Heartbeat will run periodically, and send a ping request to connections
        /// that have not been written to over the previous period.</para>
        /// </remarks>
        public int PeriodMilliseconds { get; set; } = 5_000;

        /// <summary>
        /// Gets or sets the timeout (how long to wait before declaring a connection down).
        /// </summary>
        /// <remarks>
        /// <para>Heartbeat will consider that connections that have not received data for
        /// the timeout duration, although they should have been pinged, are down.</para>
        /// <para>The timeout should be longer than the period.</para>
        /// </remarks>
        public int TimeoutMilliseconds { get; set; } = 60_000;

        /// <summary>
        /// Gets or sets the ping timeout (how long to wait when pinging a member).
        /// </summary>
        /// <remarks>
        /// <para>The timeout should be shorter that the period.</para>
        /// </remarks>
        public int PingTimeoutMilliseconds { get; set; } = 2_000;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal HeartbeatOptions Clone() => new HeartbeatOptions(this);
    }
}
