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
using Hazelcast.Configuration;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    internal class HazelcastClient : IHazelcastClient
    {
        private readonly ClientConfig _configuration;
        private readonly DistributedObjectFactory _distributedObjectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        /// <param name="configuration">The client configuration.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HazelcastClient(ClientConfig configuration, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
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
            : this(XmlClientConfigBuilder.Build(), cluster, serializationService, loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        public HazelcastClient(string configurationFilepath, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : this(XmlClientConfigBuilder.Build(configurationFilepath), cluster, serializationService, loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        public HazelcastClient(Action<ClientConfig> configure, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : this(BuildConfiguration(configure), cluster, serializationService, loggerFactory)
        { }

        /// <summary>
        /// Builds the configuration.
        /// </summary>
        /// <param name="configure">A configuration builder.</param>
        /// <returns>The configuration.</returns>
        private static ClientConfig BuildConfiguration(Action<ClientConfig> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var configuration = new ClientConfig();
            configure(configuration);
            return configuration;
        }

        /// <summary>
        /// Gets the <see cref="Cluster"/>.
        /// </summary>
        public Cluster Cluster { get; }

        /// <summary>
        /// Opens the client.
        /// </summary>
        /// <returns>A task that will complete when the client is open and ready.</returns>
        public async Task OpenAsync()
        {
            await Cluster.ConnectAsync();
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

        // TODO: implement HazelcastClient access to other Distributed Objects

        /// <summary>
        /// Gets an <see cref="IMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>A task that will complete when the map has been retrieved or created,
        /// and represents the map that has been retrieved or created.</returns>
#if DEBUG // maintain full stack traces
        public async Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => await GetDistributedObjectAsync<IMap<TKey,TValue>>(Constants.ServiceNames.Map, name);
#else
        public Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => GetDistributedObjectAsync<IMap<TKey,TValue>>(Constants.ServiceNames.Map, name);
#endif

        /// <summary>
        /// Gets a distributed object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <returns>A task that will complete when the object has been retrieved or created,
        /// and represents the object that has been retrieved or created.</returns>
#if DEBUG // maintain full stack traces
        private async ValueTask<TObject> GetDistributedObjectAsync<TObject>(string serviceName, string name)
            where TObject : IDistributedObject
            => await _distributedObjectFactory.GetOrCreateAsync<TObject>(serviceName, name);
#else
        private ValueTask<T> GetDistributedObjectAsync<T>(string serviceName, string name)
            where T : IDistributedObject
            => _distributedObjectFactory.GetOrCreateAsync<T>(serviceName, name);
#endif
    }
}