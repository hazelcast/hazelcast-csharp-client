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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Partitioning;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the state of the cluster.
    /// </summary>
    internal class ClusterState : IAsyncDisposable
    {
        private readonly CancellationTokenSource _clusterCancellation = new CancellationTokenSource(); // general kill switch
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1); // general lock
        private readonly StateChangeQueue _stateChangeQueue;

        private volatile bool _readonlyProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterState"/> class.
        /// </summary>
        public ClusterState(IClusterOptions options, string clusterName, string clientName, Partitioner partitioner, ILoggerFactory loggerFactory)
        {
            Options = options;
            ClusterName = clusterName;
            ClientName = clientName;
            Partitioner = partitioner;
            LoggerFactory = loggerFactory;

            _stateChangeQueue = new StateChangeQueue(loggerFactory);
        }

        #region Events

        /// <summary>
        /// Triggers when the state changes.
        /// </summary>
        public Func<ConnectionState, ValueTask> StateChanged
        {
            get => _stateChangeQueue.StateChanged;
            set
            {
                ThrowIfPropertiesAreReadOnly();
                _stateChangeQueue.StateChanged = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        #endregion

        #region Readonly Properties

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if properties (On...) are read-only.
        /// </summary>
        public void ThrowIfPropertiesAreReadOnly()
        {
            if (_readonlyProperties) throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
        }

        /// <summary>
        /// Sets properties (On...) as read-only.
        /// </summary>
        public void SetPropertiesReadOnly()
        {
            _readonlyProperties = true;
        }

        #endregion

        #region Infos

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

        #endregion

        #region ConnectionState

        /// <summary>
        /// Gets the connection state.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; } = ConnectionState.NotConnected;

        /// <summary>
        /// Transitions to a new connection state.
        /// </summary>
        /// <param name="to">The new state.</param>
        /// <param name="from">An optional expected current state.</param>
        /// <returns>A task that will complete once the cluster has transitioned to the new state.</returns>
        /// <exception cref="InvalidOperationException">The current state was not the expected state.</exception>
        public async ValueTask TransitionAsync(ConnectionState to, ConnectionState from = 0)
        {
            await _lock.WaitAsync(CancellationToken.None).CAF();

            try
            {
                if (from != 0 && from != ConnectionState)
                {
                    throw new InvalidOperationException($"Cannot transition to {to} from {from} because the current state is {ConnectionState}.");
                }
                ConnectionState = to;

                // queue will trigger events sequentially, in order, and in the background
                _stateChangeQueue.Add(to);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Whether the cluster is connected.
        /// </summary>
        public bool IsConnected => ConnectionState == ConnectionState.Connected;

        /// <summary>
        /// Whether the cluster is "up" i.e. connected or connecting.
        /// </summary>
        public bool IsUp => ConnectionState == ConnectionState.Connected ||
                            ConnectionState == ConnectionState.Connecting ||
                            ConnectionState == ConnectionState.Reconnecting;

        /// <summary>
        /// Whether the cluster is "down" i.e. not connected, or disconnecting.
        /// </summary>
        public bool IsDown => ConnectionState == ConnectionState.NotConnected ||
                              ConnectionState == ConnectionState.Disconnecting;

        /// <summary>
        /// Throws a <see cref="ClientNotConnectedException"/> if the cluster is not connected.
        /// </summary>
        /// <param name="innerException">An optional inner exception.</param>
        public void ThrowIfNotConnected(Exception innerException = null)
        {
            if (ConnectionState != ConnectionState.Connected) 
                throw new ClientNotConnectedException(innerException);
        }

        #endregion

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
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner { get; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Locks the state.
        /// </summary>
        public async ValueTask WaitAsync(CancellationToken cancellationToken = default)
            => await _lock.WaitAsync(cancellationToken).CAF();

        /// <summary>
        /// Unlocks the state.
        /// </summary>
        public void Release() => _lock.Release();

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
        /// Gets the connection identifier sequence.
        /// </summary>
        public ISequence<int> ConnectionIdSequence { get; } = new Int32Sequence();

        /// <summary>
        /// Throws a <see cref="ClientNotConnectedException"/> if the cluster operations have been canceled.
        /// </summary>
        public void ThrowIfCancelled()
        {
            if (_clusterCancellation.IsCancellationRequested) throw new ClientNotConnectedException();
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
        public async ValueTask DisposeAsync()
        {
            await _stateChangeQueue.DisposeAsync().CAF();
            _clusterCancellation.Dispose();
            _lock.Dispose();
        }
    }
}
