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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Logging;
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

        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectFactory"/> class.
        /// </summary>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        public DistributedObjectFactory(Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async ValueTask<T> GetOrCreateAsync<T>(string serviceName, string name, bool remote,
                                                      Func<string, Cluster, ISerializationService, ILoggerFactory, T> factory)
            where T: DistributedObjectBase
        {
            var k = new DistributedObjectInfo(serviceName, name);

            async ValueTask<IDistributedObject> CreateAsync()
            {
                var x = factory(name, _cluster, _serializationService, _loggerFactory);

                // initialize the object
                if (remote)
                {
                    var requestMessage = ClientCreateProxyCodec.EncodeRequest(x.Name, x.ServiceName);
                    XConsole.WriteLine(this, "Send initialize request");
                    var responseMessage = await _cluster.SendAsync(requestMessage);
                    XConsole.WriteLine(this, "Rcvd initialize response");
                }

                x.OnInitialized();

                return x;
            }

            // try to get the object - thanks to the concurrent dictionary there will be only 1 task
            // and if several concurrent requests are made, they will all await that same task
            IDistributedObject o;
            try
            {
                o = await _objects.GetOrAdd(k, _ => CreateAsync());
            }
            catch
            {
                _objects.TryRemove(k, out _);
                throw;
            }

            if (o is T t) return t;

            // if the object that was retrieved is not of the right type, it's a problem
            // preserve the existing object, but throw
            throw new InvalidCastException("A distributed object with the specified service name and name, but "
                                           + "with a different type, has already been created.");
        }

        /// <summary>
        /// Creates all known <see cref="IDistributedObject"/> on a cluster.
        /// </summary>
        /// <returns>A task that will complete when the state has been sent.</returns>
        /// <remarks>
        /// <para>This is used when connecting to a new cluster.</para>
        /// </remarks>
        public async ValueTask CreateAllAsync()
        {
            foreach (var (o, _) in _objects)
            {
                // TODO: what-if some succeed and some fail?
                var requestMessage = ClientCreateProxyCodec.EncodeRequest(o.Name, o.ServiceName);
                await _cluster.SendAsync(requestMessage);
            }
        }
    }
}