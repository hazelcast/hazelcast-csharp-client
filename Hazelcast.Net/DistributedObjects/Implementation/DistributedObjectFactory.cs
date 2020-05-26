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
using System.Threading;
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
    internal class DistributedObjectFactory : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<DistributedObjectInfo, ValueTask<DistributedObjectBase>> _objects
            = new ConcurrentDictionary<DistributedObjectInfo, ValueTask<DistributedObjectBase>>();

        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;

        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectFactory"/> class.
        /// </summary>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public DistributedObjectFactory(Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Gets or creates a distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the distributed object.</typeparam>
        /// <param name="serviceName">The unique name of the service.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="remote">Whether to create the object remotely too.</param>
        /// <param name="factory">The object factory.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The distributed object.</returns>
        public async Task<T> GetOrCreateAsync<T>(string serviceName, string name, bool remote,
                                                 Func<string, Cluster, ISerializationService, ILoggerFactory, T> factory,
                                                 CancellationToken cancellationToken)
            where T: DistributedObjectBase
        {
            var k = new DistributedObjectInfo(serviceName, name);

            async ValueTask<DistributedObjectBase> CreateAsync()
            {
                var x = factory(name, _cluster, _serializationService, _loggerFactory);

                // initialize the object
                if (remote)
                {
                    var requestMessage = ClientCreateProxyCodec.EncodeRequest(x.Name, x.ServiceName);
                    XConsole.WriteLine(this, "Send initialize request");
                    _ = await _cluster.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
                    XConsole.WriteLine(this, "Rcvd initialize response");
                }

                x.OnInitialized();

                return x;
            }

            // try to get the object - thanks to the concurrent dictionary there will be only 1 task
            // and if several concurrent requests are made, they will all await that same task
            DistributedObjectBase o;
            try
            {
                o = await _objects.GetOrAdd(k, _ => CreateAsync()).ConfigureAwait(false);
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
        public async ValueTask CreateAllAsync(CancellationToken cancellationToken)
        {
            foreach (var (o, _) in _objects)
            {
                // TODO: what-if some succeed and some fail?
                var requestMessage = ClientCreateProxyCodec.EncodeRequest(o.Name, o.ServiceName);
                await _cluster.SendAsync(requestMessage, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // FIXME: implement!
            // also... we want to lock (read/write) while retrieving?

            //foreach (var t in _objects.Values)
            //{
            //    var o = await t;
            //    await o.DisposeAsync();
            //}
        }
    }
}