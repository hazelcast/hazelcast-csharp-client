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

namespace Hazelcast.Clustering
{
    // partial: members
    public partial class Cluster
    {
        /// <summary>
        /// Clears the client currently handling cluster events, if it has not changed.
        /// </summary>
        /// <param name="client">The expected client.</param>
        /// <returns>true if the current client matched the expected client, and was cleared; otherwise false.</returns>
        private bool ClearClusterEventsClient(Client client)
            => Interlocked.CompareExchange(ref _clusterEventsClient, null, client) == client;

        /// <summary>
        /// Assigns a new client to handle cluster events.
        /// </summary>
        /// <param name="client">An optional candidate client.</param>
        /// <returns>A task that will complete when a new client has been assigned to handle cluster events.</returns>
        private async Task AssignClusterEventsClient(Client client = null)
        {
            // TODO: log when ending unsuccessfully

            if (client == null) client = GetRandomClient();
            if (client == null) return; // running out of clients, end

            // try to become the new client by replacing the current value, which is expected to be 'null'
            // if it is not 'null', it means that 'someone else' has taken over the assignment and we can end
            if (Interlocked.CompareExchange(ref _clusterEventsClient, client, null) != null)
                return;

            // arbitrarily decide to end after some amount of trying
            int GetMaxAttempts() => _clients.Count * 3;

            var failedAttempts = 0;
            while (!await SubscribeToClusterEvents(client))
            {
                // the client we tried failed to subscribe to events, try another client,
                // but ensure we don't enter some sort of infinite loop by counting attempts
                // TODO: consider using a timeout, instead?
                var nextClient = GetRandomClient();
                if (nextClient == null || ++failedAttempts > GetMaxAttempts())
                {
                    // running out of clients, or tried too many times - end
                    Interlocked.CompareExchange(ref _clusterEventsClient, null, client);
                    return;
                }

                // try to become the new client by replacing the current value, which is expected to be 'client'
                // if it is not 'client', it means that 'someone else' has taken over the assignment and we can end
                if (Interlocked.CompareExchange(ref _clusterEventsClient, nextClient, client) != client)
                    return;

                client = nextClient;
            }

            // success
        }

        /// <summary>
        /// Subscribes a client to cluster events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the subscription has been processed, and represent whether it was successful.</returns>
        private async Task<bool> SubscribeToClusterEvents(Client client) // aka subscribe to member/partition view events?
        {
            XConsole.WriteLine(this, "subscribe");

            // handles the "partition view" event
            void HandlePartitionViewEvent(int version, ICollection<KeyValuePair<Guid, IList<int>>> partitions)
                => Partitioner.NotifyPartitionView(client.Id, version, MapPartitions(partitions));

            // handles the event
            void HandleEvent(ClientMessage message, object state)
                => ClientAddClusterViewListenerCodec.EventHandler.HandleEvent(message, HandleMemberViewEvent, HandlePartitionViewEvent);

            var correlationId = _correlationIdSequence.Next;
            try
            {
                var subscribeRequest = ClientAddClusterViewListenerCodec.EncodeRequest();
                _correlatedSubscriptions[correlationId] = new ClusterEventSubscription(HandleEvent);
                _ = await client.SendAsync(subscribeRequest, correlationId);
                XConsole.WriteLine(this, "subscribed");
                return true;

                // FIXME: but then are we ever going to clear that event handler?
            }
            catch
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                // TODO: at least, log the exception! OR just throw then exception and return Task
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
        /// Handles the "member view" event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        private void HandleMemberViewEvent(int version, ICollection<MemberInfo> members)
        {
            // FIXME: threading

            // notify the load balancer of the new list of members
            _loadBalancer.NotifyMembers(members.Select(x => x.Id));

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

            // process changes, gather events
            var eventArgs = new List<MembershipEventArgs>();
            foreach (var (member, status) in diff)
            {
                switch (status)
                {
                    case 1: // old but not new = removed
                        XConsole.WriteLine(this, $"Removed member {member.Id}");
                        eventArgs.Add(new MembershipEventArgs(MembershipEventType.Removed, member));
                        if (_clients.TryGetValue(member.Id, out var client))
                            client.ShutdownAsync().Wait(); // will self-remove once down FIXME: async oops!!
                        break;

                    case 2: // new but not old = added
                        XConsole.WriteLine(this, $"Added member {member.Id}");
                        eventArgs.Add(new MembershipEventArgs(MembershipEventType.Added, member));
                        break;

                    default: // unchanged
                        break;
                }
            }

            // raise events
            foreach (var args in eventArgs)
            {
                // FIXME: async oops!
                switch (args.EventType)
                {
                    case MembershipEventType.Added:
                        MemberAdded.InvokeAsync(args).AsTask().Wait();
                        break;
                    case MembershipEventType.Removed:
                        MemberRemoved.InvokeAsync(args).AsTask().Wait();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
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
