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

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the Hazelcast Cloud options.
    /// </summary>
    public class CloudOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudOptions"/> class.
        /// </summary>
        public CloudOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudOptions"/> class.
        /// </summary>
        private CloudOptions(CloudOptions other)
        {
            DiscoveryToken = other.DiscoveryToken;
            UrlBase = other.UrlBase;
        }

        /// <summary>
        /// Whether Hazelcast Cloud is enabled.
        /// </summary>
        internal bool Enabled => !string.IsNullOrWhiteSpace(DiscoveryToken);

        /// <summary>
        /// Gets or sets the discovery token of the cluster.
        /// </summary>
        public string DiscoveryToken { get; set; }

        /// <summary>
        /// Gets or sets the cloud url base.
        /// </summary>
        internal Uri UrlBase { get; set; } = new Uri("https://coordinator.hazelcast.cloud");

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal CloudOptions Clone() => new CloudOptions(this);
    }
}
