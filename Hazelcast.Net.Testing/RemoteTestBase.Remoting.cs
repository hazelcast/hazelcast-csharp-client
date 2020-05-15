using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Networking;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods for the <see cref="IRemoteControllerClient"/> interface.
    /// </summary>
    public static class RemoteControllerClientExtensions
    {
        /// <summary>
        /// Creates a new cluster.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <returns>The new cluster.</returns>
        public static Task<Remote.Cluster> CreateClusterAsync(this IRemoteControllerClient rc)
            => rc.CreateClusterAsync(null, Remote.Resources.hazelcast);

        /// <summary>
        /// Creates a new cluster.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="serverConfiguration">The server Xml configuration.</param>
        /// <returns>The new cluster.</returns>
        public static Task<Remote.Cluster> CreateClusterAsync(this IRemoteControllerClient rc, string serverConfiguration)
            => rc.CreateClusterAsync(null, serverConfiguration);

        /// <summary>
        /// Shuts a cluster down.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>Whether the cluster was properly shut down.</returns>
        public static Task<bool> ShutdownClusterAsync(this IRemoteControllerClient rc, Remote.Cluster cluster)
            => rc.ShutdownClusterAsync(cluster.Id);

        /// <summary>
        /// Shuts a cluster down repeatedly until it is down.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>Whether the cluster was properly shut down.</returns>
        public static async Task ShutdownClusterDownAsync(this IRemoteControllerClient rc, Remote.Cluster cluster)
        {
            while (!(await rc.ShutdownClusterAsync(cluster)))
                await Task.Delay(1_000);
        }

        /// <summary>
        /// Starts a new member.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>The new member.</returns>
        public static Task<Member> StartMemberAsync(this IRemoteControllerClient rc, Remote.Cluster cluster)
            => rc.StartMemberAsync(cluster.Id);

        /// <summary>
        /// Starts a new member and wait until it is added.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="client">The Hazelcast client.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="expectedPartitionOwnersCount">The expected number of partition owners.</param>
        /// <returns>The new member.</returns>
        public static async Task<Member> StartMemberAddedAsync(this IRemoteControllerClient rc, IHazelcastClient client, Remote.Cluster cluster, int expectedPartitionOwnersCount)
        {
            var clientInternal = (HazelcastClient) client;
            var added = new SemaphoreSlim(0);
            var partitions = new SemaphoreSlim(0);

            var subscriptionId = await clientInternal.Cluster.SubscribeAsync(on => on
                .MemberAdded((sender, args) =>
                {
                    added.Release();
                })
                .PartitionsUpdated((sender, args) =>
                {
                    partitions.Release();
                }));

            var member = await rc.StartMemberAsync(cluster);
            await added.WaitAsync(TimeSpan.FromSeconds(120));

            // trigger the partition table creation
            var map = await client.GetMapAsync<object, object>("default");
            _ = map.GetAsync(new object());

            await partitions.WaitAsync(TimeSpan.FromSeconds(120));
            await clientInternal.Cluster.UnsubscribeAsync(subscriptionId);

            var partitioner = clientInternal.Cluster.Partitioner;
            var partitionsCount = partitioner.Count;
            var owners = new HashSet<Guid>();
            for (var i = 0; i < partitionsCount; i++)
            {
                var owner = partitioner.GetPartitionOwner(i);
                if (owner != default) owners.Add(owner);
            }

            Assert.AreEqual(expectedPartitionOwnersCount, owners.Count);

            return member;
        }

        /// <summary>
        /// Shuts a member down.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        /// <returns>Whether the member was properly shut down.</returns>
        public static Task<bool> StopMemberAsync(this IRemoteControllerClient rc, Remote.Cluster cluster, Member member)
            => rc.ShutdownMemberAsync(cluster.Id, member.Uuid);

        /// <summary>
        /// Shuts a member down and wait until it is removed.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="client">The Hazelcast client.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        public static async Task StopMemberRemovedAsync(this IRemoteControllerClient rc, IHazelcastClient client, Remote.Cluster cluster, Member member)
        {
            var clientInternal = (HazelcastClient) client;
            var removed = new SemaphoreSlim(0);

            var subscriptionId = await clientInternal.Cluster.SubscribeAsync(on => on
                .MemberRemoved((sender, args) =>
                {
                    removed.Release();
                }));

            await rc.StopMemberAsync(cluster, member);
            await removed.WaitAsync(TimeSpan.FromSeconds(120));
            await clientInternal.Cluster.UnsubscribeAsync(subscriptionId);
        }

        /// <summary>
        /// Suspends a member.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        /// <returns>Whether the member was properly suspended.</returns>
        public static Task<bool> SuspendMemberAsync(IRemoteControllerClient rc, Remote.Cluster cluster, Member member)
            => rc.ShutdownMemberAsync(cluster.Id, member.Uuid);

        /// <summary>
        /// Resumes a member.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        /// <returns>Whether the member was properly resumed.</returns>
        public static Task<bool> ResumeMemberAsync(this IRemoteControllerClient rc, Remote.Cluster cluster, Member member)
            => rc.ResumeMemberAsync(cluster.Id, member.Uuid);
    }

    // partial: remoting
    public abstract partial class RemoteTestBase
    {
        /// <summary>
        /// Creates a remote controller.
        /// </summary>
        /// <returns>A new remote controller.</returns>
        protected async Task<IRemoteControllerClient> CreateRemoteControllerAsync()
        {
            try
            {
#if NETFRAMEWORK
                var transport = new Thrift.Transport.TFramedTransport(new Thrift.Transport.TSocket("localhost", 9701));
                transport.Open();
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                return RemoteControllerClient.Create(protocol);
#else
                var rcHostAddress = NetworkAddress.GetIPAddressByName("localhost");
                var tSocketTransport = new Thrift.Transport.Client.TSocketTransport(rcHostAddress, 9701);
                var transport = new Thrift.Transport.TFramedTransport(tSocketTransport);
                if (!transport.IsOpen)
                {
                    await transport.OpenAsync();
                }
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                return RemoteControllerClient.Create(protocol);
#endif
            }
            catch (Exception e)
            {
                Logger.LogDebug(e, "Cannot start Remote Controller");
                throw new AssertionException("Cannot start Remote Controller", e);
            }
        }
    }
}
