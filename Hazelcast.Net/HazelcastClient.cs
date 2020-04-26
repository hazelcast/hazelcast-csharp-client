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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Portable;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    internal class HazelcastClient
    {
        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;
        private readonly DistributedObjectFactory _distributedObjectFactory;

        /// <summary>
        /// Initializes the <see cref="HazelcastClient"/> class.
        /// </summary>
        static HazelcastClient()
        {
            // FIXME: this is not the right place to to this
            InitializeServices();
        }

        public static void InitializeServices()
        {
            // plug the serializer hooks.
            var serializerHooks = Services.TryGetInstance<SerializerHooks>();
            if (serializerHooks == null) Services.Register(serializerHooks = new SerializerHooks());

            serializerHooks.Add<PredicateDataSerializerHook>();
            serializerHooks.Add<AggregatorDataSerializerHook>();
            serializerHooks.Add<ProjectionDataSerializerHook>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        public HazelcastClient()
        {
            _cluster = new Cluster();
            _serializationService = new SerializationService(
                new ByteArrayInputOutputFactory(Endianness.Native),
                1,
                new Dictionary<int, IDataSerializableFactory>(),
                new Dictionary<int, IPortableFactory>(),
                new List<IClassDefinition>(),
                false,
                new NullPartitioningStrategy(),
                512);
            _distributedObjectFactory = new DistributedObjectFactory(_cluster, _serializationService);
        }

        public async Task OpenAsync()
        {
            await _cluster.Connect(); // FIXME ConnectAsync!!
        }

        // FIXME Task vs ValueTask
        // use Task publicly and ValueTask internally? or?
        // skip async/await in places to be faster, but then bad stacktrace?

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