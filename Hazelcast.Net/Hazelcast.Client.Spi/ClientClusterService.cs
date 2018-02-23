// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Security;
using Hazelcast.Util;
using ICredentials = Hazelcast.Security.ICredentials;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    ///     The
    ///     <see cref="Hazelcast.Client.Spi.ClientClusterService" />
    ///     implementation.
    /// </summary>
    internal class ClientClusterService : IClientClusterService, IConnectionListener, IConnectionHeartbeatListener
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientClusterService));
        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<string, IMembershipListener> _listeners =
            new ConcurrentDictionary<string, IMembershipListener>();

        private readonly AtomicReference<IDictionary<Address, IMember>> _membersRef =
            new AtomicReference<IDictionary<Address, IMember>>();

        private ClientMembershipListener _clientMembershipListener;
        private ClientConnectionManager _connectionManager;
        private ICredentials _credentials;
        private Address _ownerConnectionAddress;
        private ClientPrincipal _principal;

        public ClientClusterService(HazelcastClient client)
        {
            _client = client;

            var listenerConfigs = client.GetClientConfig().GetListenerConfigs();
            foreach (var listenerConfig in listenerConfigs)
            {
                var listener = listenerConfig.GetImplementation();
                if (listener == null)
                {
                    try
                    {
                        var className = listenerConfig.GetClassName();
                        var type = Type.GetType(className);
                        if (type != null)
                        {
                            listener = Activator.CreateInstance(type) as IEventListener;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Severe(e);
                    }
                }
                var membershipListener = listener as IMembershipListener;
                if (membershipListener != null)
                {
                    AddMembershipListenerWithoutInit(membershipListener);
                }
            }
        }

        public IMember GetMember(Address address)
        {
            var members = _membersRef.Get();
            if (members == null) return null;
            return members.ContainsKey(address) ? members[address] : null;
        }

        public IMember GetMember(string uuid)
        {
            var memberList = GetMemberList();
            return memberList.FirstOrDefault(member => uuid.Equals(member.GetUuid()));
        }

        public ICollection<IMember> GetMemberList()
        {
            var members = _membersRef.Get();
            return members != null ? members.Values : new HashSet<IMember>();
        }

        public Address GetMasterAddress()
        {
            var master = GetMemberList().FirstOrDefault();
            return master == null ? null : master.GetAddress();
        }

        public int GetSize()
        {
            return GetMemberList().Count;
        }

        public long GetClusterTime()
        {
            return Clock.CurrentTimeMillis();
        }

        public IClient GetLocalClient()
        {
            var cm = (ClientConnectionManager) _client.GetConnectionManager();
            var cp = GetPrincipal();
            var ownerConnection = cm.GetConnection(_ownerConnectionAddress);

            var socketAddress = ownerConnection != null ? ownerConnection.GetLocalSocketAddress() : null;
            var uuid = cp != null ? cp.GetUuid() : null;
            return new Client(uuid, socketAddress);
        }

        public Address GetOwnerConnectionAddress()
        {
            return _ownerConnectionAddress;
        }

        public string AddMembershipListener(IMembershipListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            var id = Guid.NewGuid().ToString();
            _listeners[id] = listener;
            if (listener is IInitialMembershipListener)
            {
                // TODO: needs sync with membership events...
                var cluster = _client.GetCluster();
                ((IInitialMembershipListener) listener).Init(new InitialMembershipEvent(cluster, cluster.GetMembers()));
            }
            return id;
        }

        public bool RemoveMembershipListener(string registrationId)
        {
            if (registrationId == null)
            {
                throw new ArgumentNullException("registrationId");
            }
            IMembershipListener removed;
            return _listeners.TryRemove(registrationId, out removed);
        }

        public void HeartBeatStarted(ClientConnection connection)
        {
        }

        public void HeartBeatStopped(ClientConnection connection)
        {
            if (connection.GetAddress().Equals(_ownerConnectionAddress))
            {
                _connectionManager.DestroyConnection(connection, new TargetDisconnectedException(_ownerConnectionAddress));
            }
        }

        public void ConnectionAdded(ClientConnection connection)
        {
        }

        public void ConnectionRemoved(ClientConnection connection)
        {
            var executionService = (ClientExecutionService) _client.GetClientExecutionService();
            if (Equals(connection.GetAddress(), _ownerConnectionAddress))
            {
                if (_client.GetLifecycleService().IsRunning())
                {
                    executionService.SubmitInternal(() =>
                    {
                        try
                        {
                            FireConnectionEvent(LifecycleEvent.LifecycleState.ClientDisconnected);
                            ConnectToCluster();
                        }
                        catch (Exception e)
                        {
                            Logger.Warning("Could not re-connect to cluster shutting down the client", e);
                            _client.GetLifecycleService().Shutdown();
                        }
                    });
                }
            }
        }

        public ClientPrincipal GetPrincipal()
        {
            return _principal;
        }

        public IMember GetRandomMember()
        {
            var rand = new Random();
            return GetMemberList().OrderBy(m => rand.Next()).FirstOrDefault();
        }

        /// <exception cref="System.Exception" />
        public virtual void Start()
        {
            Init();
            ConnectToCluster();
            InitMembershipListener();
        }

        internal virtual void FireMemberAttributeEvent(MemberAttributeEvent @event)
        {
            _client.GetClientExecutionService().Submit(() =>
            {
                foreach (var listener in _listeners.Values)
                {
                    listener.MemberAttributeChanged(@event);
                }
            });
        }

        internal virtual void FireMembershipEvent(MembershipEvent @event)
        {
            _client.GetClientExecutionService().Submit(
                (() =>
                {
                    foreach (var listener in _listeners.Values)
                    {
                        if (@event.GetEventType() == MembershipEvent.MemberAdded)
                        {
                            listener.MemberAdded(@event);
                        }
                        else
                        {
                            listener.MemberRemoved(@event);
                        }
                    }
                }));
        }

        internal virtual IDictionary<Address, IMember> GetMembersRef()
        {
            return _membersRef.Get();
        }

        internal virtual ISerializationService GetSerializationService()
        {
            return _client.GetSerializationService();
        }

        internal virtual string MembersString()
        {
            var sb = new StringBuilder("\n\nMembers [");
            var members = GetMemberList();
            sb.Append(members != null ? members.Count : 0);
            sb.Append("] {");
            if (members != null)
            {
                foreach (var member in members)
                {
                    sb.Append("\n\t").Append(member);
                }
            }
            sb.Append("\n}\n");
            return sb.ToString();
        }

        internal virtual void SetMembersRef(IDictionary<Address, IMember> map)
        {
            _membersRef.Set(map);
        }

        private void AddMembershipListenerWithoutInit(IMembershipListener listener)
        {
            var id = Guid.NewGuid().ToString();
            _listeners[id] = listener;
        }

        /// <exception cref="System.Exception" />
        private bool Connect(ICollection<IPEndPoint> triedAddresses)
        {
            ICollection<IPEndPoint> socketAddresses = GetEndpoints();
            foreach (var inetSocketAddress in socketAddresses)
            {
                try
                {
                    triedAddresses.Add(inetSocketAddress);
                    var address = new Address(inetSocketAddress);
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest("Trying to connect to " + address);
                    }
                    var connection = _connectionManager.GetOrConnect(address, ManagerAuthenticator);
                    // promote connection to owner if not already
                    if (!connection.IsOwner())
                    {
                        Logger.Finest("Promoting connection " + connection + " to owner.");
                        ManagerAuthenticator(connection);
                    }
                    FireConnectionEvent(LifecycleEvent.LifecycleState.ClientConnected);
                    _ownerConnectionAddress = connection.GetAddress();
                    return true;
                }
                catch (Exception e)
                {
                    var level = e is AuthenticationException ? LogLevel.Warning : LogLevel.Finest;
                    Logger.Log(level, "Exception during initial connection to " + inetSocketAddress, e);
                }
            }
            return false;
        }

        private void ConnectToCluster()
        {
            ConnectToOne();
            _clientMembershipListener.ListenMembershipEvents(_ownerConnectionAddress);
            //_clientListenerService.TriggerFailedListeners(); //TODO: triggerfailedlisteners
        }

        private void ConnectToOne()
        {
            _ownerConnectionAddress = null;
            var networkConfig = GetClientConfig().GetNetworkConfig();
            var connAttemptLimit = networkConfig.GetConnectionAttemptLimit();
            var connectionAttemptPeriod = networkConfig.GetConnectionAttemptPeriod();
            var connectionAttemptLimit = connAttemptLimit == 0 ? int.MaxValue : connAttemptLimit;
            var attempt = 0;
            ICollection<IPEndPoint> triedAddresses = new HashSet<IPEndPoint>();
            while (attempt < connectionAttemptLimit)
            {
                if (!_client.GetLifecycleService().IsRunning())
                {
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest("Giving up on retrying to connect to cluster since client is shutdown");
                    }
                    break;
                }
                attempt++;
                var nextTry = Clock.CurrentTimeMillis() + connectionAttemptPeriod;
                var isConnected = Connect(triedAddresses);
                if (isConnected)
                {
                    return;
                }
                var remainingTime = nextTry - Clock.CurrentTimeMillis();
                Logger.Warning(
                    string.Format("Unable to get alive cluster connection, try in {0} ms later, attempt {1} of {2}.",
                        Math.Max(0, remainingTime), attempt, connectionAttemptLimit));
                if (remainingTime > 0)
                {
                    try
                    {
                        Thread.Sleep((int) remainingTime);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            throw new InvalidOperationException("Unable to connect to any address in the config! " +
                                                "The following addresses were tried:" +
                                                string.Join(", ", triedAddresses));
        }

        private void FireConnectionEvent(LifecycleEvent.LifecycleState state)
        {
            var lifecycleService = (LifecycleService) _client.GetLifecycleService();
            lifecycleService.FireLifecycleEvent(state);
        }

        private ClientConfig GetClientConfig()
        {
            return _client.GetClientConfig();
        }

        private ICollection<IPEndPoint> GetConfigAddresses()
        {
            IEnumerable<IPEndPoint> socketAddresses = new List<IPEndPoint>();
            foreach (var address in _client.GetClientConfig().GetNetworkConfig().GetAddresses())
            {
                var endPoints = AddressHelper.GetSocketAddresses(address);
                socketAddresses = socketAddresses.Union(endPoints);
            }
            return socketAddresses.ToList();
        }

        private IList<IPEndPoint> GetEndpoints()
        {
            var memberList = _client.GetClientClusterService().GetMemberList();
            var endpoints = memberList.Select(member => member.GetSocketAddress());
            endpoints = endpoints.Union(GetConfigAddresses());

            var r = new Random();
            return endpoints.OrderBy(x => r.Next()).ToList();
        }

        private void Init()
        {
            _connectionManager = _client.GetConnectionManager();
            _clientMembershipListener = new ClientMembershipListener(_client);
            _connectionManager.AddConnectionHeartBeatListener(this);
            _connectionManager.AddConnectionListener(this);
            _credentials = _client.GetClientConfig().GetCredentials();
        }

        private void InitMembershipListener()
        {
            foreach (var membershipListener in _listeners.Values)
            {
                if (membershipListener is IInitialMembershipListener)
                {
                    // TODO: needs sync with membership events...
                    var cluster = _client.GetCluster();
                    var @event = new InitialMembershipEvent(cluster, cluster.GetMembers());
                    ((IInitialMembershipListener) membershipListener).Init(@event);
                }
            }
        }

        private void ManagerAuthenticator(ClientConnection connection)
        {
            Logger.Finest("Authenticating against the owner node");
            var ss = _client.GetSerializationService();

            string uuid = null;
            string ownerUuid = null;
            if (_principal != null)
            {
                uuid = _principal.GetUuid();
                ownerUuid = _principal.GetOwnerUuid();
            }
            ClientMessage request;
            if (_credentials is UsernamePasswordCredentials)
            {
                var usernamePasswordCr = (UsernamePasswordCredentials) _credentials;
                request = ClientAuthenticationCodec.EncodeRequest(usernamePasswordCr.GetUsername(),
                    usernamePasswordCr.GetPassword(), uuid, ownerUuid, true,
                    ClientTypes.Csharp, _client.GetSerializationService().GetVersion(), EnvironmentUtil.GetDllVersion());
            }
            else
            {
                var data = ss.ToData(_credentials);
                request = ClientAuthenticationCustomCodec.EncodeRequest(data, uuid, ownerUuid, false,
                    ClientTypes.Csharp, _client.GetSerializationService().GetVersion(), EnvironmentUtil.GetDllVersion());
            }

            IClientMessage response;
            try
            {
                var invocationService = (ClientInvocationService) _client.GetInvocationService();
                response = ThreadUtil.GetResult(invocationService.InvokeOnConnection(request, connection));
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            var result = ClientAuthenticationCodec.DecodeResponse(response);

            if (result.address == null)
            {
                throw new HazelcastException("Could not resolve address for owner node.");
            }

            var member = new Member(result.address, result.ownerUuid);
            _principal = new ClientPrincipal(result.uuid, result.ownerUuid);

            connection.Member  =member;
            connection.SetOwner();
        }
    }
}