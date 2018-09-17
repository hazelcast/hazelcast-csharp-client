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
        private readonly ICredentials _credentials;
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
            _credentials = config.GetCredentials();

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
        public ClientConnection GetOrConnect(Address address, Authenticator authenticator)
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
                    connection = InitializeConnection(address, authenticator);
                    FireConnectionListenerEvent(f => f.ConnectionAdded(connection));
                    _activeConnections.TryAdd(connection.GetAddress(), connection);
                    Logger.Finest("Active list of connections: " + string.Join(", ", _activeConnections.Values));
                }
                else
                    connection = _activeConnections[address];

                if (connection == null)
                {
                    Logger.Severe("CONNECTION Cannot be NULL here");
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

        /// <summary>
        /// Gets the connection for the address. If there is no connection, a new one will be created
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public ClientConnection GetOrConnect(Address target)
        {
            return GetOrConnect(target, ClusterAuthenticator);
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
                    var task = _client.GetClientExecutionService().Submit(() => GetOrConnect(address));
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

        private void ClusterAuthenticator(ClientConnection connection)
        {
            var ss = _client.GetSerializationService();
            var clusterService = (ClientClusterService) _client.GetClientClusterService();
            var principal = clusterService.GetPrincipal();

            var uuid = principal.GetUuid();
            var ownerUuid = principal.GetOwnerUuid();
            ClientMessage request;

            var usernamePasswordCr = _credentials as UsernamePasswordCredentials;
            if (usernamePasswordCr != null)
            {
                request = ClientAuthenticationCodec.EncodeRequest(usernamePasswordCr.GetUsername(),
                    usernamePasswordCr.GetPassword(), uuid, ownerUuid, false,
                    ClientTypes.Csharp, _client.GetSerializationService().GetVersion(), VersionUtil.GetDllVersion());
            }
            else
            {
                var data = ss.ToData(_credentials);
                request = ClientAuthenticationCustomCodec.EncodeRequest(data, uuid, ownerUuid, false,
                    ClientTypes.Csharp, _client.GetSerializationService().GetVersion(), VersionUtil.GetDllVersion());
            }

            IClientMessage response;
            try
            {
                var future = ((ClientInvocationService) _client.GetInvocationService()).InvokeOnConnection(request,
                    connection);
                response = ThreadUtil.GetResult(future);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            var rp = ClientAuthenticationCodec.DecodeResponse(response);

            if (rp.address == null)
            {
                throw new HazelcastException("Could not resolve address for member.");
            }

            var member = _client.GetClientClusterService().GetMember(rp.address);
            if (member == null)
            {
                throw new HazelcastException("Node with address '" + rp.address + "' was not found in the member list");
            }
            connection.Member = member;
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

        private ClientConnection InitializeConnection(Address address, Authenticator authenticator)
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
                authenticator(connection);
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
    }
}