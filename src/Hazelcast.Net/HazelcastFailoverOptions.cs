// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Configuration.Binding;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client failover options.
    /// </summary>
    /// <remarks>
    /// <para>The client initially tries to connect to the first cluster of the <see cref="Clients"/>
    /// list, then tries each cluster in the list in order, and cycle the list at most <see cref="TryCount"/>
    /// times before failing and shutting down.</para>
    /// <para>When the client gets disconnected, it first tries to reconnect to the current cluster,
    /// then tries each cluster in the list in order, and cycle the list at most <see cref="TryCount"/>
    /// times before failing and shutting down.</para>
    /// <para>So if the <see cref="Clients"/> list is (A, B, C) and client is disconnected from B and
    /// <see cref="TryCount"/> is 2, it will try C, B, A, C, B, A in this order and then shutdown.</para>
    /// <para>The retry strategy for each cluster is configured with a 2 minutes timeout, i.e. the
    /// client will try to connect to each cluster for at most 2 minutes before failing.</para>
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
            Clients = new List<HazelcastOptions>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastFailoverOptions"/> class.
        /// </summary>
        private HazelcastFailoverOptions(HazelcastFailoverOptions other)
        {
            TryCount = other.TryCount;
            Clients = new List<HazelcastOptions>(other.Clients.Select(x => x.Clone()));
            Enabled = other.Enabled;
        }

        /// <inheritdoc />
        internal override string SectionName => SectionNameConstant;

        /// <summary>
        /// Gets status of failover
        /// </summary>
        [BinderIgnore(false)]
        internal bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the number of times that the client will try to reconnect to each
        /// cluster in the failover setup before shutting down.
        /// </summary>
        /// <remarks>Default try count is infinite.</remarks>
        public int TryCount { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets the list of cluster in the failover setup.
        /// </summary>
        // TODO: consider supporting merging external files?
        // "clients": [ { "client": "path-to-json" }, ... ]
        public IList<HazelcastOptions> Clients { get; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        /// <remarks>Dosen't clone <see cref="HazelcastOptions.FailoverOptions"/> due to cyclic dependncy</remarks>
        /// <returns>A deep clone of the options.</returns>
        internal HazelcastFailoverOptions Clone() => new HazelcastFailoverOptions(this);
    }
}
