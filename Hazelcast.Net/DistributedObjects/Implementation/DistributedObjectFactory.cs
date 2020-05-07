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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.DistributedObjects.Implementation.Map;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Represents a factory that creates <see cref="IDistributedObject"/> instances.
    /// </summary>
    internal class DistributedObjectFactory
    {
        private readonly ConcurrentDictionary<DistributedObjectInfo, ValueTask<IDistributedObject>> _objects
            = new ConcurrentDictionary<DistributedObjectInfo, ValueTask<IDistributedObject>>();

        // no need to be concurrent as this is configured once
        private readonly Dictionary<string, Func<string, Type, DistributedObjectBase>> _factories
            = new Dictionary<string, Func<string, Type, DistributedObjectBase>>();

        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectFactory"/> class.
        /// </summary>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        public DistributedObjectFactory(Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));

            _factories[Constants.ServiceNames.Map] = (name, type) =>
            {
                // FIXME avoid Activator.CreateInstance, it's slow!
                // FIXME pass cluster + serializationService to factory!
                // FIXME factory method should be static Map.Create()?

                bool IsIMapInterface(Type t)
                    => t.IsInterface && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IMap<,>);

                // find the IMap<,> that we want to create
                var interfaceType = IsIMapInterface(type) ? type : type.GetInterfaces().FirstOrDefault(IsIMapInterface);
                if (interfaceType == null)
                    throw new InvalidOperationException("The specified type does not implement IMap<,>.");

                // get the TKey and TValue, and create the actual Map<TKey, TValue> type
                var tKey = interfaceType.GetGenericArguments()[0];
                var tValue = interfaceType.GetGenericArguments()[1];
                var actualType = typeof(Map<,>).MakeGenericType(tKey, tValue);

                // FIXME this is obviously TEMP, the sequence belongs to the HazelcastClient (or so it seems)
                // but then how would cross-client locks work?
                var lockReferenceIdSequence = new Int64Sequence();

                // create the actual instance
                var o = Activator.CreateInstance(actualType, Constants.ServiceNames.Map, name, _cluster, _serializationService, lockReferenceIdSequence, loggerFactory);
                if (!(o is DistributedObjectBase d))
                    throw new InvalidOperationException("The created object does not inherit from DistributedObjectBase.");

                return d;
            };

            // TODO: add factories for other services
            // TODO: provide a way to inject factories
        }

        /// <summary>
        /// Gets or creates an <see cref="IDistributedObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of the <see cref="IDistributedObject"/>.</typeparam>
        /// <param name="serviceName">The service name.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <returns>A task that will complete when the object has been retrieved or created, and
        /// represents the object that was retrieved or created.</returns>
        public async ValueTask<TObject> GetOrCreateAsync<TObject>(string serviceName, string name)
            where TObject : IDistributedObject
            => (TObject) await GetOrCreateAsync(typeof(TObject), serviceName, name, true);

        /// <summary>
        /// Gets or creates an <see cref="IDistributedObject"/>.
        /// </summary>
        /// <param name="type">The type of the <see cref="IDistributedObject"/>.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="remote">Whether to initialize the object on the server.</param>
        /// <returns>A task that will complete when the object has been retrieved or created, and
        /// represents the object that was retrieved or created.</returns>
        private async ValueTask<IDistributedObject> GetOrCreateAsync(Type type, string serviceName, string name, bool remote)
        {
            var k = new DistributedObjectInfo(serviceName, name);

            // try to get the object - thanks to the concurrent dictionary there will be only 1 task
            // and if several concurrent requests are made, they will all await that same task
            IDistributedObject o;
            try
            {
                o = await _objects.GetOrAdd(k, x => CreateAsync(type, serviceName, name, remote));
            }
            catch
            {
                _objects.TryRemove(k, out _);
                throw;
            }

            // ReSharper disable once UseMethodIsInstanceOfType
            if (!type.IsAssignableFrom(o.GetType()))
            {
                // if the object that was retrieved is not of the right type, it's a problem
                // preserve the existing object, but throw
                throw new InvalidCastException("A distributed object with the specified service name and name, but "
                                               + "with a different type, has already been created.");
            }

            return o;
        }

        /// <summary>
        /// Creates an <see cref="IDistributedObject"/>.
        /// </summary>
        /// <param name="type">The type of the <see cref="IDistributedObject"/>.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="remote">Whether to initialize the object on the server.</param>
        /// <returns>A task that will complete when the object has been created, and
        /// represents the object that was created.</returns>
        private async ValueTask<IDistributedObject> CreateAsync(Type type, string serviceName, string name, bool remote)
        {
            // get a factory for the service
            if (!_factories.TryGetValue(serviceName, out var factory))
                throw new ArgumentException("no factory", nameof(serviceName));

            // let the factory create the object
            // TODO was filtered via ExceptionUtil.Rethrow that does weird things?
            var o = factory(name, type);

            // ReSharper disable once UseMethodIsInstanceOfType
            if (!type.IsAssignableFrom(o.GetType()))
            {
                // if the object that was created is not of the right type, it's a problem
                throw new InvalidOperationException("The distributed objects factory created an object with an unexpected type.");
            }

            // initialize the object
            if (remote)
            {
                var requestMessage = ClientCreateProxyCodec.EncodeRequest(o.Name, o.ServiceName);
                XConsole.WriteLine(this, "Send initialize request");
                var responseMessage = await _cluster.SendAsync(requestMessage);
                XConsole.WriteLine(this, "Rcvd initialize response");
            }

            o.OnInitialized();

            return o;
        }
    }
}