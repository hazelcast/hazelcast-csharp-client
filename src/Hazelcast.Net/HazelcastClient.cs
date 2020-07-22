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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    internal partial class HazelcastClient : IHazelcastClient
    {
        private readonly HazelcastOptions _options;
        private readonly DistributedObjectFactory _distributedObjectFactory;
        private readonly NearCacheManager _nearCacheManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        /// <param name="options">The client configuration.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HazelcastClient(HazelcastOptions options, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<IHazelcastClient>();

            _distributedObjectFactory = new DistributedObjectFactory(Cluster, serializationService, loggerFactory);
            Cluster.OnConnectingToNewCluster = cancellationToken => _distributedObjectFactory.CreateAllAsync(cancellationToken);

            _nearCacheManager = new NearCacheManager(cluster, serializationService, loggerFactory, options.NearCache);

            // every async operations using this client will need a proper async context
            AsyncContext.Ensure();
        }

        /// <summary>
        /// Gets the <see cref="Cluster"/>.
        /// </summary>
        public Cluster Cluster { get; }

        /// <summary>
        /// Gets the <see cref="ISerializationService"/>.
        /// </summary>
        public ISerializationService SerializationService { get; }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task ConnectAsync(TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(ConnectAsync, timeout, _options.Networking.ConnectionTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task ConnectAsync(CancellationToken cancellationToken)
        {
            var task = Cluster.ConnectAsync(cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // order is important,
            // don't dispose the cluster before the rest!

            try
            {
                await _nearCacheManager.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the near cache manager.");
            }

            try
            {
                await _distributedObjectFactory.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the distributed object factory.");
            }

            try
            {
                await Cluster.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the cluster.");
            }
        }
    }
}
