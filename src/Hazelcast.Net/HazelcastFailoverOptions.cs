// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Linq;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client failover options.
    /// </summary>
    /// <remarks>
    /// <para>The client initially tries to connect to the first cluster of the <see cref="Clusters"/>
    /// list. When it fails, or if the client eventually gets disconnected, it tries each cluster in
    /// the list in order, and cycle the list at most <see cref="TryCount"/> times before failing and
    /// shutting down.</para>
    /// </remarks>
    public sealed class HazelcastFailoverOptions : HazelcastOptionsBase
    {
        /// <summary>
        /// Gets the Hazelcast failover configuration section name, which is <c>"hazelcast-failover"</c>.
        /// </summary>
        internal const string SectionNameConstant = "hazelcast-failover";

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastFailoverOptions"/> class.
        /// </summary>
        public HazelcastFailoverOptions()
        {
            Clusters = new List<HazelcastOptions>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastFailoverOptions"/> class.
        /// </summary>
        private HazelcastFailoverOptions(HazelcastFailoverOptions other)
        {
            TryCount = other.TryCount;
            Clusters = new List<HazelcastOptions>(other.Clusters.Select(x => x.Clone()));
        }

        /// <inheritdoc />
        internal override string SectionName => SectionNameConstant;

        /// <summary>
        /// Gets or sets the number of times that the client will try to reconnect to each
        /// cluster in the failover setup before shutting down.
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// Gets the list of cluster in the failover setup.
        /// </summary>
        // TODO: consider supporting merging external files?
        // "clients": [ { "client": "path-to-json" }, ... ]
        public IList<HazelcastOptions> Clusters { get; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        /// <returns>A deep clone of the options.</returns>
        internal HazelcastFailoverOptions Clone() => new HazelcastFailoverOptions(this);
    }
}
