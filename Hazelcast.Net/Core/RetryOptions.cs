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

using Hazelcast.Clustering;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a <see cref="RetryStrategy"/> configuration.
    /// </summary>
    public class RetryOptions
    {
        /// <summary>
        /// Gets or sets the back-off time in milliseconds.
        /// </summary>
        public int InitialBackoffMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum back-off time in milliseconds.
        /// </summary>
        public int MaxBackoffMilliseconds { get; set; } = 30*1000;

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        public double Multiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets the cluster connection timeout in milliseconds.
        /// </summary>
        public long ClusterConnectionTimeoutMilliseconds { get; set; } = 20*1000;

        /// <summary>
        /// Gets or sets the jitter.
        /// </summary>
        public double Jitter { get; set; } = 0;

        /// <summary>
        /// Clones the options.
        /// </summary>
        public RetryOptions Clone()
        {
            return new RetryOptions
            {
                InitialBackoffMilliseconds = InitialBackoffMilliseconds,
                MaxBackoffMilliseconds = MaxBackoffMilliseconds,
                Multiplier = Multiplier,
                ClusterConnectionTimeoutMilliseconds = ClusterConnectionTimeoutMilliseconds,
                Jitter = Jitter
            };
        }
    }
}
