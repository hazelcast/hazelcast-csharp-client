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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.DistributedObjects.Implementation;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    internal partial class HazelcastClient : IHazelcastClient
    {
        private readonly HazelcastConfiguration _configuration;
        private readonly DistributedObjectFactory _distributedObjectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        /// <param name="configuration">The client configuration.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HazelcastClient(HazelcastConfiguration configuration, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));

            _distributedObjectFactory = new DistributedObjectFactory(Cluster, serializationService, loggerFactory);
            Cluster.OnConnectingToNewCluster = () => _distributedObjectFactory.CreateAllAsync();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        public HazelcastClient(Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : this(HazelcastConfiguration.CreateDefault(), cluster, serializationService, loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        public HazelcastClient(string configurationFilepath, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : this(HazelcastConfiguration.Parse(configurationFilepath), cluster, serializationService, loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        public HazelcastClient(Action<HazelcastConfiguration> configure, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : this(HazelcastClientFactory.BuildConfiguration(configure), cluster, serializationService, loggerFactory)
        { }

        /// <summary>
        /// Gets the <see cref="Cluster"/>.
        /// </summary>
        public Cluster Cluster { get; }

        /// <inheritdoc />
        public async Task OpenAsync()
        {
            await Cluster.ConnectAsync();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        /// <summary>
        /// Closes the client.
        /// </summary>
        /// <returns>A task that will complete when the client has closed.</returns>
        public Task CloseAsync()
        {
            // TODO: implement HazelcastClient.CloseAsync()
            // TODO: consider making HazelcastClient IDisposable
            throw new NotImplementedException();
        }
    }
}