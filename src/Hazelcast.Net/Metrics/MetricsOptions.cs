// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Metrics
{
    /// <summary>
    /// Represents the client metrics options.
    /// </summary>
    public class MetricsOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsOptions"/> class.
        /// </summary>
        public MetricsOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsOptions"/> class.
        /// </summary>
        private MetricsOptions(MetricsOptions other)
        {
            Enabled = other.Enabled;
            PeriodSeconds = other.PeriodSeconds;
        }

        /// <summary>
        /// Whether client statistics are enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the period at which client metrics  are sent to the cluster.
        /// </summary>
        public int PeriodSeconds { get; set; } = 5;

        /// <summary>
        /// Clone the options.
        /// </summary>
        /// <returns>A deep clone of the options.</returns>
        public MetricsOptions Clone() => new MetricsOptions(this);
    }
}
