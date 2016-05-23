// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Security;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    internal class ClientConnectionManager : IClientConnectionManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (IClientConnectionManager));

        private readonly ConcurrentDictionary<Address, ClientConnection> _addresses =
            new ConcurrentDictionary<Address, ClientConnection>();

        private readonly HazelcastClient _client;

        private readonly ConcurrentBag<IConnectionListener> _connectionListeners =
            new ConcurrentBag<IConnectionListener>();

        private readonly object _connectionMutex = new object();
        private readonly ICredentials _credentials;
        private readonly int _heartBeatInterval = 5000;
        private readonly int _heartBeatTimeout = 60000;

        private readonly ConcurrentBag<IConnectionHeartbeatListener> _heatHeartbeatListeners =
            new ConcurrentBag<IConnectionHeartbeatListener>();

        private readonly ClientNetworkConfig _networkConfig;

        private readonly Dictionary<Address, Task<ClientConnection>> _pendingConnections =
            new Dictionary<Address, Task<ClientConnection>>();

        private readonly ISocketInterceptor _socketInterceptor;
        private Thread _heartBeatThread;
        private volatile bool _live;
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

            var timeout = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.invocation.timeout.seconds");
            if (timeout > 0)
            {
                ThreadUtil.TaskOperationTimeOutMilliseconds = timeout.Value * 1000;
            }

            _heartBeatTimeout = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.heartbeat.timeout") ??
                                _heartBeatTimeout;
            _heartBeatInterval = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.heartbeat.interval") ??
                                 _heartBeatInterval;
        }

        public void Start()
        {
            if (_live)
            {
                return;
            }
            _live = true;
            //start HeartBeat
            _heartBeatThread = new Thread(HearthBeatLoop)
            {
                IsBackground = true,
                Name = ("hz-heartbeat-" + new Random().Next()).Substring(0, 15)
            };
            _heartBeatThread.Start();
        }

        public bool Shutdown()
        {
            if (!_live)
            {
                return _live;
            }
            _live = false;

            //Stop heartBeat
            if (_heartBeatThread.IsAlive)
            {
                _heartBeatThread.Interrupt();
                _heartBeatThread.Join();
            }
            try
            {
                foreach (var kvPair in _addresses)
                {
                    try
                    {
                        DestroyConnection(kvPair.Value);
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
            return _live;
        }

        public bool Live
        {
            get { return _live; }
        }

        public void AddConnectionListener(IConnectionListener connectionListener)
        {
            _connectionListeners.Add(connectionListener);
        }

        public void AddConnectionHeartBeatListener(IConnectionHeartbeatListener connectonHeartbeatListener)
        {
            _heatHeartbeatListeners.Add(connectonHeartbeatListener);
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
                if (!_addresses.ContainsKey(address))
                {
                    connection = InitializeConnection(address, authenticator);
                    FireConnectionListenerEvent(f => f.ConnectionAdded(connection));
                    _addresses.TryAdd(connection.GetAddress(), connection);
                }
                else
                    connection = _addresses[address];

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
            return _addresses.TryGetValue(address, out connection) ? connection : null;
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

        public void DestroyConnection(ClientConnection connection)
        {
            var address = connection.GetAddress();
            // ReSharper disable once InconsistentlySynchronizedField
            Logger.Finest("Destroying connection " + connection);
            if (address != null)
            {
                ClientConnection conn;
                if (_addresses.TryRemove(address, out conn))
                {
                    connection.Close();
                    FireConnectionListenerEvent(f => f.ConnectionRemoved(connection));
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
            if (!_live)
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
                    ClientTypes.Csharp, _client.GetSerializationService().GetVersion());
            }
            else
            {
                var data = ss.ToData(_credentials);
                request = ClientAuthenticationCustomCodec.EncodeRequest(data, uuid, ownerUuid, false,
                    ClientTypes.Csharp, _client.GetSerializationService().GetVersion());
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
            connection.SetRemoteMember(member);
        }

        private void FireConnectionListenerEvent(Action<IConnectionListener> listenerAction)
        {
            foreach (var listener in _connectionListeners)
            {
                listenerAction(listener);
            }
        }

        private void FireHeartBeatEvent(Action<IConnectionHeartbeatListener> listenerAction)
        {
            foreach (var listener in _heatHeartbeatListeners)
            {
                listenerAction(listener);
            }
        }

        private void HearthBeatLoop()
        {
            while (_live)
            {
                foreach (var clientConnection in _addresses.Values)
                {
                    var request = ClientPingCodec.EncodeRequest();
                    var task = ((ClientInvocationService) _client.GetInvocationService()).InvokeOnConnection(request,
                        clientConnection);
                    try
                    {
                        var response = ThreadUtil.GetResult(task, _heartBeatTimeout);
                        ClientPingCodec.DecodeResponse(response);
                    }
                    catch (Exception e)
                    {
                        // ReSharper disable once InconsistentlySynchronizedField
                        Logger.Warning(string.Format("Error getting heartbeat from {0}: {1}",
                            clientConnection.GetAddress(), e));
                        var connection = clientConnection;
                        FireHeartBeatEvent(listener => listener.HeartBeatStopped(connection));
                    }
                }
                try
                {
                    Thread.Sleep(_heartBeatInterval);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private ClientConnection InitializeConnection(Address address, Authenticator authenticator)
        {
            CheckLive();
            ClientConnection connection = null;
            var id = _nextConnectionId;
            try
            {
                Logger.Finest("Creating new connection for " + address + " with id " + id);
                connection = new ClientConnection(this, (ClientInvocationService) _client.GetInvocationService(),
                    id,
                    address, _networkConfig);
                connection.Init();
                if (_socketInterceptor != null)
                {
                    _socketInterceptor.OnConnect(connection.GetSocket());
                }
                connection.SwitchToNonBlockingMode();
                authenticator(connection);
                Interlocked.Increment(ref _nextConnectionId);
                Logger.Finest("Authenticated to " + connection);
                return connection;
            }
            catch (Exception e)
            {
                Logger.Severe("Error connecting to " + address + " with id " + id, e);
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