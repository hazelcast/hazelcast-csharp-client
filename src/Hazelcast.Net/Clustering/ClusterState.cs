// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the state of the cluster.
    /// </summary>
    internal class ClusterState : IAsyncDisposable
    {
        private readonly CancellationTokenSource _clusterCancellation = new(); // general kill switch
        private readonly object _mutex = new();
        private readonly StateChangeQueue _stateChangeQueue;
        private readonly Failover _failover;
        private Action _shutdownRequested;
        private volatile bool _readonlyProperties;
        private ClusterVersion _clusterVersion = new ClusterVersion(Models.ClusterVersion.Unknown, ClusterVersion.Unknown);

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterState"/> class.
        /// </summary>
        public ClusterState(HazelcastOptions options, string clusterName, string clientName, Partitioner partitioner, ILoggerFactory loggerFactory)
        {
            Options = options;
            ClusterName = clusterName; // TODO: should remove? given name can be overriden ex: dev
            ClientName = clientName;
            Partitioner = partitioner;
            LoggerFactory = loggerFactory;

            AddressProvider = new AddressProvider(AddressProvider.GetSource(options.Networking, loggerFactory), LoggerFactory);

            _stateChangeQueue = new StateChangeQueue(loggerFactory);

            // FIXME failover should be created outside of the state class
            // and maybe it was wrong to add the AddressProvider here in the first place, too...
            // and... failover should not have a dependency on the state.
            // this all thing to be refactored
            _failover = new Failover(this, options);

            StateChanged += _failover.OnClusterStateChanged;

            _failover.ClusterChanged += cluster =>
            {
                AddressProvider.AddressProviderSource = AddressProvider.GetSource(cluster.Networking, loggerFactory);
                ClusterName = cluster.ClusterName;
            };

            HConsole.Configure(x => x.Configure<ClusterState>().SetPrefix("CLUST.STATE"));
        }


        #region Events

        /// <summary>
        /// Triggers when the state has changed.
        /// </summary>
        public Func<ClientState, ValueTask> StateChanged
        {
            get => _stateChangeQueue.StateChanged;
            set
            {
                _stateChangeQueue.StateChanged = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Triggers when shutdown is requested.
        /// </summary>
        public Action ShutdownRequested
        {
            get => _shutdownRequested;
            set
            {
                ThrowIfPropertiesAreReadOnly();
                _shutdownRequested = value;
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
        public string ClusterName { get; private set; }

        /// <summary>
        /// Cluster version.
        /// </summary>
        public ClusterVersion ClusterVersion => _clusterVersion;

        #endregion

        #region ClientState

        // NOTE: the initial ClientState is the default value, i.e. zero
        // we don't make it ClientState.Unknown because we don't want it
        // to be publicly visible, as this is a purely internal state

        /// <summary>
        /// Gets the client state.
        /// </summary>
        public ClientState ClientState { get; private set; }

        /// <summary>
        /// Sets the cluster version.
        /// </summary>
        /// <param name="clusterVersion">The new cluster version.</param>
        public void ChangeClusterVersion(ClusterVersion clusterVersion)
        {
            if(clusterVersion == null) return;
            
            lock (_mutex)
            {
                _clusterVersion = clusterVersion;    
            }
        }
        
        /// <summary>
        /// Changes the state, and pushes the change to the events queue.
        /// </summary>
        /// <param name="newState">The new state.</param>
        public void ChangeState(ClientState newState)
        {
            lock (_mutex)
            {
                ChangeStateLocked(newState);
            }
        }

        /// <summary>
        /// Changes the state if it is as expected, and pushes the change to the events queue.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <param name="expectedState">The expected state.</param>
        /// <returns><c>true</c> if the state was as expected, and thus changed; otherwise <c>false</c>.</returns>
        public bool ChangeState(ClientState newState, ClientState expectedState)
        {
            lock (_mutex)
            {
                if (ClientState != expectedState) return false;
                ChangeStateLocked(newState);
                return true;
            }
        }

        /// <summary>
        /// Changes the state if it is as expected, and pushes the change to the events queue.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <param name="expectedStates">The expected states.</param>
        /// <returns><c>true</c> if the state was as expected, and thus changed; otherwise <c>false</c>.</returns>
        public bool ChangeState(ClientState newState, params ClientState[] expectedStates)
        {
            lock (_mutex)
            {
                if (!expectedStates.Contains(ClientState)) return false;
                ChangeStateLocked(newState);
                return true;
            }
        }

        private void ChangeStateLocked(ClientState newState)
        {
            if (ClientState == newState) return;

            ClientState = newState;
            HConsole.WriteLine(this, $"{ClientName} state -> {ClientState}");
            // ReSharper disable once InconsistentlySynchronizedField - invoked from within lock
            _stateChangeQueue.Add(newState);
        }

        /// <summary>
        /// Changes the state if it is as expected, and pushes the change to the events queue,
        /// then waits for the event to be handled.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <returns>A task that will complete when the state change event has been handled.</returns>
        public async Task ChangeStateAndWait(ClientState newState)
        {
            Task wait;
            lock (_mutex)
            {
                if (ClientState == newState) return;

                ClientState = newState;
                HConsole.WriteLine(this, $"{ClientName} state -> {ClientState}");
                wait = _stateChangeQueue.AddAndWait(newState);
            }

            await wait.CfAwait();
        }

        /// <summary>
        /// Changes the state if it is as expected, and pushes the change to the events queue,
        /// then waits for the event to be handled.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <param name="expectedState">The expected state.</param>
        /// <returns><c>true</c> if the state was as expected, and thus changed, and the corresponding
        /// event has been handled; otherwise (not changed) <c>false</c>.</returns>
        public async Task<bool> ChangeStateAndWait(ClientState newState, ClientState expectedState)
        {
            Task wait;
            lock (_mutex)
            {
                if (ClientState != expectedState) return false;

                ClientState = newState;
                HConsole.WriteLine(this, $"{ClientName} state -> {ClientState}");
                wait = _stateChangeQueue.AddAndWait(newState);
            }

            await wait.CfAwait();
            return true;
        }

        /// <summary>
        /// Changes the state if it is as expected, and pushes the change to the events queue,
        /// then waits for the event to be handled.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <param name="expectedStates">The expected states.</param>
        /// <returns><c>true</c> if the state was as expected, and thus changed, and the corresponding
        /// event has been handled; otherwise (not changed) <c>false</c>.</returns>
        public async Task<bool> ChangeStateAndWait(ClientState newState, params ClientState[] expectedStates)
        {
            Task wait;
            lock (_mutex)
            {
                if (!expectedStates.Contains(ClientState)) return false;

                ClientState = newState;
                HConsole.WriteLine(this, $"{ClientName} state -> {ClientState}");
                wait = _stateChangeQueue.AddAndWait(newState);
            }

            await wait.CfAwait();
            return true;
        }

        /// <summary>
        /// Waits until connected, or it becomes impossible to connect.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><c>true</c> if connected; otherwise <c>false</c> meaning it has become impossible to connect.</returns>
        public ValueTask<bool> WaitForConnectedAsync(MemberConnection connection, CancellationToken cancellationToken)
        {
            TaskCompletionSource<ClientState> wait;

            lock (_mutex) // don't change the state now
            {
                // already connected
                if (ClientState == ClientState.Connected) return new ValueTask<bool>(true);

                // never going to be connected
                if (ClientState != ClientState.Started &&
                    ClientState != ClientState.ClusterChanged &&
                    ClientState != ClientState.Disconnected) return new ValueTask<bool>(false);

                // we are going to have to wait, register the StateChanged handler while state is locked
                wait = new TaskCompletionSource<ClientState>();
                _stateChangeQueue.StateChanged += OnStateChanged;
            }

            return WaitForConnectedAsync2(connection, wait, cancellationToken);

            ValueTask OnStateChanged(ClientState clientState)
            {
                // either connected, or never going to be connected
                if (clientState != ClientState.Started &&
                    clientState != ClientState.ClusterChanged &&
                    clientState != ClientState.Disconnected)
                {
                    _stateChangeQueue.StateChanged -= OnStateChanged;
                    wait.TrySetResult(clientState);
                }

                // keep waiting
                return default;
            }
        }

        private async ValueTask<bool> WaitForConnectedAsync2(MemberConnection connection, TaskCompletionSource<ClientState> wait, CancellationToken cancellationToken)
        {
            using var reg = cancellationToken.Register(() => wait.TrySetCanceled());

            await connection.AddClosed(OnConnectionClosed).CfAwait();

            ClientState state;
            HConsole.WriteLine(this, "Waiting for state change...");
            try
            {
                state = await wait.Task.CfAwait();
                HConsole.WriteLine(this, $"State changed to {state}");
            }
            catch (OperationCanceledException)
            {
                state = 0;
                HConsole.WriteLine(this, $"Connection operation was canceled");
            }
            catch (Exception ex)
            {
                state = 0;
                HConsole.WriteLine(this, $"{ex.GetType().Name}: {ex.Message}");
            }

            connection.RemoveClosed(OnConnectionClosed);
            return state == ClientState.Connected;

            ValueTask OnConnectionClosed(MemberConnection c)
            {
                // report a bogus state, just to break the wait
                wait.TrySetResult(0);
                return default;
            }
        }

        /// <summary>
        /// Whether the cluster is connected.
        /// </summary>
        public bool IsConnected => ClientState == ClientState.Connected;

        /// <summary>
        /// Whether the cluster is active i.e. connected or connecting.
        /// </summary>
        /// <remarks>
        /// <para>When the cluster is active it is either connected, or trying to get
        /// connected. It may make sense to retry operations that fail, because they
        /// should succeed when the cluster is eventually connected.</para>
        /// </remarks>
        public bool IsActive => ClientState.IsActiveState();

        /// <summary>
        /// Throws a <see cref="ClientOfflineException"/> if the cluster is not active.
        /// </summary>
        /// <param name="innerException">An optional inner exception.</param>
        public void ThrowIfNotActive(Exception innerException = null)
        {
            if (!IsActive) throw new ClientOfflineException(innerException, ClientState);
        }

#endregion

        public void ThrowIfNotConnectedAndAsyncReconnect()
        {
            var clientState = ClientState;
            if (clientState == ClientState.Disconnected && Options.Networking.ReconnectMode == ReconnectMode.ReconnectAsync)
                throw new ClientOfflineException(clientState);
        }
        public Exception ThrowClientOfflineException()
        {
            // due to a race condition between ClusterMembers potentially removing all its connections,
            // and ClusterConnections figuring we are now disconnected and changing the state, the state
            // here could still be ClientState.Connected - fix it.

            var clientState = ClientState;
            if (clientState == ClientState.Connected) clientState = ClientState.Disconnected;
            return new ClientOfflineException(clientState);
        }

        /// <summary>
        /// Requests that the client shuts down.
        /// </summary>
        public void RequestShutdown()
        {
            _shutdownRequested?.Invoke();
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public HazelcastOptions Options { get; }

        /// <summary>
        /// Whether smart routing is enabled.
        /// </summary>
        public bool IsSmartRouting => Options.Networking.SmartRouting;

        /// <summary>
        /// Gets Failover service.
        /// </summary>
        public Failover Failover => _failover;

        /// <summary>
        /// Gets the address provider.
        /// </summary>
        public AddressProvider AddressProvider { get; }

        /// <summary>
        /// Gets <see cref="ClusterOptions"/> of current cluster
        /// </summary>
        public HazelcastOptions CurrentClusterOptions => _failover.CurrentClusterOptions;

        /// <summary>
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner { get; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

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
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _stateChangeQueue.DisposeAsync().CfAwait();
            _clusterCancellation.Dispose();
        }
    }
}
