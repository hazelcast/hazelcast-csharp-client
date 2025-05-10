﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the configuration for the retry strategy.
    /// </summary>
    public class ConnectionRetryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRetryOptions"/> class.
        /// </summary>
        public ConnectionRetryOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRetryOptions"/> class.
        /// </summary>
        private ConnectionRetryOptions(ConnectionRetryOptions other)
        {
            InitialBackoffMilliseconds = other.InitialBackoffMilliseconds;
            MaxBackoffMilliseconds = other.MaxBackoffMilliseconds;
            Multiplier = other.Multiplier;
            ClusterConnectionTimeoutMilliseconds = other.ClusterConnectionTimeoutMilliseconds;
            Jitter = other.Jitter;
        }

        /// <summary>
        /// Gets or sets the initial back-off time in milliseconds.
        /// </summary>
        public int InitialBackoffMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum back-off time in milliseconds.
        /// </summary>
        public int MaxBackoffMilliseconds { get; set; } = 30*1000;

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        public double Multiplier { get; set; } = 1.05;

        /// <summary>
        /// Gets or sets the timeout in milliseconds.
        /// </summary>
        /// <remarks>
        /// <p>Use <code>-1</code> to indicate an infinite timeout.</p>
        /// <p>This value must be smaller than <c>TimeSpan.MaxValue.TotalMilliseconds</c>.</p>
        /// </remarks>
        public long ClusterConnectionTimeoutMilliseconds { get; set; } = -1; // infinite

        /// <summary>
        /// Gets or sets the jitter.
        /// </summary>
        public double Jitter { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal ConnectionRetryOptions Clone() => new ConnectionRetryOptions(this);
    }
}
