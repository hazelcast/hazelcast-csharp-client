// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Security;
using Hazelcast.Util;
using static Hazelcast.Util.ExceptionUtil;

namespace Hazelcast.Client.Network
{
    internal class ConnectionManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ConnectionManager));
        private const string ClientTypeCsharp = "CSP";

        private readonly HazelcastClient _client;
        private readonly ClientNetworkConfig _networkConfig;

        private readonly ConcurrentDictionary<Guid, Connection> _connections = new ConcurrentDictionary<Guid, Connection>();

        private readonly ConcurrentDictionary<Address, IPEndPoint> _inetSocketAddressCache =
            new ConcurrentDictionary<Address, IPEndPoint>();

        private readonly ConcurrentDictionary<Address, bool> _connectingAddresses = new ConcurrentDictionary<Address, bool>();

        private readonly ConcurrentBag<IConnectionListener> _connectionListeners = new ConcurrentBag<IConnectionListener>();

        private readonly ILoadBalancer _loadBalancer;
        private readonly AtomicBoolean _isAlive = new AtomicBoolean(false);
        private readonly object _clientStateMutex = new object();

        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        private ISet<string> _labels;
        private int _connectionTimeout;
        private readonly HeartbeatManager _heartbeat;
        private TimeSpan _authenticationTimeout;
        private readonly bool _shuffleMemberList;
        private readonly WaitStrategy _waitStrategy;
        private Guid? _clusterId;
        private int _nextConnectionId;
        private ClientState _clientState = ClientState.INITIAL;
        private readonly ReconnectMode _reconnectMode;
        private readonly bool _asyncStart;
        private volatile bool _connectToClusterTaskSubmitted;

        public bool IsSmartRoutingEnabled { get; }

        public bool IsALive => _isAlive.Get();

        public ICollection<Connection> ActiveConnections => _connections.Values;


        public ConnectionManager(HazelcastClient client)
        {
            _client = client;
            _loadBalancer = client.LoadBalancer;
            //
            var config = client.ClientConfig;
            _networkConfig = config.GetNetworkConfig();
            IsSmartRoutingEnabled = _networkConfig.IsSmartRouting();
            _labels = client.ClientConfig.Labels;

            var connectionTimeout = _networkConfig.GetConnectionTimeout();
            _connectionTimeout = connectionTimeout == 0 ? int.MaxValue : connectionTimeout;

            //TODO outboundPorts
            // this.networking = initNetworking();
            // this.outboundPorts.addAll(getOutboundPorts());
            // this.outboundPortCount = outboundPorts.size();

            _heartbeat = new HeartbeatManager(client, this);

            _authenticationTimeout = _heartbeat.HeartbeatTimeout;
            _shuffleMemberList = EnvironmentUtil.ReadBool("hazelcast.client.shuffle.member.list") ?? false;
            _waitStrategy = InitializeWaitStrategy(client.ClientConfig);

            var connectionStrategyConfig = client.ClientConfig.GetConnectionStrategyConfig();
            _asyncStart = connectionStrategyConfig.AsyncStart;
            _reconnectMode = connectionStrategyConfig.ReconnectMode;
        }

        private WaitStrategy InitializeWaitStrategy(ClientConfig clientConfig)
        {
            var connectionStrategyConfig = clientConfig.GetConnectionStrategyConfig();
            var expoRetryConfig = connectionStrategyConfig.ConnectionRetryConfig;
            return new WaitStrategy(expoRetryConfig.InitialBackoffMillis, expoRetryConfig.MaxBackoffMillis,
                expoRetryConfig.Multiplier, expoRetryConfig.ClusterConnectTimeoutMillis, expoRetryConfig.Jitter);
        }


        public void Start()
        {
            if (!_isAlive.CompareAndSet(false, true))
            {
                return;
            }
            _heartbeat.Start();
            ConnectToCluster();
            if (IsSmartRoutingEnabled)
            {
                var interval = TimeSpan.FromSeconds(1);
                _client.ExecutionService.ScheduleWithFixedDelay(ConnectToAllClusterMembersTask, interval, interval,
                    _cancelToken.Token);
            }
        }

        private void ConnectToCluster()
        {
            if (_asyncStart)
            {
                SubmitConnectToClusterTask();
            }
            else
            {
                DoConnectToCluster();
            }
        }

        private void DoConnectToCluster()
        {
            ICollection<Address> triedAddresses = new HashSet<Address>();
            IList<Exception> exceptions = new List<Exception>();
            _waitStrategy.Reset();
            do
            {
                var addresses = GetPossibleMemberAddresses();
                foreach (var address in addresses)
                {
                    CheckClientActive();
                    triedAddresses.Add(address);
                    var connection = Connect(address, exceptions);
                    if (connection != null)
                    {
                        return;
                    }
                }

                // If the address providers load no addresses (which seems to be possible), then the above loop is not entered
                // and the lifecycle check is missing, hence we need to repeat the same check at this point.
                CheckClientActive();
            } while (_waitStrategy.Sleep());

            // notify when no succeeded cluster connection is found
            var msg =
                $"Unable to connect to any address from the cluster with name:{_client.ClientConfig.GetClusterName()}. The following addresses were tried: {string.Join(", ", triedAddresses)}";

            throw new InvalidOperationException(msg, new AggregateException(exceptions));
        }

        private Connection Connect(Address address, IList<Exception> exceptions)
        {
            try
            {
                Logger.Info("Trying to connect to " + address);
                return GetOrConnect(address);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                Logger.Warning("Exception during initial connection to " + address + ": " + e);
                return null;
            }
        }

        private IEnumerable<Address> GetPossibleMemberAddresses()
        {
            var memberList = _client.ClusterService.Members;
            var addresses = memberList.Select(member => member.Address).ToList();

            if (_shuffleMemberList)
            {
                addresses = AddressUtil.Shuffle(addresses);
            }

            var configAddresses = _client.AddressProvider.GetAddresses();
            if (_shuffleMemberList)
            {
                configAddresses = AddressUtil.Shuffle(configAddresses);
            }
            addresses.AddRange(configAddresses);
            return addresses;
        }

        private void ConnectToAllClusterMembersTask()
        {
            if (!_client.LifecycleService.IsRunning()) return;

            foreach (var member in _client.ClusterService.Members)
            {
                var address = member.Address;

                if (_client.LifecycleService.IsRunning() && GetConnection(address) == null &&
                    _connectingAddresses.TryAdd(address, true))
                {
                    // submit a task for this address only if there is no
                    // another connection attempt for it
                    _client.ExecutionService.Submit(() =>
                    {
                        try
                        {
                            if (!_client.LifecycleService.IsRunning()) return;

                            if (GetConnection(member.Uuid) == null)
                            {
                                GetOrConnect(address);
                            }
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                        finally
                        {
                            _connectingAddresses.TryAdd(address, true);
                        }
                    });
                }
            }
        }

        public void Shutdown()
        {
            if (!_isAlive.CompareAndSet(true, false))
            {
                return;
            }
            _cancelToken.Cancel();

            foreach (var connection in _connections.Values)
            {
                connection.Close("Hazelcast client is shutting down", null);
            }

            while (_connectionListeners.TryTake(out _)) ;
            _heartbeat.Shutdown();
        }

        public Connection GetRandomConnection()
        {
            if (IsSmartRoutingEnabled)
            {
                var member = _loadBalancer.Next();
                if (member != null)
                {
                    var connection = GetConnection(member.Uuid);
                    if (connection != null)
                    {
                        return connection;
                    }
                }
            }
            return _connections.Values.SingleOrDefault();
        }

        public Connection GetConnection(Guid memberGuid)
        {
            return _connections.TryGetValue(memberGuid, out var connection) ? connection : null;
        }

        private Connection GetConnection(Address address)
        {
            return _connections.Values.FirstOrDefault(connection => connection.RemoteAddress.Equals(address));
        }

        Connection GetOrConnect(Address address)
        {
            CheckClientActive();
            var connection = GetConnection(address);
            if (connection != null) return connection;

            lock (ResolveAddress(address))
            {
                // this critical section is used for making a single connection
                // attempt to the given address at a time.
                connection = GetConnection(address);
                if (connection != null) return connection;

                address = Translate(address);
                connection = CreateSocketConnection(address);
                AuthenticateOnCluster(connection);
                return connection;
            }
        }

        private void AuthenticateOnCluster(Connection connection)
        {
            var request = EncodeAuthenticationRequest();
            var future = _client.InvocationService.InvokeOnConnection(request, connection);
            ClientAuthenticationCodec.ResponseParameters response;
            try
            {
                var responseMsg = ThreadUtil.GetResult(future, _authenticationTimeout);
                response = ClientAuthenticationCodec.DecodeResponse(responseMsg);
            }
            catch (Exception e)
            {
                connection.Close("Failed to authenticate connection", e);
                throw Rethrow(e);
            }

            var authenticationStatus = (AuthenticationStatus) response.Status;

            switch (authenticationStatus)
            {
                case AuthenticationStatus.Authenticated:
                    HandleSuccessfulAuth(connection, response);
                    break;
                case AuthenticationStatus.CredentialsFailed:
                    var authException = new AuthenticationException("Invalid credentials!");
                    connection.Close("Failed to authenticate connection", authException);
                    throw authException;
                case AuthenticationStatus.NotAllowedInCluster:
                    var notAllowedException = new ClientNotAllowedInClusterException("Client is not allowed in the cluster");
                    connection.Close("Failed to authenticate connection", notAllowedException);
                    throw notAllowedException;
                case AuthenticationStatus.SerializationVersionMismatch:
                    var operationException =
                        new InvalidOperationException("Server serialization version does not match to client");
                    connection.Close("Failed to authenticate connection", operationException);
                    throw operationException;
                default:
                    var exception =
                        new AuthenticationException("Authentication status code not supported. status: " + authenticationStatus);
                    connection.Close("Failed to authenticate connection", exception);
                    throw exception;
            }
        }

        private void HandleSuccessfulAuth(Connection connection, ClientAuthenticationCodec.ResponseParameters response)
        {
            lock (_clientStateMutex)
            {
                CheckPartitionCount(response.PartitionCount);
                connection.ConnectedServerVersion = response.ServerHazelcastVersion;
                connection.RemoteAddress = response.Address;
                connection.RemoteGuid = response.MemberUuid;

                var newClusterId = response.ClusterId;

                var hasNoConnectionToCluster = _connections.IsEmpty;
                var changedCluster = hasNoConnectionToCluster && _clusterId.HasValue && !newClusterId.Equals(_clusterId);
                if (changedCluster)
                {
                    Logger.Warning($"Switching from current cluster: {_clusterId} to new cluster: {newClusterId}");
                    _client.OnClusterRestart();
                }

                _connections.TryAdd(response.MemberUuid, connection);
                Interlocked.Increment(ref _nextConnectionId);

                if (hasNoConnectionToCluster)
                {
                    _clusterId = newClusterId;
                    if (changedCluster)
                    {
                        //TODO cluster state????
                        _clientState = ClientState.CONNECTED_TO_CLUSTER;
                        _client.ExecutionService.Submit(() => InitializeClientOnCluster(newClusterId));
                    }
                    else
                    {
                        _clientState = ClientState.INITIALIZED_ON_CLUSTER;
                        _client.LifecycleService.FireLifecycleEvent(LifecycleEvent.LifecycleState.ClientConnected);
                    }
                }
                Logger.Info(string.Format("Authenticated with server {0}:{1}, server version: {2}, local address: {3}",
                    response.Address, response.MemberUuid, response.ServerHazelcastVersion, connection.GetLocalSocketAddress()));
                FireConnectionAddedEvent(connection);
            }

            // It could happen that this connection is already closed and
            // onConnectionClose() is called even before the synchronized block
            // above is executed. In this case, now we have a closed but registered
            // connection. We do a final check here to remove this connection
            // if needed.
            if (!connection.IsAlive)
            {
                OnConnectionClose(connection);
            }
        }

        private void InitializeClientOnCluster(Guid targetClusterId)
        {
            // submitted inside synchronized(clientStateMutex)
            try
            {
                lock (_clientStateMutex)
                {
                    if (!targetClusterId.Equals(_clusterId))
                    {
                        Logger.Warning("Won't send client state to cluster: " + targetClusterId +
                                       " Because switched to a new cluster: " + _clusterId);
                        return;
                    }
                }

                _client.SendStateToCluster();

                lock (_clientStateMutex)
                {
                    if (targetClusterId.Equals(_clusterId))
                    {
                        if (Logger.IsFinestEnabled)
                        {
                            Logger.Finest("Client state is sent to cluster: " + targetClusterId);
                        }

                        _clientState = ClientState.INITIALIZED_ON_CLUSTER;
                        _client.LifecycleService.FireLifecycleEvent(LifecycleEvent.LifecycleState.ClientConnected);
                    }
                    else if (Logger.IsFinestEnabled)
                    {
                        Logger.Warning("Cannot set client state to " + ClientState.INITIALIZED_ON_CLUSTER +
                                       " because current cluster id: " + _clusterId + " is different than expected cluster id: " +
                                       targetClusterId);
                    }
                }
            }
            catch (Exception e)
            {
                var clusterName = _client.ClusterService.ClusterName;
                Logger.Warning("Failure during sending state to the cluster.", e);
                lock (_clientStateMutex)
                {
                    if (targetClusterId.Equals(_clusterId))
                    {
                        if (Logger.IsFinestEnabled)
                        {
                            Logger.Warning("Retrying sending state to the cluster: " + targetClusterId + ", name: " +
                                           clusterName);
                        }
                        _client.ExecutionService.Submit(() => InitializeClientOnCluster(targetClusterId));
                    }
                }
            }
        }


        private void CheckPartitionCount(int newPartitionCount)
        {
            if (!_client.PartitionService.CheckAndSetPartitionCount(newPartitionCount))
            {
                throw new ClientNotAllowedInClusterException(string.Format(
                    "Client can not work with this cluster because it has a different partition count. " +
                    "Expected partition count: {0}, Member partition count: {1}", _client.PartitionService.GetPartitionCount(),
                    newPartitionCount));
            }
        }

        private ClientMessage EncodeAuthenticationRequest()
        {
            var serializationService = _client.SerializationService;
            var serializationVersion = serializationService.GetVersion();

            var credentials = _client.CredentialsFactory.NewCredentials();
            var clusterName = _client.ClientConfig.GetClusterName();

            var dllVersion = VersionUtil.GetDllVersion();
            var clientGuid = _client.ClientGuid;
            if (credentials is IPasswordCredentials passwordCredentials)
            {
                return ClientAuthenticationCodec.EncodeRequest(clusterName, passwordCredentials.Name,
                    passwordCredentials.Password, clientGuid, ClientTypeCsharp, serializationVersion, dllVersion, _client.Name,
                    _labels);
            }
            byte[] secretBytes;
            if (credentials is ITokenCredentials tokenCredentials)
            {
                secretBytes = tokenCredentials.Token;
            }
            else
            {
                secretBytes = serializationService.ToData(credentials).ToByteArray();
            }
            return ClientAuthenticationCustomCodec.EncodeRequest(clusterName, secretBytes, clientGuid, ClientTypeCsharp,
                serializationVersion, dllVersion, _client.Name, _labels);
        }

        private Connection CreateSocketConnection(Address address)
        {
            var conn = new Connection(this, _client.InvocationService, _nextConnectionId, address, _networkConfig);
            conn.NetworkInit();
            return conn;
        }

        private Address Translate(Address target)
        {
            var addressProvider = _client.AddressProvider;
            try
            {
                var translatedAddress = addressProvider.TranslateToPublic(target);
                if (translatedAddress == null)
                {
                    throw new NullReferenceException($"Address Provider could not translate address {target}");
                }

                return translatedAddress;
            }
            catch (Exception e)
            {
                Logger.Warning($"Failed to translate address {target} via address provider {e.Message}");
                throw Rethrow(e);
            }
        }

        private IPEndPoint ResolveAddress(Address target)
        {
            return _inetSocketAddressCache.GetOrAdd(target, address =>
            {
                try
                {
                    return target.GetInetSocketAddress();
                }
                catch (SocketException e)
                {
                    throw Rethrow(e);
                }
            });
        }

        private void CheckClientActive()
        {
            if (!_client.LifecycleService.IsRunning())
            {
                throw new HazelcastClientNotActiveException();
            }
        }

        public Connection GetTargetOrRandomConnection(Guid? memberGuid)
        {
            Connection connection = null;
            if (memberGuid.HasValue)
            {
                connection = GetConnection(memberGuid.Value);
            }
            return connection ?? GetRandomConnection();
        }

        public void ConnectToAllClusterMembers()
        {
            if (!IsSmartRoutingEnabled)
            {
                return;
            }

            foreach (var member in _client.ClusterService.Members)
            {
                try
                {
                    GetOrConnect(member.Address);
                }
                catch (Exception e)
                {
                    //ignore
                }
            }
        }

        public void AddConnectionListener(IConnectionListener connectionListener)
        {
            _connectionListeners.Add(connectionListener);
        }

        private void FireConnectionAddedEvent(Connection connection)
        {
            foreach (var connectionListener in _connectionListeners)
            {
                connectionListener.ConnectionAdded(connection);
            }
        }

        private void FireConnectionRemovedEvent(Connection connection)
        {
            foreach (var connectionListener in _connectionListeners)
            {
                connectionListener.ConnectionRemoved(connection);
            }
        }

        public void OnConnectionClose(Connection connection)
        {
            var endpoint = connection.RemoteAddress;
            var memberUuid = connection.RemoteGuid;

            if (endpoint == null)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest(
                        $"Destroying {connection} , but it has end-point set to null -> not removing it from a connection dict.");
                }
                return;
            }

            lock (_clientStateMutex)
            {
                if (_connections.TryUpdate(memberUuid, null, connection))
                {
                    _connections.TryRemove(memberUuid, out _);
                    Logger.Info($"Removed connection to endpoint: {endpoint}:{memberUuid}, connection: {connection}");
                    if (_connections.IsEmpty)
                    {
                        if (_clientState == ClientState.INITIALIZED_ON_CLUSTER)
                        {
                            _client.LifecycleService.FireLifecycleEvent(LifecycleEvent.LifecycleState.ClientDisconnected);
                        }

                        TriggerClusterReconnection();
                    }

                    FireConnectionRemovedEvent(connection);
                }
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest(
                        $"Destroying a connection, but there is no mapping {endpoint}:{memberUuid} -> {connection}  in the connection dict.");
                }
            }
        }

        private void TriggerClusterReconnection()
        {
            if (_reconnectMode == ReconnectMode.OFF)
            {
                Logger.Info("RECONNECT MODE is off. Shutting down the client.");
                ShutdownWithExternalThread();
                return;
            }

            if (_client.LifecycleService.IsRunning())
            {
                try
                {
                    SubmitConnectToClusterTask();
                }
                catch (Exception r)
                {
                    ShutdownWithExternalThread();
                }
            }
        }

        private void SubmitConnectToClusterTask()
        {
            // called in synchronized(clusterStateMutex)
            if (_connectToClusterTaskSubmitted)
            {
                return;
            }

            _client.ExecutionService.Submit(() =>
            {
                try
                {
                    DoConnectToCluster();
                    lock (_clientStateMutex)
                    {
                        _connectToClusterTaskSubmitted = false;
                        if (_connections.IsEmpty)
                        {
                            if (Logger.IsFinestEnabled)
                            {
                                Logger.Warning($"No connection to cluster: {_clusterId}");
                            }
                            SubmitConnectToClusterTask();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning("Could not connect to any cluster, shutting down the client", e);
                    ShutdownWithExternalThread();
                }
            });
            _connectToClusterTaskSubmitted = true;
        }

        private void ShutdownWithExternalThread()
        {
            new Thread(() =>
            {
                try
                {
                    _client.LifecycleService.Shutdown();
                }
                catch (Exception exception)
                {
                    Logger.Severe("Exception during client shutdown", exception);
                }
            }) {Name = $"{_client.Name}.clientShutdown-"}.Start();
        }

        private enum ClientState
        {
            /**
         * Clients start with this state. Once a client connects to a cluster,
         * it directly switches to {@link #INITIALIZED_ON_CLUSTER} instead of
         * {@link #CONNECTED_TO_CLUSTER} because on startup a client has no
         * local state to send to the cluster.
         */
            INITIAL,

            /**
         * When a client switches to a new cluster, it moves to this state.
         * It means that the client has connected to a new cluster but not sent
         * its local state to the new cluster yet.
         */
            CONNECTED_TO_CLUSTER,

            /**
         * When a client sends its local state to the cluster it has connected,
         * it switches to this state. When a client loses all connections to
         * the current cluster and connects to a new cluster, its state goes
         * back to {@link #CONNECTED_TO_CLUSTER}.
         * <p>
         * Invocations are allowed in this state.
         */
            INITIALIZED_ON_CLUSTER
        }
    }
}