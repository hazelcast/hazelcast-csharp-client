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
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Partitioning;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the state of the cluster.
    /// </summary>
    internal class ClusterState : IDisposable
    {
        private readonly CancellationTokenSource _clusterCancellation = new CancellationTokenSource(); // general kill switch

        private bool _readonlyProperties;

        public ClusterState(IClusterOptions options, string clusterName, string clientName, Partitioner partitioner, ILoadBalancer loadBalancer, ILoggerFactory loggerFactory)
        {
            Options = options;
            ClusterName = clusterName;
            ClientName = clientName;
            Partitioner = partitioner;
            LoadBalancer = loadBalancer;
            LoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the unique identifier of the cluster, as assigned by the client.
        /// </summary>
        public Guid ClientId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the name of the cluster client, as assigned by the client.
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Gets the name of the cluster server.
        /// </summary>
        public string ClusterName { get; }

        /// <summary>
        /// Gets or sets the connection state.
        /// </summary>
        public ClusterConnectionState ConnectionState { get; set; } = ClusterConnectionState.NotConnected;

        /// <summary>
        /// Gets the cluster general <see cref="CancellationToken"/>.
        /// </summary>
        public CancellationToken CancellationToken => _clusterCancellation.Token;

        /// <summary>
        /// Cancels the cluster general <see cref="CancellationToken"/>.
        /// </summary>
        public void CancelOperations()
        {
            _clusterCancellation.Cancel();
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public IClusterOptions Options { get; }

        /// <summary>
        /// Whether smart routing is enabled.
        /// </summary>
        public bool IsSmartRouting => Options.Networking.SmartRouting;

        /// <summary>
        /// Whether the cluster is connected.
        /// </summary>
        public bool IsConnected => ConnectionState == ClusterConnectionState.Connected;

        /// <summary>
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner { get; }

        /// <summary>
        /// Gets the load balancer.
        /// </summary>
        public ILoadBalancer LoadBalancer { get; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the cluster general lock.
        /// </summary>
        public SemaphoreSlim ClusterLock { get; } = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Gets the cluster instrumentation.
        /// </summary>
        public ClusterInstrumentation Instrumentation { get; } = new ClusterInstrumentation();

        /// <summary>
        /// Gets the correlation identifier sequence.
        /// </summary>
        public ISequence<long> CorrelationIdSequence { get; } = new Int64Sequence();

        /// <summary>
        /// Gets the next correlation identifier.
        /// </summary>
        /// <returns>The next correlation identifier.</returns>
        public long GetNextCorrelationId() => CorrelationIdSequence.GetNext();

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if properties (On...) are read-only.
        /// </summary>
        public void ThrowIfReadOnlyProperties()
        {
            if (_readonlyProperties) throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
        }

        /// <summary>
        /// Mark properties (On...) as read-only.
        /// </summary>
        public void MarkPropertiesReadOnly()
        {
            _readonlyProperties = true;
        }

        /// <summary>
        /// Throws a <see cref="ClientNotConnectedException"/> if the cluster operations have been canceled.
        /// </summary>
        public void ThrowIfCancelled()
        {
            if (_clusterCancellation.IsCancellationRequested) throw new ClientNotConnectedException();
        }

        /// <summary>
        /// Throws a <see cref="ClientNotConnectedException"/> if the cluster is not connected.
        /// </summary>
        /// <param name="innerException">An optional inner exception.</param>
        public void ThrowIfNotConnected(Exception innerException = null)
        {
            if (ConnectionState != ClusterConnectionState.Connected) throw new ClientNotConnectedException(innerException);
        }

        /// <summary>
        /// Gets a <see cref="CancellationTokenSource"/> obtained by linking the cluster general
        /// cancellation with the supplied <paramref name="cancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="throwIfNotConnected">Whether to throw immediately if the cluster is not connected.</param>
        /// <returns>A new <see cref="CancellationTokenSource"/>obtained by linking the cluster general
        /// cancellation with the supplied <paramref name="cancellationToken"/>.</returns>
        /// <remarks>
        /// <para>The called must ensure that the returned <see cref="CancellationTokenSource"/> is
        /// eventually disposed.</para>
        /// </remarks>
        public CancellationTokenSource GetLinkedCancellation(CancellationToken cancellationToken, bool throwIfNotConnected = true)
        {
            // fail fast
            if (throwIfNotConnected) ThrowIfNotConnected();

            // note: that is a bad idea - what we return will be disposed, and we certainly do not
            // want the main _clusterCancellation to be disposed! plus, LinkedWith invoked with
            // a default CancellationToken will lead to practically doing nothing anyways
            //
            // succeed fast
            //if (cancellationToken == default) return _clusterCancellation;

            // still, there is a race condition - a chance that the _clusterCancellation
            // is gone by the time we use it = handle the situation here
            try
            {
                return _clusterCancellation.LinkedWith(cancellationToken);
            }
            catch
            {
                throw new ClientNotConnectedException();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _clusterCancellation.Dispose();
        }
    }
}
