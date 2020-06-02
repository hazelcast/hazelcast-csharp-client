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
using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Core;

namespace Hazelcast
{
    /// <summary>
    /// Main options to setup a Hazelcast Client.
    /// </summary>
    public sealed partial class HazelcastOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        public HazelcastOptions()
        {
            Subscribers = new List<IClusterEventSubscriber>();
            SubscribersBinder = new CollectionBinder<string>(x
                => Subscribers.Add(new ClusterEventSubscriber(x)));
        }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        internal IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal HazelcastOptions Clone()
        {
            return new HazelcastOptions
            {
                ClientName = ClientName,

                ClusterName = ClusterName,
                Subscribers = new List<IClusterEventSubscriber>(Subscribers),

                Properties = new Dictionary<string, string>(Properties),
                Labels = new HashSet<string>(Labels),
                AsyncStart = AsyncStart,

                Logging = Logging.Clone(),
                Network = Network.Clone(),
                Security = Security.Clone(),
                Authentication = Authentication.Clone(),
                LoadBalancer = LoadBalancer.Clone(),
                Serialization = Serialization.Clone(),
                NearCache = NearCache.Clone()
            };
        }
    }
}
