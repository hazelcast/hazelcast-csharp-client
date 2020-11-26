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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the members management services of a cluster.
    /// </summary>
    internal class ClusterMembers : IAsyncDisposable
    {
        private readonly ClusterState _clusterState;
        private readonly AddressProvider _addressProvider;

        private MemberTable _memberTable;
        private volatile int _firstMembersViewed;
        private SemaphoreSlim _firstMembersView = new SemaphoreSlim(0, 1);

        //private volatile int _firstPartitionsViewed;
        //private SemaphoreSlim _firstPartitionsView = new SemaphoreSlim(0, 1);

        // member id -> client
        // the master clients list
        private readonly ConcurrentDictionary<Guid, MemberConnection> _connections = new ConcurrentDictionary<Guid, MemberConnection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterMembers"/> class.
        /// </summary>
        /// <param name="clusterState">The cluster state.</param>
        public ClusterMembers(ClusterState clusterState)
        {
            _clusterState = clusterState;
            _addressProvider = new AddressProvider(clusterState.Options.Networking, clusterState.LoggerFactory);
        }



        /// <summary>
        /// Notifies the members service of a new connection.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if the connection is the first one to be established; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method should be invoked within the global cluster lock.</para>
        /// </remarks>
        public bool NotifyNewConnection(Guid memberId, MemberConnection connection)
        {
            var isFirst = _connections.IsEmpty;

            if (_connections.ContainsKey(memberId))
                throw new HazelcastException("Duplicate client.");

            _connections[memberId] = connection;

            return isFirst;
        }

        /// <summary>
        /// Notifies the member service of a terminated connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if the connection was the last one; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method should be invoked within the global cluster lock.</para>
        /// </remarks>
        public bool NotifyTerminatedConnection(MemberConnection connection)
        {
            _connections.TryRemove(connection.MemberId, out _);

            var wasLast = _connections.IsEmpty;
            return wasLast;
        }


        /// <summary>
        /// Notifies the member service of a new cluster.
        /// </summary>
        /// <remarks>
        /// <para>This method should be invoked within the global cluster lock.</para>
        /// </remarks>
        public void NotifyNewCluster()
        {
            // the table should be empty anyways - a new cluster means we just established
            // the very first connection to a different cluster - so there were no members left
            _memberTable = new MemberTable(0, Array.Empty<MemberInfo>());
        }



        /// <summary>
        /// Gets a connection to a random member.
        /// </summary>
        /// <param name="throwIfNoConnection">Whether to throw if no client connection can be obtained immediately.</param>
        /// <returns>A random client connection.</returns>
        /// <remarks>
        /// <para>Throws if not client connection can be obtained immediately.</para>
        /// </remarks>
        public MemberConnection GetRandomConnection(bool throwIfNoConnection = true)
        {
            // In "smart mode" the clients connect to each member of the cluster. Since each
            // data partition uses the well known and consistent hashing algorithm, each client
            // can send an operation to the relevant cluster member, which increases the
            // overall throughput and efficiency. Smart mode is the default mode.
            //
            // In "uni-socket mode" the clients is required to connect to a single member, which
            // then behaves as a gateway for the other members. Firewalls, security, or some
            // custom networking issues can be the reason for these cases.

            MemberConnection connection;

            var maxTries = _clusterState.LoadBalancer.Count;

            if (_clusterState.IsSmartRouting)
            {
                for (var i = 0; i < maxTries; i++)
                {
                    var memberId = _clusterState.LoadBalancer.GetMember();
                    if (_connections.TryGetValue(memberId, out connection))
                        return connection;
                }

                connection = _connections.Values.FirstOrDefault();
                if (connection == null && throwIfNoConnection)
                    throw new HazelcastException("Could not get a connection.");

                return connection;
            }

            // there should be only one
            connection = _connections.Values.FirstOrDefault();
            if (connection == null && throwIfNoConnection)
                throw new HazelcastException("Could not get a connection.");
            return connection;
        }

        /// <summary>
        /// Tries to get a connection for a member.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if a connection to the specified member was found; otherwise <c>false</c>.</returns>
        public bool TryGetConnection(Guid memberId, out MemberConnection connection)
            => _connections.TryGetValue(memberId, out connection);

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
                var clientConnection = GetRandomConnection(false);
                if (clientConnection != null) return clientConnection;

                // no need to try again if the client died
                _clusterState.ThrowIfCancelled();

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
            IEnumerable<MemberConnection> connections = _connections.Values;
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
        /// Terminate all member connections.
        /// </summary>
        /// <returns>A task that will complete when all member connections have been terminated.</returns>
        public async ValueTask TerminateAll()
        {
            foreach (var (_, connection) in _connections)
            {
                await connection.TerminateAsync().CAF();
            }
        }



        /// <summary>
        /// Gets all candidate network addresses for connecting to the cluster.
        /// </summary>
        /// <returns>All candidate network addresses.</returns>
        /// <remarks>
        /// <para>Returns unique addresses (unique by their IPEndPoint).</para>
        /// </remarks>
        public IEnumerable<NetworkAddress> GetCandidateAddresses()
        {
            var addresses = new HashSet<NetworkAddress>();

            // do it in two batches,
            // each batch may be shuffled, but know members always come first

            // first, add all known members
            var members = _memberTable?.Members;
            if (members != null)
            {
                var memberAddresses = _memberTable.Members.Values.Select(x => x.Address);
                if (_clusterState.Options.Networking.ShuffleAddresses)
                    memberAddresses = memberAddresses.Shuffle();

                foreach (var address in memberAddresses)
                    addresses.Add(address);
            }

            // second, add all known addresses (de-duplicated thanks to HashSet)
            var configuredAddresses = _addressProvider.GetAddresses();
            if (_clusterState.Options.Networking.ShuffleAddresses)
                configuredAddresses = configuredAddresses.Shuffle();

            foreach (var address in configuredAddresses)
                addresses.Add(address);

            return addresses;
        }

        /// <summary>
        /// Maps a network private address to a public address.
        /// </summary>
        /// <param name="address">The address to map.</param>
        /// <returns>The corresponding public address.</returns>
        public NetworkAddress MapAddress(NetworkAddress address)
            => _addressProvider.Map(address);



        /// <summary>
        /// Handles the 'members view' event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        public async ValueTask<List<(MemberLifecycleEventType, MemberLifecycleEventArgs)>> HandleMemberViewEvent(int version, ICollection<MemberInfo> members)
        {
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
            _clusterState.LoadBalancer.NotifyMembers(members.Select(x => x.Id));

            // signal once
            if (Interlocked.CompareExchange(ref _firstMembersViewed, 1, 0) == 0)
                _firstMembersView.Release();

            // process changes, gather events
            var eventArgs = new List<(MemberLifecycleEventType, MemberLifecycleEventArgs)>();
            foreach (var (member, status) in diff)
            {
                switch (status)
                {
                    case 1: // old but not new = removed
                        HConsole.WriteLine(this, $"Removed member {member.Id}");
                        eventArgs.Add((MemberLifecycleEventType.Removed, new MemberLifecycleEventArgs(member)));
                        if (_connections.TryGetValue(member.Id, out var client))
                            await client.TerminateAsync().CAF(); // TODO: consider dying in the background?
                        break;

                    case 2: // new but not old = added
                        HConsole.WriteLine(this, $"Added member {member.Id}");
                        eventArgs.Add((MemberLifecycleEventType.Added, new MemberLifecycleEventArgs(member)));
                        break;

                    case 3: // old and new = no change
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return eventArgs;
        }

        /// <summary>
        /// Waits for the first member view event.
        /// </summary>
        /// <returns>A task that will complete when the first member view event has been received.</returns>
        public async ValueTask WaitForFirstMemberViewEventAsync(CancellationToken cancellationToken)
        {
            await _firstMembersView.WaitAsync(cancellationToken).CAF();
            _firstMembersView.Dispose();
            _firstMembersView = null;
        }


        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await TerminateAll().CAF();
            _firstMembersView?.Dispose();
        }
    }
}
