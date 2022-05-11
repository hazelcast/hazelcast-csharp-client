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
    /// FIXME: specs & docs are not consistent!
    /// https://docs.hazelcast.com/hazelcast/latest/getting-started/blue-green says:
    /// <para>Through this configuration, the client is directed to connect to the clusters
    /// indicated by <see cref="Clusters"/> in the given order of the list. The client
    /// initially connects to the first cluster of the list. Should this cluster fail, the
    /// client would attempt to reconnect to it <see cref="TryCount"/> times, then to each
    /// subsequent cluster in the list <see cref="TryCount"/> times, until it succeeds to
    /// connect to a cluster, or fails and shuts down.</para>
    /// https://docs.hazelcast.com/hazelcast/latest/clients/java#ordering-of-clusters-when-clients-try-to-connect says:
    /// ... the client will try each cluster once, in order, and repeat the entire list
    /// <see cref="TryCount"/> times ... this is different ... and also the list does
    /// not appear to be circular, so if the client is running on the last cluster of the
    /// list it immediately increments and tests <see cref="TryCount"/>?
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
