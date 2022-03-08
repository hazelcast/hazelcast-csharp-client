﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the members management services of a cluster.
    /// </summary>
    internal class ClusterMembers : IAsyncDisposable
    {
        private const int SqlConnectionRandomAttempts = 10;

        private readonly object _mutex = new object();
        private readonly ClusterState _clusterState;
        private readonly ILogger _logger;
        private readonly ILoadBalancer _loadBalancer;

        private readonly TerminateConnections _terminateConnections;
        private readonly MemberConnectionQueue _memberConnectionQueue;

        private MemberTable _members;
        private bool _connected;
        private bool _usePublicAddresses;

        // flag + semaphore to wait for the first "partitions view" event
        //private volatile int _firstPartitionsViewed;
        //private SemaphoreSlim _firstPartitionsView = new SemaphoreSlim(0, 1);

        // member id -> connection
        // not concurrent, always managed through the mutex
        private readonly Dictionary<Guid, MemberConnection> _connections = new Dictionary<Guid, MemberConnection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterMembers"/> class.
        /// </summary>
        /// <param name="clusterState">The cluster state.</param>
        /// <param name="terminateConnections">The terminate connections task.</param>
        public ClusterMembers(ClusterState clusterState, TerminateConnections terminateConnections)
        {
            HConsole.Configure(x => x.Configure<ClusterMembers>().SetPrefix("CLUST.MBRS"));

            _clusterState = clusterState;
            _terminateConnections = terminateConnections;
            _loadBalancer = clusterState.Options.LoadBalancer.Service ?? new RandomLoadBalancer();

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterMembers>();

            _members = new MemberTable();

            // members to connect
            if (clusterState.IsSmartRouting)
            {
                // initialize the queue of members to connect
                // and the handler to re-queue members that have failed, *if* they are still members
                _memberConnectionQueue = new MemberConnectionQueue(clusterState.LoggerFactory);
                _memberConnectionQueue.ConnectionFailed += (_, member) =>
                {
                    lock (_mutex)
                    {
                        if (_members.ContainsMember(member.Id))
                            _memberConnectionQueue.Add(member);
                    }
                };
            }
        }

        // NOTES
        // we cannot have two connections to the same member ID at the same time, but a member IP may change,
        // so having a connection to a member ID does not mean that the member is connected, and we may have
        // to switch a member's connection over to a new IP

        // determines whether a member is connected.
        private bool IsMemberConnected(MemberInfo member)
        {
            // a member is connected when it is registered, and has a connection
            return _members.ContainsMember(member.Id) &&
                   HasConnectionForMember(member);
        }

        // determines whether at least one member is connected.
        private bool IsAnyMemberConnected()
        {
            lock (_mutex) return _members.Members.Any(HasConnectionForMemberLocked);
        }

        private bool HasConnectionForMemberLocked(MemberInfo member)
            => _connections.TryGetValue(member.Id, out var connection) &&
               connection.Address == member.ConnectAddress;

        // determines whether we have a connection for a member
        private bool HasConnectionForMember(MemberInfo member)
        {
            lock (_mutex) return HasConnectionForMemberLocked(member);
        }

        // registers a connection for termination
        public void TerminateConnection(MemberConnection connection)
            => _terminateConnections.Add(connection);

        #region Event Handlers

        /// <summary>
        /// Adds a connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="isNewCluster">Whether the connection is the first connection to a new cluster.</param>
        public void AddConnection(MemberConnection connection, bool isNewCluster)
        {
            // accept every connection, regardless of whether there is a known corresponding member,
            // since the first connection is going to come before we get the first members view.

            lock (_mutex)
            {
                // don't add the connection if it is not active - if it *is* active, it still
                // could turn not-active anytime, but thanks to _mutex that will only happen
                // after the connection has been added
                if (!connection.Active) return;

                var contains = _connections.TryGetValue(connection.MemberId, out var existingConnection);

                if (contains)
                {
                    if (existingConnection.Address != connection.Address)
                    {
                        _terminateConnections.Add(existingConnection);
                    }
                    else
                    {
                        // we cannot accept this connection, it's a duplicate (internal error?)
                        _logger.LogWarning($"Cannot add connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()} at {connection.Address}, a connection to that member at that address already exists.");
                        _terminateConnections.Add(connection); // kill.kill.kill
                        return;
                    }
                }

                // add the connection
                _connections[connection.MemberId] = connection;

                if (isNewCluster)
                {
                    // reset members
                    // this is safe because... isNewCluster means that this is the very first connection and there are
                    // no other connections yet and therefore we should not receive events and therefore no one
                    // should invoke SetMembers.
                    _members = new MemberTable();
                }

                // if this is a true member connection
                if (_members.TryGetMember(connection.MemberId, out var member) && member.ConnectAddress == connection.Address)
                {
                    // if this is the first connection to an actual member, change state & trigger event
                    if (!_connected)
                    {
                        // change Started | Disconnected -> Connected, ignore otherwise, it could be ShuttingDown or Shutdown
                        _logger.LogDebug($"Added connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()} at {connection.Address}, now connected.");
                        _clusterState.ChangeState(ClientState.Connected, ClientState.Started, ClientState.Disconnected);
                        _connected = true;
                    }
                    else if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        var msg = $"Added connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()} at {connection.Address}";
                        msg += existingConnection == null
                            ? "."
                            : $", replacing connection {existingConnection.Id.ToShortString()} at {existingConnection.Address}.";
                        _logger.LogDebug(msg);
                    }
                }
                else
                {
                    _logger.IfDebug()?.LogDebug($"Added orphan connection {connection.Id.ToShortString()} at {connection.Address} (member {connection.MemberId.ToShortString()}).");
                }
            }
        }

        /// <summary>
        /// Removes a connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public async Task RemoveConnectionAsync(MemberConnection connection)
        {
            lock (_mutex)
            {
                // ignore unknown connections that were not added in the first place,
                // or that have been replaced with another connection to the same member
                if (!_connections.TryGetValue(connection.MemberId, out var c) || connection.Id != c.Id)
                {
                    _logger.IfDebug()?.LogDebug($"Removed orphan connection {connection.Id.ToShortString()}.");
                    return;
                }

                // remove the connection and check whether we are potentially disconnecting
                // ie whether we were connected, and either we don't have connections any more, or no member
                // is connected (has a matching connection)
                _connections.Remove(connection.MemberId);
                var disconnecting = _connected && (_connections.Count == 0 || !IsAnyMemberConnected());

                // if we are not disconnecting, we can return - we are done
                if (!disconnecting)
                {
                    _logger.LogDebug($"Removed connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()}, remain {(_connected?"":"dis")}connected.");

                    // if we are connected,
                    // and the disconnected member is still a member, queue it for reconnection
                    if (_connected && _members.TryGetMember(connection.MemberId, out var member))
                        _memberConnectionQueue?.Add(member);
                    return;
                }
            }

            // otherwise, we might be disconnecting

            // but, the connection queue was running and might have added a new connection
            // we *need* a stable state in order to figure out whether we are disconnecting or not,
            // and if we are, we *need* to drain the queue (stop connecting more members) - and
            // the only way to achieve this is to suspend the queue
            if (_memberConnectionQueue != null) await _memberConnectionQueue.SuspendAsync().CfAwait();

            // note: multiple connections can close an once = multiple calls can reach this point

            var drain = false;
            try
            {
                lock (_mutex) // but we deal with calls one by one
                {
                    if (_connected) // and only disconnect once
                    {
                        // if we have connections, and at least one member is connected (has a matching connection),
                        // then the queue has added a new connection indeed and we are finally not disconnecting - we
                        // can return - we are done
                        if (_connections.Count > 0 && _members.Members.Any(x => _connections.ContainsKey(x.Id)))
                        {
                            // if the disconnected member is still a member, queue it for reconnection
                            if (_members.TryGetMember(connection.MemberId, out var member))
                                _memberConnectionQueue?.Add(member);
                            _logger.LogDebug($"Removed connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()}, remain connected.");
                            return;
                        }

                        // otherwise, we're really disconnecting: flip _connected, and change the state
                        _connected = false;
                        _logger.LogDebug($"Removed connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()}, disconnecting.");
                        _clusterState.ChangeState(ClientState.Disconnected, ClientState.Connected);

                        // and drain the queue: stop connecting members, we need to fully reconnect
                        drain = true;
                    }
                    else
                    {
                        _logger.LogDebug($"Removed connection {connection.Id.ToShortString()} to member {connection.MemberId.ToShortString()}, already disconnected (?).");
                    }
                }
            }
            finally
            {
                // don't forget to resume the queue
                _memberConnectionQueue?.Resume(drain);
            }
        }

        private void LogDiffs(MemberTable table, Dictionary<MemberInfo, int> diff)
        {
            var msg = new StringBuilder();
            msg.Append("Members [");
            msg.Append(table.Count);
            msg.AppendLine("] {");
            foreach (var (m, d) in diff)
            {
                msg.Append("    ");
                msg.Append(m.ToShortString(true));
                var status = d switch
                {
                    1 => "Removing",
                    2 => "Adding",
                    3 => "Unchanged",
                    _ => ""
                };
                msg.Append(' ');
                msg.Append(status);
                msg.AppendLine();
            }
            msg.Append('}');

            _logger.LogInformation(msg.ToString());
        }

        /// <summary>
        /// Set the members.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        /// <returns>The corresponding event arguments, if members were updated; otherwise <c>null</c>.</returns>
        public async Task<MembersUpdatedEventArgs> SetMembersAsync(int version, ICollection<MemberInfo> members)
        {
            // skip old sets
            if (version < _members.Version)
                return null;

            // note: members are compared by member.Id and member.ConnectAddress
            // as that is what makes a difference, really - the actual Address and
            // PublicAddress don't matter much for what we do

            // replace the table
            var previous = _members;
            var table = new MemberTable(version, members);
            lock (_mutex) _members = table;

            // notify the load balancer of the new list of members
            // (the load balancer can always return a member that is not a member
            // anymore, see note in GetMember)
            _loadBalancer.SetMembers(members.Select(x => x.Id));

            // members provided through the members view event always provide their own internal address,
            // and may (e.g. for Kubernetes clusters) also provide their public address - but in a Cloud
            // setup for instance, members are not aware of their public address. so, we try to "fix"
            // the members using the AddressProvider map, if any.
            if (_clusterState.AddressProvider.HasMap)
            {
                foreach (var member in members.Where(x => !x.HasPublicAddress))
                    member.PublicAddress = _clusterState.AddressProvider.Map(member.Address);
            }

            // and then we need to determine whether to connect to members through their internal address
            // or their public address - this is the role of the ConnectAddressResolver - it can be forced
            // through configuration options, or determined through rules:
            // - if one member responds on its internal address, assume internal addresses are OK
            // - if enough (sample size) members respond only on their public address, use public addresses
            // for performance reasons (and this is what the Java client does) we determine this
            // once when getting the first members view, and don't change our mind later on, ever.
            if (previous.Count == 0) // first members view
            {
                var resolver = new ConnectAddressResolver(_clusterState.Options.Networking, _clusterState.LoggerFactory);
                if (!(members is IReadOnlyCollection<MemberInfo> mro)) throw new HazelcastException("panic"); // TODO: not exactly pretty
                _usePublicAddresses = await resolver.DetermineUsePublicAddresses(mro).CfAwaitNoThrow(false);
            }

            // update members
            foreach (var member in members) member.UsePublicAddress = _usePublicAddresses;

            // compute changes
            // count 1 for old members, 2 for new members, and then the result is
            // 1=removed, 2=added, 3=unchanged
            // MemberInfo overrides GetHashCode and can be used as a key here
            var diff = new Dictionary<MemberInfo, int>();
            if (previous == null)
            {
                foreach (var m in members)
                    diff[m] = 2;
            }
            else
            {
                foreach (var m in previous.Members)
                    diff[m] = 1;

                foreach (var m in members)
                    if (diff.ContainsKey(m)) diff[m] += 2;
                    else diff[m] = 2;
            }

            // log
            if (_logger.IsEnabled(LogLevel.Information))
                LogDiffs(table, diff);

            // process changes, gather events
            var added = new List<MemberInfo>();
            var removed = new List<MemberInfo>();
            foreach (var (member, status) in diff) // all members, old and new
            {
                switch (status)
                {
                    case 1: // old but not new = removed
                        HConsole.WriteLine(this, $"Removed {member}");
                        removed.Add(member);

                        // dequeue the member
                        _memberConnectionQueue?.Remove(member.Id);

                        break;

                    case 2: // new but not old = added
                        HConsole.WriteLine(this, $"Added {member}");
                        added.Add(member);

                        // queue the member for connection
                        _memberConnectionQueue?.Add(member);

                        break;

                    case 3: // old and new = no change
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            var maybeDisconnected = false;
            lock (_mutex)
            {
                // removed members need to have their connection removed and terminated
                foreach (var member in removed)
                {
                    if (_connections.TryGetValue(member.Id, out var c))
                    {
                        _logger.LogDebug($"Set members: remove obsolete connection {c.Id.ToShortString()} to {c.MemberId.ToShortString()} at {c.Address}.");
                        _connections.Remove(member.Id);
                        _terminateConnections.Add(c);
                    }
                }

                // remove connections that don't match a member
                var d = members.ToDictionary(x => x.Id, x => x);
                List<MemberConnection> toRemove = null;
                foreach (var c in _connections.Values)
                {
                    if (!d.TryGetValue(c.MemberId, out var m) || m.ConnectAddress != c.Address)
                        (toRemove ??= new List<MemberConnection>()).Add(c);
                }

                if (toRemove != null)
                {
                    foreach (var c in toRemove)
                    {
                        _connections.Remove(c.Id);
                        _logger.LogDebug($"Set members: remove orphaned connection {c.Id.ToShortString()} to {c.MemberId.ToShortString()} at {c.Address}.");
                        _terminateConnections.Add(c);
                    }
                }

                var isAnyMemberConnected = IsAnyMemberConnected();

                if (!_connected)
                {
                    if (isAnyMemberConnected)
                    {
                        // if we were not connected and now one member happens to be connected then we are now connected
                        // we hold the mutex so nothing bad can happen
                        _logger.LogDebug($"Set members: {removed.Count} removed, {added.Count} added, {members.Count} total and at least one is connected, now connected.");
                        _clusterState.ChangeState(ClientState.Connected, ClientState.Started, ClientState.Disconnected);
                        _connected = true;
                    }
                    else
                    {
                        // remain disconnected
                        _logger.LogDebug($"Set members: {removed.Count} removed, {added.Count} added, {members.Count} total and none is connected, remain disconnected.");
                    }
                }
                else
                {
                    if (isAnyMemberConnected)
                    {
                        // remain connected
                        _logger.LogDebug($"Set members: {removed.Count} removed, {added.Count} added, {members.Count} total and at least one is connected, remain connected.");
                    }
                    else
                    {
                        // we probably are disconnected now
                        // but the connection queue is running and might have re-added a member
                        maybeDisconnected = true;
                    }
                }
            }

            // if we cannot be disconnected, we can return immediately
            if (!maybeDisconnected) return new MembersUpdatedEventArgs(added, removed, members.ToList());

            // else, suspend the queue - we need stable connections before we can make a decision
            if (_memberConnectionQueue != null) await _memberConnectionQueue.SuspendAsync().CfAwait();

            var disconnected = false;
            try
            {
                lock (_mutex)
                {
                    var isAnyMemberConnected = IsAnyMemberConnected();
                    if (!isAnyMemberConnected)
                    {
                        // no more connected member, we are now disconnected
                        _logger.LogDebug($"Set members: {removed.Count} removed, {added.Count} added, {members.Count} total and none connected, disconnecting.");
                        _clusterState.ChangeState(ClientState.Disconnected, ClientState.Connected);
                        _connected = false;
                        disconnected = true;
                    }
                    else
                    {
                        _logger.LogDebug($"Set members: {removed.Count} removed, {added.Count} added, {members.Count} total and at least one is connected, remain connected.");
                    }
                }
            }
            finally
            {
                // if we are now disconnected, make sure to drain the queue
                _memberConnectionQueue?.Resume(drain: disconnected);
            }

            return new MembersUpdatedEventArgs(added, removed, members.ToList());
        }

        #endregion


        /// <summary>
        /// Enumerates the members to connect.
        /// </summary>
        public IAsyncEnumerable<MemberConnectionRequest> MemberConnectionRequests
            => _memberConnectionQueue;

        /// <summary>
        /// Gets a connection to a random member.
        /// </summary>
        /// <returns>A random client connection if available; otherwise <c>null</c>.</returns>
        /// <para>The connection should be active, but there is no guarantee it will not become immediately inactive.</para>
        public MemberConnection GetRandomConnection()
        {
            MemberConnection connection;

            // In "smart routing" mode the clients connect to each member of the cluster. Since each
            // data partition uses the well known and consistent hashing algorithm, each client
            // can send an operation to the relevant cluster member, which increases the
            // overall throughput and efficiency. Smart mode is the default mode.
            //
            // In "uni-socket" mode the clients is required to connect to a single member, which
            // then behaves as a gateway for the other members. Firewalls, security, or some
            // custom networking issues can be the reason for these cases.

            if (_clusterState.IsSmartRouting)
            {
                // "smart" mode

                // limit the number of tries to the amount of known members, but
                // it is ok to try more than once, order to return a connection
                // that has a reasonable chance of being usable
                var count = _loadBalancer.Count;

                for (var i = 0; i < count; i++)
                {
                    var memberId = _loadBalancer.GetMember();

                    // if the load balancer does not have members, break
                    if (memberId == Guid.Empty)
                        break;

                    // we cannot guarantee that the connection we'll return will not correspond to
                    // a member... that is not a member by the time it is used... but at least we
                    // can make sure it *still* is a member now
                    if (!_members.ContainsMember(memberId))
                        continue;

                    lock (_mutex)
                    {
                        if (_connections.TryGetValue(memberId, out connection))
                            return connection;
                    }
                }
            }

            // either "smart" mode but the load balancer did not return a member,
            // or "uni-socket" mode where there should only be once connection
            lock (_mutex) connection = _connections.Values.FirstOrDefault();

            // may be null
            return connection;
        }

        /// <summary>
        /// Gets connection to execute SQL queries/statements.
        /// </summary>
        public MemberConnection GetConnectionForSql()
        {
            if (_clusterState.IsSmartRouting)
            {
                // There might be a race - the chosen member might be just connected or disconnected - try a
                // couple of times, the memberOfLargerSameVersionGroup returns a random connection,
                // we might be lucky...
                for (var i = 0; i < SqlConnectionRandomAttempts; i++)
                {
                    var member = GetMemberForSql();
                    if (member == null) break;

                    if (TryGetConnection(member.Id, out var memberConnection))
                        return memberConnection;
                }
            }

            // Otherwise iterate over connections and return the first one that's not to a lite member
            MemberConnection firstConnection = null;

            lock (_mutex)
            {
                foreach (var (memberId, connection) in _connections)
                {
                    firstConnection ??= connection;

                    if (_members.TryGetMember(memberId, out var member) && !member.IsLiteMember)
                        return connection;
                }
            }

            // Failed to get a connection to a data member, return first lite member instead
            // Lite members support DDL but note DML statements
            // https://docs.hazelcast.com/hazelcast/5.0-SNAPSHOT/sql/sql-statements.html
            return firstConnection;
        }

        /// <summary>
        /// Finds a larger same-version group of data members from a collection of members.
        /// Otherwise returns a random member from the group. If the same-version
        /// groups have the same size, returns a member from the newer group.
        /// </summary>
        /// <returns><see cref="MemberInfo"/> if one is found or <c>null</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">If there are more than 2 distinct member versions found.</exception>
        public MemberInfo GetMemberForSql()
        {
            (MemberVersion version0, MemberVersion version1) = (null, null);
            var (count0, count1) = (0, 0);

            foreach (var member in _members.Members)
            {
                if (member.IsLiteMember)
                    continue;

                var memberVersion = member.Version;

                if (version0 == null || version0.Equals(memberVersion, ignorePatchVersion: true))
                {
                    version0 = memberVersion;
                    count0++;
                }
                else if (version1 == null || version1.Equals(memberVersion, ignorePatchVersion: true))
                {
                    version1 = memberVersion;
                    count1++;
                }
                else
                {
                    var strVersion0 = version0.ToString(ignorePatchVersion: true);
                    var strVersion1 = version1.ToString(ignorePatchVersion: true);
                    var strVersion = memberVersion.ToString(ignorePatchVersion: true);

                    throw new InvalidOperationException(
                        $"More than 2 distinct member versions found: {strVersion0}, {strVersion1}, {strVersion}"
                    );
                }
            }

            // no data members
            if (count0 == 0)
                return null;

            int count;
            MemberVersion version;

            if (count0 > count1 || (count0 == count1 && version0 > version1))
                (count, version) = (count0, version0);
            else
                (count, version) = (count1, version1);

            // otherwise return a random member from the larger group
            var randomIndex = RandomProvider.Next(count);
            foreach (var member in _members.Members)
            {
                if (!member.IsLiteMember && member.Version.Equals(version, ignorePatchVersion: true))
                {
                    randomIndex--;
                    if (randomIndex < 0)
                        return member;
                }
            }

            // should never get here
            throw new HazelcastException($"Reached unexpected state in {nameof(GetMemberForSql)}.");
        }

        /// <summary>
        /// Gets the oldest active connection.
        /// </summary>
        /// <returns>The oldest active connection, or <c>null</c> if no connection is active.</returns>
        public MemberConnection GetOldestConnection()
        {
            lock (_mutex) return _connections.Values
                .Where(x => x.Active)
                .OrderBy(x => x.ConnectTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Tries to get a connection for a member.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if a connection to the specified member was found; otherwise <c>false</c>.</returns>
        /// <para>The connection should be active, but there is no guarantee it will not become immediately inactive.</para>
        public bool TryGetConnection(Guid memberId, out MemberConnection connection)
        {
            lock (_mutex) return _connections.TryGetValue(memberId, out connection);
        }

        /// <summary>
        /// Gets information about each member.
        /// </summary>
        /// <param name="liteOnly">Whether to only return lite members.</param>
        /// <returns>The current members.</returns>
        public IEnumerable<MemberInfoState> GetMembersAndState(bool liteOnly = false)
        {
            IEnumerable<MemberInfo> members = _members.Members;
            if (liteOnly) members = members.Where(x => x.IsLiteMember);

            lock (_mutex) return members.Select(x => new MemberInfoState(x, HasConnectionForMemberLocked(x))).ToList();
        }

        /// <summary>
        /// Gets information about each member.
        /// </summary>
        /// <param name="liteOnly">Whether to only return lite members.</param>
        /// <returns>The current members.</returns>
        public IEnumerable<MemberInfo> GetMembers(bool liteOnly = false)
        {
            IEnumerable<MemberInfo> members = _members.Members;
            return liteOnly ? members.Where(x => x.IsLiteMember).ToList() : members;
        }

        /// <summary>
        /// Gets information about a member.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <returns>Information about the specified member, or <c>null</c> if no member with the specified identifier was found.</returns>
        public MemberInfo GetMember(Guid memberId)
        {
            return _members.TryGetMember(memberId, out var memberInfo)
                ? memberInfo
                : null;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _memberConnectionQueue.DisposeAsync().CfAwait();
        }
    }
}
