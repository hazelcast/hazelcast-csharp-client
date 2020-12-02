﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a factory that creates <see cref="IDistributedObject"/> instances.
    /// </summary>
    internal class DistributedObjectFactory : IAsyncDisposable
    {
        private readonly ConcurrentAsyncDictionary<DistributedObjectInfo, DistributedObjectBase> _objects
            = new ConcurrentAsyncDictionary<DistributedObjectInfo, DistributedObjectBase>();

        private readonly Cluster _cluster;
        private readonly SerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private volatile int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectFactory"/> class.
        /// </summary>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public DistributedObjectFactory(Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<DistributedObjectFactory>();
        }

        /// <summary>
        /// Gets or creates a distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the distributed object.</typeparam>
        /// <typeparam name="TImpl">The type of the implementation.</typeparam>
        /// <param name="serviceName">The unique name of the service.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="remote">Whether to create the object remotely too.</param>
        /// <param name="factory">The object factory.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The distributed object.</returns>
        public async Task<T> GetOrCreateAsync<T, TImpl>(
            string serviceName, string name, bool remote,
            Func<string, DistributedObjectFactory, Cluster, SerializationService, ILoggerFactory, TImpl> factory,
            CancellationToken cancellationToken = default)
            where TImpl : DistributedObjectBase, T
        {
            if (_disposed == 1) throw new ObjectDisposedException("DistributedObjectFactory");
            await _cluster.ThrowIfNotConnected().CAF();

            var info = new DistributedObjectInfo(serviceName, name);

            async ValueTask<DistributedObjectBase> CreateAsync(DistributedObjectInfo info2, CancellationToken token)
            {
                var x = factory(name, this, _cluster, _serializationService, _loggerFactory);
                x.OnDispose = ObjectDisposed; // this is why is has to be DistributedObjectBase

                // initialize the object
                if (remote)
                {
                    var requestMessage = ClientCreateProxyCodec.EncodeRequest(x.Name, x.ServiceName);
                    _ = await _cluster.Messaging.SendAsync(requestMessage, token).CAF();
                }

                x.OnInitialized();
                _logger.LogDebug("Initialized ({Object}) distributed object.", info2);
                return x;
            }

            // try to get the object - thanks to the concurrent dictionary there will be only 1 task
            // and if several concurrent requests are made, they will all await that same task

            var o = await _objects.GetOrAddAsync(info, CreateAsync, cancellationToken).CAF();

            // race condition: maybe the factory has been disposed and is already disposing
            // objects and will ignore this new object even though it has been added to the
            // dictionary, so take care of it ourselves
            if (_disposed == 1)
            {
                await o.DisposeAsync().CAF();
                throw new ObjectDisposedException("DistributedObjectFactory");
            }

            // if the object is a T then we can return it
            if (o is T t) return t;

            // otherwise, the client was already used to retrieve an object with the specified service
            // name and object name, but a different type, for instance IHList<int> vs IHList<string>,
            // and we just cannot support this = throw

            throw new HazelcastException($"A distributed object with the specified service name ({serviceName}) " +
                                         $"and object name ({name}) exists but of type {o.GetType().ToCsString()}, " +
                                         $"instead of {typeof(T).ToCsString()}.");
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
            await _cluster.ThrowIfNotConnected().CAF();

            await foreach (var (key, _) in _objects)
            {
                // if the cluster goes down, we want to stop everything
                // but each invocation is non-cancellable
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var requestMessage = ClientCreateProxyCodec.EncodeRequest(key.Name, key.ServiceName);
                    await _cluster.Messaging.SendAsync(requestMessage, cancellationToken).CAF();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to create ({key}) distributed object on new cluster.");
                }
            }
        }

        /// <summary>
        /// Deals with an object being disposed.
        /// </summary>
        /// <param name="o">The object.</param>
        private void ObjectDisposed(DistributedObjectBase o)
        {
            // simply disposing the distributed object removes it from the list
            var info = new DistributedObjectInfo(o.ServiceName, o.Name);
            _objects.TryRemove(info);
        }

        /// <summary>
        /// Destroys a distributed object.
        /// </summary>
        /// <param name="o">The distributed object.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask DestroyAsync(IDistributedObject o, CancellationToken cancellationToken = default)
        {
            // try to get the object - and then, dispose it

            var info = new DistributedObjectInfo(o.ServiceName, o.Name);
            var attempt = await _objects.TryGetAndRemoveAsync(info).CAF();
            if (attempt)
                await TryDispose(attempt.Value).CAF();

            // regardless of whether the object was known locally, destroy on server
            var clientMessage = ClientDestroyProxyCodec.EncodeRequest(o.Name, o.ServiceName);
            var responseMessage = await _cluster.Messaging.SendAsync(clientMessage, cancellationToken).CAF();
            _ = ClientDestroyProxyCodec.DecodeResponse(responseMessage);
        }


        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // there is a potential race-cond here, if an item is added to _objects after
            // we enumerate (capture) values, but it is taken care of in GetOrCreateAsync

            await foreach (var (_, value) in _objects)
            {
                await TryDispose(value).CAF();
            }
        }

        private async ValueTask TryDispose(IDistributedObject o)
        {
            try
            {
                await o.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to dispose ({o}) distributed object.", o);
            }
        }
    }
}
