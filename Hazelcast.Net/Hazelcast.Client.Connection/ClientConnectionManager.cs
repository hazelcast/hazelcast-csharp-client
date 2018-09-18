// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

namespace Hazelcast.Client.Connection
{
    internal class ClientConnectionManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientConnectionManager));

        private readonly ConcurrentDictionary<Address, ClientConnection> _activeConnections =
            new ConcurrentDictionary<Address, ClientConnection>();

        private readonly HazelcastClient _client;

        private readonly ConcurrentBag<IConnectionListener> _connectionListeners =
            new ConcurrentBag<IConnectionListener>();

        private readonly object _connectionMutex = new object();
        private ICredentialsFactory _credentialsFactory;
        private readonly TimeSpan _heartbeatTimeout;
        private readonly TimeSpan _heartbeatInterval;

        private readonly ClientNetworkConfig _networkConfig;

        private readonly Dictionary<Address, Task<ClientConnection>> _pendingConnections =
            new Dictionary<Address, Task<ClientConnection>>();

        private readonly ISocketInterceptor _socketInterceptor;
        private CancellationTokenSource _heartbeatToken;
        private readonly AtomicBoolean _live = new AtomicBoolean(false);
        private int _nextConnectionId;

        public ClientConnectionManager(HazelcastClient client)
        {
            _client = client;

            var config = client.GetClientConfig();

            _networkConfig = config.GetNetworkConfig();

            config.GetNetworkConfig().IsRedoOperation();

            //init socketInterceptor
            var sic = config.GetNetworkConfig().GetSocketInterceptorConfig();
            if (sic != null && sic.IsEnabled())
            {
                //TODO Socket interceptor
                throw new NotImplementedException("Socket Interceptor not yet implemented.");
            }
            _socketInterceptor = null;

            const int defaultHeartbeatInterval = 5000;
            const int defaultHeartbeatTimeout = 60000;

            var heartbeatTimeoutMillis =
                EnvironmentUtil.ReadInt("hazelcast.client.heartbeat.timeout") ?? defaultHeartbeatTimeout;
            var heartbeatIntervalMillis =
                EnvironmentUtil.ReadInt("hazelcast.client.heartbeat.interval") ?? defaultHeartbeatInterval;
            
            _heartbeatTimeout = TimeSpan.FromMilliseconds(heartbeatTimeoutMillis);
            _heartbeatInterval = TimeSpan.FromMilliseconds(heartbeatIntervalMillis);
        }

        public void Start()
        {
            if (!_live.CompareAndSet(false, true))
            {
                return;
            }
            _credentialsFactory = _client.GetCredentialsFactory();
            
            //start Heartbeat
            _heartbeatToken = new CancellationTokenSource();
            _client.GetClientExecutionService().ScheduleWithFixedDelay(Heartbeat,
                (long) _heartbeatInterval.TotalMilliseconds,
                (long) _heartbeatInterval.TotalMilliseconds,
                TimeUnit.Milliseconds,
                _heartbeatToken.Token);
        }

        public void Shutdown()
        {
            if (!_live.CompareAndSet(true, false))
            {
                return;
            }

            try
            {
                _heartbeatToken.Cancel();
                foreach (var kvPair in _activeConnections)
                {
                    try
                    {
                        DestroyConnection(kvPair.Value,
                            new TargetDisconnectedException(kvPair.Key, "client shutting down"));
                    }
                    catch (Exception)
                    {
                        // ReSharper disable once InconsistentlySynchronizedField
                        Logger.Finest("Exception during closing connection on shutdown");
                    }
                }
            }
            catch (Exception e)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                Logger.Warning(e.Message);
            }
            finally
            {
                _heartbeatToken.Dispose();
            }
        }

        public ICredentials LastCredentials { get; set; }
        
        public ClientPrincipal ClientPrincipal { get; set; }

        public bool Live
        {
            get { return _live.Get(); }
        }
        
        public ICollection<ClientConnection> ActiveConnections
        {
            get { return _activeConnections.Values; }
        }

        public void AddConnectionListener(IConnectionListener connectionListener)
        {
            _connectionListeners.Add(connectionListener);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection GetOrConnect(Address address, bool isOwner)
        {
            if (address == null)
            {
                throw new ArgumentException("address");
            }
            lock (_connectionMutex)
            {
                ClientConnection connection;
                if (!_activeConnections.ContainsKey(address))
                {
                    connection = InitializeConnection(address, isOwner);
                    FireConnectionListenerEvent(f => f.ConnectionAdded(connection));
                    _activeConnections.TryAdd(connection.GetAddress(), connection);
                    Logger.Finest("Active list of connections: " + string.Join(", ", _activeConnections.Values));
                }
                else
                {
                    connection = _activeConnections[address];
                    // promote connection to owner if not already
                    if (!connection.IsOwner())
                    {
                        Logger.Finest("Promoting connection " + connection + " to owner.");
                        PromoteToOwner(connection);
                    }
                }
                return connection;
            }
        }
        
        /// <summary>
        /// Gets an existing connection for the given address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public ClientConnection GetConnection(Address address)
        {
            ClientConnection connection;
            return _activeConnections.TryGetValue(address, out connection) ? connection : null;
        }

        public void DestroyConnection(ClientConnection connection, Exception cause)
        {
            var address = connection.GetAddress();
            Logger.Finest("Destroying connection " + connection + " due to " + cause.Message);
            if (address != null)
            {
                ClientConnection conn;
                if (_activeConnections.TryRemove(address, out conn))
                {
                    connection.Close();
                    FireConnectionListenerEvent(f => f.ConnectionRemoved(connection));
                }
                else
                {
                    Logger.Warning("Could not find connection " + connection +
                                   " in list of connections, could not destroy connection. Current list of connections are " +
                                   string.Join(", ", _activeConnections.Keys));
                    connection.Close();
                }
            }
            else
            {
                // connection has not authenticated yet
                ((ClientInvocationService) _client.GetInvocationService()).CleanUpConnectionResources(connection);
            }
        }

        public Task<ClientConnection> GetOrConnectAsync(Address address)
        {
            lock (_pendingConnections)
            {
                if (!_pendingConnections.ContainsKey(address))
                {
                    var task = _client.GetClientExecutionService().Submit(() => GetOrConnect(address, isOwner:false));
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            if (Logger.IsFinestEnabled())
                            {
                                Logger.Finest("Exception in async pending connection:", t.Exception);
                            }
                        }
                        lock (_pendingConnections)
                        {
                            _pendingConnections.Remove(address);
                        }
                    });
                    _pendingConnections[address] = task;
                }
                return _pendingConnections[address];
            }
        }

        /// <exception cref="HazelcastException"></exception>
        private void CheckLive()
        {
            if (!_live.Get())
            {
                throw new HazelcastException("ConnectionManager is not active");
            }
        }

        private void Authenticate(ClientConnection connection, bool isOwnerConnection)
        {
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest(string.Format("Authenticating against the {0} node", isOwnerConnection?"owner":"non-owner"));
            }
            string uuid = null;
            string ownerUuid = null;
            if (ClientPrincipal != null)
            {
                uuid = ClientPrincipal.GetUuid();
                ownerUuid = ClientPrincipal.GetOwnerUuid();
            }

            var ss = _client.GetSerializationService();
            ClientMessage request;
            var credentials = _credentialsFactory.NewCredentials();
            LastCredentials = credentials;
            if (credentials.GetType() == typeof(UsernamePasswordCredentials))
            {
                var usernamePasswordCr = (UsernamePasswordCredentials) credentials;
                request = ClientAuthenticationCodec.EncodeRequest(usernamePasswordCr.Username, usernamePasswordCr.Password, uuid,
                    ownerUuid, isOwnerConnection, ClientTypes.Csharp, ss.GetVersion(), VersionUtil.GetDllVersion());
            }
            else
            {
                var data = ss.ToData(credentials);
                request = ClientAuthenticationCustomCodec.EncodeRequest(data, uuid, ownerUuid, isOwnerConnection,
                    ClientTypes.Csharp, ss.GetVersion(), VersionUtil.GetDllVersion());
            }

            IClientMessage response;
            try
            {
                var invocationService = (ClientInvocationService) _client.GetInvocationService();
                response = ThreadUtil.GetResult(invocationService.InvokeOnConnection(request, connection), _heartbeatTimeout);
            }
            catch (Exception e)
            {
                var ue = ExceptionUtil.Rethrow(e);
                Logger.Finest("Member returned an exception during authentication.", ue);
                throw ue;
            }
            var result = ClientAuthenticationCodec.DecodeResponse(response);

            if (result.address == null)
            {
                throw new HazelcastException("Could not resolve address for member.");
            }
            switch (result.status)
            {
                case AuthenticationStatus.Authenticated:
                    if (isOwnerConnection)
                    {
                        var member = new Member(result.address, result.ownerUuid);
                        ClientPrincipal = new ClientPrincipal(result.uuid, result.ownerUuid);
                        connection.Member = member;
                        connection.SetOwner();
                        connection.ConnectedServerVersionStr = result.serverHazelcastVersion;
                    }
                    else
                    {
                        var member = _client.GetClientClusterService().GetMember(result.address);
                        if (member == null)
                        {
                            throw new HazelcastException(string.Format("Node with address '{0}' was not found in the member list",
                                result.address));
                        }
                        connection.Member = member;
                    }
                    break;
                case AuthenticationStatus.CredentialsFailed:
                    throw new AuthenticationException("Invalid credentials! Principal: " + ClientPrincipal);
                case AuthenticationStatus.SerializationVersionMismatch:
                    throw new InvalidOperationException("Server serialization version does not match to client");
                default:
                    throw new AuthenticationException("Authentication status code not supported. status: " + result.status);
            }           
        }

        private void FireConnectionListenerEvent(Action<IConnectionListener> listenerAction)
        {
            foreach (var listener in _connectionListeners)
            {
                listenerAction(listener);
            }
        }

        private void Heartbeat()
        {
            if (!_live.Get()) return;

            var now = DateTime.Now;
            foreach (var connection in _activeConnections.Values)
            {
                CheckConnection(now, connection);
            }
        }

        private void CheckConnection(DateTime now, ClientConnection connection)
        {
            if (!connection.Live)
            {
                return;
            }

            if (now - connection.LastRead > _heartbeatTimeout)
            {
                if (connection.Live)
                {
                    Logger.Warning("Heartbeat failed over the connection: " + connection);
                    DestroyConnection(connection,
                        new TargetDisconnectedException("Heartbeat timed out to connection " + connection));
                }
            }

            if (now - connection.LastWrite > _heartbeatInterval)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Sending heartbeat to " + connection);
                }
                var request = ClientPingCodec.EncodeRequest();
                ((ClientInvocationService) _client.GetInvocationService()).InvokeOnConnection(request, connection);
            }
        }

        private ClientConnection InitializeConnection(Address address, bool isOwner)
        {
            CheckLive();
            ClientConnection connection = null;
            var id = _nextConnectionId;
            try
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Creating new connection for " + address + " with id " + id);
                }

                connection = new ClientConnection(this, (ClientInvocationService) _client.GetInvocationService(), id, address, _networkConfig);

                connection.Init(_socketInterceptor);
                Authenticate(connection, isOwner);
                Interlocked.Increment(ref _nextConnectionId);
                Logger.Finest("Authenticated to " + connection);
                return connection;
            }
            catch (Exception e)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Error connecting to " + address + " with id " + id, e);
                }

                if (connection != null)
                {
                    connection.Close();
                }
                throw ExceptionUtil.Rethrow(e, typeof (IOException), typeof (SocketException), 
                    typeof (TargetDisconnectedException));
            }
        }
        
        private void PromoteToOwner(ClientConnection connection)
        {
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest("Promoting a non-owner connection to owner connection!");
            }
            Authenticate(connection, true);
        }
    }
}