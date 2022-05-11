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
    // FIXME discussion
    //
    // TODO: which one is best / easier to understand here?
    // var failoverOptions = new HazelcastOptionsBuilder().WithFailover(...).BuildFailover();
    // var failoverOptions = new HazelcastOptionsBuilder().Failover.With(...).Build();
    // var failoverOptions = new HazelcastFailoverOptionsBuilder().With(...).Build();
    //
    // var client = await HazelcastClientFactory.StartNewClientAsync(failoverOptions);
    //
    // TODO: overload resolution?
    // var client = await HazelcastClientFactory.StartNewClientAsync(options => {
    //   options.ClusterName = "bob";
    // });
    // var client = await HazelcastClientFactory.StartNewClientAsync((HazelcastOptions options) => { ... });
    // var client = await HazelcastFailoverClientFactory.StartNewClientAsync(options => { ... });
    // var client = await HazelcastFailoverClientFactory.StartNewClientAsync(options => { ... });
    // var client = await HazelcastClientFactory.StartNewFailoverClientAsync(options => { ... });
    //
    // var failoverOptions = new HazelcastFailoverOptionsBuilder().With(...).Build();
    // var client = await HazelcastFailoverClientFactory.StartNewClientAsync(failoverOptions);

    /// <summary>
    /// Represents the Hazelcast client failover options.
    /// </summary>
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
        }

        /// <inheritdoc />
        internal override string SectionName => SectionNameConstant;

        /// <summary>
        /// FIXME document
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// FIXME document
        /// </summary>
        // TODO: consider supporting merging external files?
        // "clients": [ { "client": "path-to-json" }, ... ]
        public IList<HazelcastOptions> Clients { get; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        /// <returns>A deep clone of the options.</returns>
        internal HazelcastFailoverOptions Clone() => new HazelcastFailoverOptions(this);
    }
}
