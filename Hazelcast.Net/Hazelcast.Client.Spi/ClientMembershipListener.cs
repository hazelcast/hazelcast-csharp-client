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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

#pragma warning disable CS1591
 namespace Hazelcast.Client.Spi
{
    internal class ClientMembershipListener
    {
        private const int InitialMembersTimeoutSeconds = 5;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientMembershipListener));
        private readonly HazelcastClient _client;
        private readonly ClientClusterService _clusterService;
        private readonly ClientConnectionManager _connectionManager;
        private readonly ISet<IMember> _members = new HashSet<IMember>();
        private readonly ClientPartitionService _partitionService;
        private ManualResetEventSlim _initialListFetched;

        public ClientMembershipListener(HazelcastClient client)
        {
            _client = client;
            _connectionManager = (ClientConnectionManager) client.GetConnectionManager();
            _partitionService = (ClientPartitionService) client.GetClientPartitionService();
            _clusterService = (ClientClusterService) client.GetClientClusterService();
        }

        public virtual void BeforeListenerRegister()
        {
        }

        private void HandleMember(IMember member, int eventType)
        {
            switch (eventType)
            {
                case MembershipEvent.MemberAdded:
                {
                    MemberAdded(member);
                    break;
                }

                case MembershipEvent.MemberRemoved:
                {
                    MemberRemoved(member);
                    break;
                }

                default:
                {
                    Logger.Warning("Unknown event type :" + eventType);
                    break;
                }
            }
            _partitionService.RefreshPartitions();
        }

        private void HandleMemberAttributeChange(string uuid, string key, int operationType, string value)
        {
            var memberMap = _clusterService.GetMembersRef();
            if (memberMap == null)
            {
                return;
            }
            foreach (var target in memberMap.Values)
            {
                if (target.GetUuid().Equals(uuid))
                {
                    var type = (MemberAttributeOperationType) operationType;
                    ((Member) target).UpdateAttribute(type, key, value);
                    var memberAttributeEvent = new MemberAttributeEvent(_client.GetCluster(), target, type, key,
                        value);
                    _clusterService.FireMemberAttributeEvent(memberAttributeEvent);
                    break;
                }
            }
        }

        private void HandleMemberCollection(ICollection<IMember> initialMembers)
        {
            var prevMembers = new HashSet<IMember>();
            if (_members.Any())
            {
                prevMembers = new HashSet<IMember>(_members);
                _members.Clear();
            }
            foreach (var initialMember in initialMembers)
            {
                _members.Add(initialMember);
            }
            
            if (prevMembers.Count == 0)
            {
                //this means this is the first time client connected to cluster
                Logger.Info(MembersString());
                var immutableSetOfMembers = ImmutableSetOfMembers();
                var cluster = _client.GetCluster();
                var initialMembershipEvent = new InitialMembershipEvent(cluster, immutableSetOfMembers);
                _clusterService.HandleInitialMembershipEvent(initialMembershipEvent);
                _initialListFetched.Set();
                return;
            }
            var events = DetectMembershipEvents(prevMembers);
            Logger.Info(MembersString());
            FireMembershipEvent(events);
            _initialListFetched.Set();
        }

        internal void ListenMembershipEvents(Address ownerConnectionAddress)
        {
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest("Starting to listen for membership events from " + ownerConnectionAddress);
            }
            _initialListFetched = new ManualResetEventSlim();
            try
            {
                var clientMessage = ClientAddMembershipListenerCodec.EncodeRequest(false);
                DistributedEventHandler handler = m => ClientAddMembershipListenerCodec.EventHandler
                    .HandleEvent(m, HandleMember, HandleMemberCollection, HandleMemberAttributeChange);

                try
                {
                    var connection = _connectionManager.GetConnection(ownerConnectionAddress);
                    if (connection == null)
                    {
                        throw new InvalidOperationException(
                            "Can not load initial members list because owner connection is null. Address "
                            + ownerConnectionAddress);
                    }
                    var invocationService = (ClientInvocationService) _client.GetInvocationService();
                    var future = invocationService.InvokeListenerOnConnection(clientMessage, handler, connection);
                    var response = ThreadUtil.GetResult(future);
                    //registration id is ignored as this listener will never be removed
                    var registirationId = ClientAddMembershipListenerCodec.DecodeResponse(response).response;
                    WaitInitialMemberListFetched();
                }
                catch (Exception e)
                {
                    throw ExceptionUtil.Rethrow(e);
                }
            }
            catch (Exception e)
            {
                if (_client.GetLifecycleService().IsRunning())
                {
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Warning("Error while registering to cluster events! -> " + ownerConnectionAddress, e);
                    }
                    else
                    {
                        Logger.Warning("Error while registering to cluster events! -> " + ownerConnectionAddress +
                                       ", Error: " + e);
                    }
                }
            }
        }

        private IList<MembershipEvent> DetectMembershipEvents(ISet<IMember> prevMembers) {
            var events = new List<MembershipEvent>();
            
            var eventMembers = ImmutableSetOfMembers();
            
            var newMembers = new LinkedList<IMember>();
            foreach (var member in _members)
            {
                if (!prevMembers.Remove(member))
                {
                    newMembers.AddLast(member);
                }
            }
            // removal events should be added before added events
            foreach (var member in  prevMembers)
            {
                events.Add(new MembershipEvent(_client.GetCluster(), member, MembershipEvent.MemberRemoved, eventMembers));
                var address = member.GetAddress();
                if (_clusterService.GetMember(address) == null)
                {
                    var connection = _connectionManager.GetConnection(address);
                    if (connection != null) {
                        _connectionManager.DestroyConnection(connection, new TargetDisconnectedException(address, "member left the cluster."));
                    }
                }
            }
            foreach (var member in newMembers)
            {
                events.Add(new MembershipEvent(_client.GetCluster(), member, MembershipEvent.MemberAdded, eventMembers));
            }
            return events;
        }
        private void FireMembershipEvent(IList<MembershipEvent> events)
        {
            foreach (var @event in events)
            {
                _clusterService.HandleMembershipEvent(@event);
            }
        }

        private ICollection<IMember> ImmutableSetOfMembers()
        {
            return new ReadOnlyCollection<IMember>(_members.ToList());
        }

        private void MemberAdded(IMember member)
        {
            _members.Add(member);
            Logger.Info(MembersString());
            var @event = new MembershipEvent(_client.GetCluster(), member, MembershipEvent.MemberAdded, ImmutableSetOfMembers());
            _clusterService.HandleMembershipEvent(@event);
        }

        private void MemberRemoved(IMember member)
        {
            _members.Remove(member);
            Logger.Info(MembersString());
            var connection = _connectionManager.GetConnection(member.GetAddress());
            if (connection != null)
            {
                _connectionManager.DestroyConnection(connection, new TargetDisconnectedException(member.GetAddress(),
                    "member left the cluster."));
            }
            var @event = new MembershipEvent(_client.GetCluster(), member, MembershipEvent.MemberRemoved, ImmutableSetOfMembers());
            _clusterService.HandleMembershipEvent(@event);
        }

        /// <exception cref="System.Exception" />
        private void WaitInitialMemberListFetched()
        {
            var timeout = TimeUnit.Seconds.ToMillis(InitialMembersTimeoutSeconds);
            var success = _initialListFetched.Wait((int) timeout);
            if (!success)
            {
                Logger.Warning("Error while getting initial member list from cluster!");
            }
        }
        
        private string MembersString() {
            var sb = new StringBuilder("\n\nMembers [");
            sb.Append(_members.Count);
            sb.Append("] {");
            foreach (var member in _members)
            {
                sb.Append("\n\t").Append(member);
            }
            sb.Append("\n}\n");
            return sb.ToString();
        }

    }
}