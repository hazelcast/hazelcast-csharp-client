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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the members management services of a cluster.
    /// </summary>
    internal class ClusterMembers : IAsyncDisposable
    {
        private readonly ClusterState _clusterState;
        private readonly ILoadBalancer _loadBalancer;

        private MemberTable _memberTable;
        private Guid _clusterId;

        // flag + semaphore to wait for the first "members view" event
        private volatile int _firstMembersViewed;
        private SemaphoreSlim _firstMembersView = new SemaphoreSlim(0, 1);

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
        public ClusterMembers(ClusterState clusterState)
        {
            _clusterState = clusterState;
            _loadBalancer = clusterState.Options.LoadBalancer.Service ?? new RandomLoadBalancer();
        }

        /// <summary>
        /// Gets this <see cref="ClusterMembers"/> lock object.
        /// </summary>
        public object Mutex { get; } = new object();

        #region Event Handlers

        /// <summary>
        /// (thread-unsafe) Notifies that a connection has been opened.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if the connection is the first one to be established; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method is not thread-safe; the caller has to lock the
        /// <see cref="Mutex"/> object to ensure thread-safety.</para>
        /// </remarks>
        public bool NotifyConnectionOpened(MemberConnection connection)
        {
            var isFirst = _connections.Count == 0;

#if NETSTANDARD2_0
            var contains = _connections.ContainsKey(connection.MemberId);
            _connections[connection.MemberId] = connection;
            if (!contains)
#else
            if (!_connections.TryAdd(connection.MemberId, connection))
#endif
                throw new HazelcastException("Failed to add a connection (duplicate memberId).");

            if (_clusterId == default)
            {
                _clusterId = connection.ClusterId; // first cluster
            }
            else if (_clusterId != connection.ClusterId)
            {
                // see TcpClientConnectionManager java class handleSuccessfulAuth method
                // does not even consider the cluster identifier when !isFirst
                if (isFirst)
                {
                    _clusterId = connection.ClusterId; // new cluster
                    _memberTable = new MemberTable();
                }
            }

            return isFirst;
        }

        /// <summary>
        /// (thread-unsafe) Notifies that a connection has been closed.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if the connection was the last one; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method is not thread-safe; the caller has to lock the
        /// <see cref="Mutex"/> object to ensure thread-safety.</para>
        /// </remarks>
        public bool NotifyConnectionClosed(MemberConnection connection)
        {
            var removed = _connections.ContainsKey(connection.MemberId);
            if (removed) _connections.Remove(connection.MemberId);
            var wasLast = removed && _connections.Count == 0;
            return wasLast;
        }

        /// <summary>
        /// Notifies of a 'members view' event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        public async ValueTask<MembersUpdatedEventArgs> NotifyMembersView(int version, ICollection<MemberInfo> members)
        {
            // FIXME could we process two of these at a time?

            // get a new table
            var table = new MemberTable(version, members);

            // compute changes
            // count 1 for old members, 2 for new members, and then the result is
            // that 1=removed, 2=added, 3=unchanged
            // MemberInfo overrides GetHashCode and can be used as a key here
            var diff = new Dictionary<MemberInfo, int>();
            if (_memberTable == null)
            {
                foreach (var m in table.Members.Values)
                    diff[m] = 2;
            }
            else
            {
                foreach (var m in _memberTable.Members.Values)
                    diff[m] = 1;
                foreach (var m in table.Members.Values)
                    if (diff.ContainsKey(m)) diff[m] += 2;
                    else diff[m] = 2;
            }

            // replace the table
            _memberTable = table;

            // notify the load balancer of the new list of members
            _loadBalancer.NotifyMembers(members.Select(x => x.Id));

            // signal once
            if (Interlocked.CompareExchange(ref _firstMembersViewed, 1, 0) == 0)
                _firstMembersView.Release();

            // process changes, gather events
            var added = new List<MemberInfo>();
            var removed = new List<MemberInfo>();
            foreach (var (member, status) in diff)
            {
                switch (status)
                {
                    case 1: // old but not new = removed
                        HConsole.WriteLine(this, $"Removed member {member.Id}");
                        removed.Add(member);
                        if (_connections.TryGetValue(member.Id, out var client))
                            await client.TerminateAsync().CAF(); // TODO: consider dying in the background?
                        break;

                    case 2: // new but not old = added
                        HConsole.WriteLine(this, $"Added member {member.Id}");
                        added.Add(member);
                        break;

                    case 3: // old and new = no change
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return new MembersUpdatedEventArgs(added, removed, table.Members.Values);
        }

        #endregion

        /// <summary>
        /// Gets the known members addresses.
        /// </summary>
        public IEnumerable<NetworkAddress> GetAddresses()
        {
            var members = _memberTable?.Members;
            return members == null 
                ? Enumerable.Empty<NetworkAddress>() 
                : members.Values.Select(x => x.Address);
        }


        /// <summary>
        /// Gets a connection to a random member.
        /// </summary>
        /// <returns>A random client connection if available; otherwise <c>null</c>.</returns>
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

                // limit the number of tries to the amount of known members
                var count = _loadBalancer.Count;

                for (var i = 0; i < count; i++)
                {
                    var memberId = _loadBalancer.GetMember();
                    lock (Mutex)
                    {
                        if (_connections.TryGetValue(memberId, out connection))
                            return connection;
                    }
                }
            }

            // either "smart" mode but the load balancer did not return a member,
            // or "uni-socket" mode where there should only be once connection
            lock (Mutex) connection = _connections.Values.FirstOrDefault();

            // may be null
            return connection;
        }

        /// <summary>
        /// Tries to get a connection for a member.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if a connection to the specified member was found; otherwise <c>false</c>.</returns>
        public bool TryGetConnection(Guid memberId, out MemberConnection connection)
        {
            lock (Mutex) return _connections.TryGetValue(memberId, out connection);
        }

        /// <summary>
        /// Waits for a random connection to be available and return it.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A random client connection.</returns>
        /// <remarks>
        /// <para>Tries to get a client connection for as long as <paramref name="cancellationToken"/> is not canceled.</para>
        /// </remarks>
        public async ValueTask<MemberConnection> WaitRandomConnection(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // this is just basically retrieving a random client
                var clientConnection = GetRandomConnection();
                if (clientConnection != null) return clientConnection;

                // no need to try again if the client is down
                _clusterState.ThrowIfCancelled(); // FIXME better?!

                // no clients => wait for clients
                // this *may* throw
                await Task.Delay(_clusterState.Options.WaitForConnectionMilliseconds, cancellationToken).CAF();
            }

            // this *will* throw
            cancellationToken.ThrowIfCancellationRequested();
            return default;
        }

        /// <summary>
        /// Gets a snapshot of the current connections.
        /// </summary>
        /// <param name="activeOnly">Whether to return active connections only.</param>
        /// <returns>A snapshot of the current client connections.</returns>
        public List<MemberConnection> SnapshotConnections(bool activeOnly)
        {
            IEnumerable<MemberConnection> connections;
            lock (Mutex) connections = _connections.Values;
            if (activeOnly) connections = connections.Where(x => x.Active);
            return connections.ToList();
        }

        /// <summary>
        /// Gets a snapshot of the current members.
        /// </summary>
        /// <returns>A snapshot of the current members.</returns>
        public IEnumerable<MemberInfo> SnapshotMembers()
        {
            var memberTable = _memberTable;
            return memberTable?.Members.Values ?? Enumerable.Empty<MemberInfo>();
        }

        /// <summary>
        /// Gets information about a member.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <returns>Information about the specified member, or null if no member with the specified identifier was found.</returns>
        public MemberInfo GetMember(Guid memberId)
        {
            return _memberTable.Members.TryGetValue(memberId, out var memberInfo) ? memberInfo : null;
        }

        /// <summary>
        /// Gets the lite members.
        /// </summary>
        public IEnumerable<MemberInfo> LiteMembers => _memberTable.Members.Values.Where(x => x.IsLiteMember);

        /// <summary>
        /// Waits for the first member view event.
        /// </summary>
        /// <returns>A task that will complete when the first member view event has been received.</returns>
        public async ValueTask WaitForMembersAsync(CancellationToken cancellationToken)
        {
            await _firstMembersView.WaitAsync(cancellationToken).CAF();
            _firstMembersView.Dispose();
            _firstMembersView = null;
        }


        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _firstMembersView?.Dispose();
        }
    }
}
