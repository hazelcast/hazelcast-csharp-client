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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Client.Network;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;
using static Hazelcast.Util.ValidationUtil;

namespace Hazelcast.Client.Spi
{
    internal partial class ClusterService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClusterService));
        private const int InitialMembersTimeoutMillis = 120_000;

        private readonly HazelcastClient _client;
        private readonly ConnectionManager _connectionManager;
        private readonly InvocationService _clientInvocationService;
        private readonly PartitionService _partitionService;

        private static readonly MemberListSnapshot EmptySnapshot = new MemberListSnapshot(-1, new Dictionary<Guid, IMember>());

        private readonly AtomicReference<MemberListSnapshot> _memberListSnapshot =
            new AtomicReference<MemberListSnapshot>(EmptySnapshot);

        private readonly ConcurrentDictionary<Guid, IMembershipListener> _listeners =
            new ConcurrentDictionary<Guid, IMembershipListener>();

        private readonly IReadOnlyCollection<string> _labels;
        private readonly object _clusterViewLock = new object();
        private CountdownEvent _initialListFetchedLatch = new CountdownEvent(1);
        
        internal string ClusterName { get; }

        public ClusterService(HazelcastClient client)
        {
            _client = client;
            _labels = new ReadOnlyCollection<string>(_client.ClientConfig.Labels.ToList());
            _connectionManager = _client.ConnectionManager;
            _partitionService = client.PartitionService;
            _clientInvocationService = client.InvocationService;
            ClusterName = _client.ClientConfig.GetClusterName();
        }

        public IMember GetMember(Guid guid)
        {
            _memberListSnapshot.Get().Members.TryGetValue(guid, out var member);
            return member;
        }

        public ICollection<IMember> Members => _memberListSnapshot.Get().Members.Values;

        public int Count => Members.Count;

        public IEnumerable<IMember> DataMemberList =>
            _memberListSnapshot.Get().Members.Values.Where(member => member.IsLiteMember);


        public long ClusterTime => Clock.CurrentTimeMillis();

        public Guid AddMembershipListener(IMembershipListener listener)
        {
            CheckNotNull(listener, NullListenerIsNotAllowed);
            lock (_clusterViewLock)
            {
                var id = AddMembershipListenerWithoutInit(listener);
                if (listener is IInitialMembershipListener)
                {
                    var cluster = ((IHazelcastInstance) _client).Cluster;
                    var members = Members;
                    //if members are empty,it means initial event did not arrive yet
                    //it will be redirected to listeners when it arrives see #handleInitialMembershipEvent
                    if (members.Count != 0)
                    {
                        var @event = new InitialMembershipEvent(cluster, members);
                        ((IInitialMembershipListener) listener).Init(@event);
                    }
                }
                return id;
            }
        }

        private Guid AddMembershipListenerWithoutInit(IMembershipListener listener)
        {
            var id = Guid.NewGuid();
            _listeners.TryAdd(id, listener);
            return id;
        }


        public bool RemoveMembershipListener(Guid registrationId)
        {
            CheckNotNull(registrationId, "registrationId can't be null");
            return _listeners.TryRemove(registrationId, out _);
        }

        public void Start(ICollection<IEventListener> configuredListeners)
        {
            ListenerService.RegisterConfigListeners<IMembershipListener>(configuredListeners, AddMembershipListenerWithoutInit);
            _connectionManager.AddConnectionListener(this);
        }


        public void WaitInitialMemberListFetched()
        {
            try
            {
                var success = _initialListFetchedLatch.Wait(InitialMembersTimeoutMillis);
                if (!success)
                {
                    throw new InvalidOperationException("Could not get initial member list from cluster!");
                }
            }
            catch (ThreadInterruptedException e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public void ClearMemberListVersion()
        {
            lock (_clusterViewLock)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest("Resetting the member list version ");
                }
                var clusterViewSnapshot = _memberListSnapshot.Get();
                //This check is necessary so that `clearMemberListVersion` when handling auth response will not
                //intervene with client failover logic
                if (clusterViewSnapshot != EmptySnapshot)
                {
                    _memberListSnapshot.Set(new MemberListSnapshot(0, clusterViewSnapshot.Members));
                }
            }
        }

        private void ApplyInitialState(int version, ICollection<MemberInfo> memberInfos)
        {
            var snapshot = CreateSnapshot(version, memberInfos);
            _memberListSnapshot.Set(snapshot);
            Logger.Info(snapshot.ToString());
            var members = snapshot.Members.Values;
            var @event = new InitialMembershipEvent(((IHazelcastInstance) _client).Cluster, members);
            foreach (var listener in _listeners.Values)
            {
                if (listener is IInitialMembershipListener initialMembershipListener)
                {
                    initialMembershipListener.Init(@event);
                }
            }
        }

        private MemberListSnapshot CreateSnapshot(int memberListVersion, ICollection<MemberInfo> memberInfos)
        {
            var newMembers =
                memberInfos.ToDictionary<MemberInfo, Guid, IMember>(memberInfo => memberInfo.Uuid, memberInfo => memberInfo);
            return new MemberListSnapshot(memberListVersion, newMembers);
        }

        private List<MembershipEvent> DetectMembershipEvents(ICollection<IMember> prevMembers,
            ICollection<IMember> currentMembers)
        {
            var newMembers = new List<IMember>();
            var deadMembers = new HashSet<IMember>(prevMembers);
            foreach (var member in currentMembers)
            {
                if (!deadMembers.Remove(member))
                {
                    newMembers.Add(member);
                }
            }

            var events = new List<MembershipEvent>();

            // removal events should be added before added events
            foreach (var member in deadMembers)
            {
                events.Add(new MembershipEvent(((IHazelcastInstance) _client).Cluster, member, MembershipEvent.MemberRemoved, currentMembers));
                var connection = _connectionManager.GetConnection(member.Uuid);
                connection?.Close(null,
                    new TargetDisconnectedException("The client has closed the connection to this member," +
                                                    " after receiving a member left event from the cluster. " + connection));
            }
            foreach (var member in newMembers)
            {
                events.Add(new MembershipEvent(((IHazelcastInstance) _client).Cluster, member, MembershipEvent.MemberAdded, currentMembers));
            }

            if (events.Count != 0)
            {
                var snapshot = _memberListSnapshot.Get();
                if (snapshot.Members.Values.Count != 0)
                {
                    Logger.Info(snapshot.ToString());
                }
            }
            return events;
        }


        public void HandleMembersViewEvent(int memberListVersion, ICollection<MemberInfo> memberInfos)
        {
            if (Logger.IsFinestEnabled)
            {
                var snapshot = CreateSnapshot(memberListVersion, memberInfos);
                Logger.Finest(
                    $"Handling new snapshot with membership version:  {memberListVersion}  members:  {snapshot}");
            }

            var clusterViewSnapshot = _memberListSnapshot.Get();
            if (clusterViewSnapshot == EmptySnapshot)
            {
                lock (_clusterViewLock)
                {
                    clusterViewSnapshot = _memberListSnapshot.Get();
                    if (clusterViewSnapshot == EmptySnapshot)
                    {
                        //this means this is the first time client connected to cluster
                        ApplyInitialState(memberListVersion, memberInfos);
                        _initialListFetchedLatch.Signal();
                        return;
                    }
                }
            }

            var events = new List<MembershipEvent>();
            if (memberListVersion >= clusterViewSnapshot.Version)
            {
                lock (_clusterViewLock)
                {
                    clusterViewSnapshot = _memberListSnapshot.Get();
                    if (memberListVersion >= clusterViewSnapshot.Version)
                    {
                        ICollection<IMember> prevMembers = clusterViewSnapshot.Members.Values;
                        var snapshot = CreateSnapshot(memberListVersion, memberInfos);
                        _memberListSnapshot.Set(snapshot);
                        ICollection<IMember> currentMembers = snapshot.Members.Values;
                        events = DetectMembershipEvents(prevMembers, currentMembers);
                    }
                }
            }
            FireEvents(events);
        }

        private void FireEvents(List<MembershipEvent> events)
        {
            foreach (var membershipEvent in events)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest($"Fire Event:{membershipEvent}");
                }
                foreach (var listener in _listeners.Values)
                {
                    try
                    {
                        if (membershipEvent.GetEventType() == MembershipEvent.MemberAdded)
                        {
                            listener.MemberAdded(membershipEvent);
                        }
                        else
                        {
                            listener.MemberRemoved(membershipEvent);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Logger.IsFinestEnabled)
                        {
                            Logger.Finest("Exception occured during membership listener callback.", e);
                        }
                    }
                }
            }
        }

        private class MemberListSnapshot
        {
            public MemberListSnapshot(int version, Dictionary<Guid, IMember> members)
            {
                Version = version;
                Members = members;
            }

            public int Version { get; }
            public Dictionary<Guid, IMember> Members { get; }

            public override string ToString()
            {
                ICollection<IMember> members = Members.Values;
                var sb = new StringBuilder("\n\nMembers [");
                sb.Append(members.Count);
                sb.Append("] {");
                foreach (var member in members)
                {
                    sb.Append("\n\t").Append(member);
                }
                sb.Append("\n}\n");
                return sb.ToString();
            }
        }
    }
}