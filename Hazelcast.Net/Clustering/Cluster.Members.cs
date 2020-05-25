using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    // partial: members
    public partial class Cluster
    {
        /// <summary>
        /// Clears a client currently handling cluster events.
        /// </summary>
        /// <param name="client">A client.</param>
        /// <returns>true if the current client matched the specified client, and was cleared; otherwise false.</returns>
        private bool ClearClusterEventsClientWithLock(Client client)
        {
            // if the specified client is *not* the cluster events client, ignore
            if (_clusterEventsClient != client)
                return false;

            // otherwise, clear the cluster event client
            _clusterEventsClient = null;
            _correlatedSubscriptions.TryRemove(_clusterEventsCorrelationId, out _);
            _clusterEventsCorrelationId = 0;
            return true;
        }

        /// <summary>
        /// Starts the task that ensures that a client handlers cluster events, if it is not already running.
        /// </summary>
        /// <param name="client">An optional candidate client.</param>
        private void StartSetClusterEventsClientWithLock(Client client = null)
        {
            // there can only be one instance of that task running at a time
            // and it runs in the background, and at any time any client could
            // shutdown, which might clear the current cluster event client
            //
            // the task self-removes itself when it ends

            _clusterEventsTask ??= SetClusterEventsClientAsync(client);
        }

        /// <summary>
        /// Sets a client to handle cluster events.
        /// </summary>
        /// <param name="client">An optional candidate client.</param>
        /// <returns>A task that will complete when a new client has been assigned to handle cluster events.</returns>
        private async Task SetClusterEventsClientAsync(Client client = null)
        {
            // this will only exit once a client is assigned, or the task is
            // cancelled, when the cluster goes down (and never up again)
            while (!_clusterEventsCancel)
            {
                client ??= GetRandomClient();

                if (client == null)
                {
                    // no clients => wait for clients
                    await Task.Delay(Constants.Cluster.WaitForClientMilliseconds);
                    continue;
                }

                // try to subscribe, relying on the default invocation timeout,
                // so this is not going to last forever - we know it will end
                var correlationId = _correlationIdSequence.Next;
                if (!await SubscribeToClusterEventsAsync(client, correlationId)) // does not throw
                {
                    // failed => try another client
                    client = null;
                    continue;
                }

                // success!
                lock (_clusterEventsLock)
                {
                    _clusterEventsClient = client;
                    _clusterEventsCorrelationId = correlationId;

                    // avoid race conditions, this task is going to end, and if the
                    // client dies we want to be sure we restart the task
                    _clusterEventsTask = null;
                }

                break;
            }
        }

        /// <summary>
        /// Subscribes a client to cluster events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>A task that will complete when the subscription has been processed, and represent whether it was successful.</returns>
        private async Task<bool> SubscribeToClusterEventsAsync(Client client, long correlationId)
        {
            // aka subscribe to member/partition view events
            XConsole.WriteLine(this, "subscribe");

            // handles the event
            void HandleEvent(ClientMessage message, object state)
                => ClientAddClusterViewListenerCodec.HandleEvent(message,
                    HandleMemberViewEvent,
                    (version, partitions) => HandlePartitionViewEvent(client.Id, version, partitions),
                    _loggerFactory);

            try
            {
                var subscribeRequest = ClientAddClusterViewListenerCodec.EncodeRequest();
                _correlatedSubscriptions[correlationId] = new ClusterSubscription(HandleEvent);
                _ = await client.SendAsync(subscribeRequest, correlationId, CancellationToken.None); // FIXME TOKEN
                XConsole.WriteLine(this, "subscribed");
                return true;
            }
            catch (Exception e)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                _logger.LogWarning(e, "Failed to subscribe to cluster events, may retry.");
                return false;
            }
        }

        /// <summary>
        /// Maps partitions from the event representation to our internal representation.
        /// </summary>
        /// <param name="partitions">The event representation of partitions.</param>
        /// <returns>Our internal representation of partitions.</returns>
        private static Dictionary<int, Guid> MapPartitions(IEnumerable<KeyValuePair<Guid, IList<int>>> partitions)
        {
            var map = new Dictionary<int, Guid>();
            foreach (var (memberId, partitionIds) in partitions)
                foreach (var partitionId in partitionIds)
                    map[partitionId] = memberId;
            return map;
        }

        /// <summary>
        /// Handles the 'partitions view' event.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client.</param>
        /// <param name="version">The version.</param>
        /// <param name="partitions">The partitions.</param>
        private void HandlePartitionViewEvent(Guid clientId, int version, IEnumerable<KeyValuePair<Guid, IList<int>>> partitions)
        {
            Partitioner.NotifyPartitionView(clientId, version, MapPartitions(partitions));

            // signal once
            //if (Interlocked.CompareExchange(ref _firstPartitionsViewed, 1, 0) == 0)
            //    _firstPartitionsView.Release();

            OnPartitionsUpdated();
        }

        /// <summary>
        /// Handles the 'members view' event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        private void HandleMemberViewEvent(int version, ICollection<MemberInfo> members)
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
            _loadBalancer.NotifyMembers(members.Select(x => x.Id));

            // signal once
            if (Interlocked.CompareExchange(ref _firstMembersViewed, 1, 0) == 0)
                _firstMembersView.Release();

            // process changes, gather events
            var eventArgs = new List<(ClusterMemberLifecycleEventType, ClusterMemberLifecycleEventArgs)>();
            foreach (var (member, status) in diff)
            {
                switch (status)
                {
                    case 1: // old but not new = removed
                        XConsole.WriteLine(this, $"Removed member {member.Id}");
                        eventArgs.Add((ClusterMemberLifecycleEventType.Removed, new ClusterMemberLifecycleEventArgs(member)));
                        if (_clients.TryGetValue(member.Id, out var client))
                            client.Shutdown(); // shuts down in the background, will self-removed once down
                        break;

                    case 2: // new but not old = added
                        XConsole.WriteLine(this, $"Added member {member.Id}");
                        eventArgs.Add((ClusterMemberLifecycleEventType.Added, new ClusterMemberLifecycleEventArgs(member)));
                        break;

                    case 3: // old and new = no change
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            // raise events
            foreach (var (eventType, args) in eventArgs)
                OnMemberLifecycleEvent(eventType, args);
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
    }
}
