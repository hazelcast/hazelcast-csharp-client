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

using System.Collections.Generic;
using System.Linq;
using Hazelcast.Clustering;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Metrics;
using Hazelcast.Networking;
using Hazelcast.Sql;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client options.
    /// </summary>
    public sealed partial class HazelcastOptions : HazelcastOptionsBase
    {
        /// <summary>
        /// Gets the Hazelcast configuration section name, which is <c>"hazelcast"</c>.
        /// </summary>
        internal const string SectionNameConstant = "hazelcast";

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        public HazelcastOptions()
        {
            Subscribers = new List<IHazelcastClientEventSubscriber>();
            SubscribersBinder = new CollectionBinder<InjectionOptions>(x
                => Subscribers.Add(new HazelcastClientEventSubscriber(x.TypeName)));

            // initializers are executed prior to executing the constructor, so Preview has a value here,
            // and we cannot use initializers for these properties these initializers cannot reference
            // the non-static Preview property + the order of execution of initializers defined in partials
            // is undefined
            Networking = new NetworkingOptions(Preview);
            Messaging = new MessagingOptions(Preview);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        private HazelcastOptions(HazelcastOptions other)
        {
            ClientName = other.ClientName;
            ClusterName = other.ClusterName;
            Subscribers = new List<IHazelcastClientEventSubscriber>(other.Subscribers);
            Labels = new HashSet<string>(other.Labels);
            LoggerFactory = other.LoggerFactory.Clone();
            PatternMatcher = other.PatternMatcher;

            ((IClusterOptions)this).ClientNamePrefix = ((IClusterOptions)other).ClientNamePrefix;

            Preview = other.Preview.Clone();
            Core = other.Core.Clone();
            Heartbeat = other.Heartbeat.Clone();
            Networking = other.Networking.Clone(Preview);
            Authentication = other.Authentication.Clone();
            LoadBalancer = other.LoadBalancer.Clone();
            Serialization = other.Serialization.Clone();
            Messaging = other.Messaging.Clone(Preview);
            Events = other.Events.Clone();
            Metrics = other.Metrics.Clone();
            Sql = other.Sql.Clone();
            NearCache = other.NearCache.Clone();
            NearCaches = other.NearCaches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone());

            FlakeIdGenerators = other.FlakeIdGenerators.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone());
        }

        /// <inheritdoc />
        internal override string SectionName => SectionNameConstant;

        /// <summary>
        /// (unsupported) Gets the <see cref="PreviewOptions"/>.
        /// </summary>
        public PreviewOptions Preview { get; } = new PreviewOptions();

        /// <summary>
        /// Gets the <see cref="CoreOptions"/>.
        /// </summary>
        /// <returns>The core options.</returns>
        [BinderIgnore(false)]
        internal CoreOptions Core { get; } = new CoreOptions();

        /// <summary>
        /// Gets the <see cref="HazelcastFailoverOptions"/>.
        /// </summary>
        /// <returns>The failover options.</returns>
        [BinderIgnore(false)]
        internal HazelcastFailoverOptions FailoverOptions { get; set; } = new HazelcastFailoverOptions();

        /// <summary>
        /// Gets the metrics options.
        /// </summary>
        public MetricsOptions Metrics { get; } = new MetricsOptions();

        /// <summary>
        /// Gets the <see cref="SqlOptions"/>.
        /// </summary>
        public SqlOptions Sql { get; } = new SqlOptions();      

        /// <summary>
        /// Clones the options.
        /// </summary>
        /// <returns>A deep clone of the options.</returns>
        internal HazelcastOptions Clone() => new (this);
    }
}
