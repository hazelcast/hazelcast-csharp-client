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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing.Remote;
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
        public static Task<Cluster> CreateClusterAsync(this IRemoteControllerClient rc)
            => rc.CreateClusterAsync(null, Resources.hazelcast);

        /// <summary>
        /// Creates a new cluster.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="serverConfiguration">The server Xml configuration.</param>
        /// <returns>The new cluster.</returns>
        public static Task<Cluster> CreateClusterAsync(this IRemoteControllerClient rc, string serverConfiguration)
            => rc.CreateClusterAsync(null, serverConfiguration);

        /// <summary>
        /// Shuts a cluster down.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>Whether the cluster was properly shut down.</returns>
        public static Task<bool> ShutdownClusterAsync(this IRemoteControllerClient rc, Cluster cluster)
            => rc.ShutdownClusterAsync(cluster.Id);

        /// <summary>
        /// Shuts a cluster down repeatedly until it is down.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>Whether the cluster was properly shut down.</returns>
        public static async Task ShutdownClusterDownAsync(this IRemoteControllerClient rc, Cluster cluster)
        {
            while (!await rc.ShutdownClusterAsync(cluster).CfAwait())
                await Task.Delay(1_000).CfAwait();
        }

        /// <summary>
        /// Starts a new member.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>The new member.</returns>
        public static Task<Member> StartMemberAsync(this IRemoteControllerClient rc, Cluster cluster)
            => rc.StartMemberAsync(cluster.Id);

        /// <summary>
        /// Starts a new member and wait until it is added.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="client">The Hazelcast client.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="expectedPartitionOwnersCount">The expected number of partition owners.</param>
        /// <returns>The new member.</returns>
        public static async Task<Member> StartMemberWaitAddedAsync(this IRemoteControllerClient rc, IHazelcastClient client, Cluster cluster, int expectedPartitionOwnersCount)
        {
            var clientInternal = (HazelcastClient) client;
            var added = new SemaphoreSlim(0);
            var partitions = new SemaphoreSlim(0);

            var subscriptionId = await clientInternal.SubscribeAsync(on => on
                    .MembersUpdated((sender, args) =>
                    {
                        if (args.AddedMembers.Count > 0) added.Release();
                    })
                    .PartitionsUpdated((sender, args) =>
                    {
                        partitions.Release();
                    }))
                .CfAwait();

            var member = await rc.StartMemberAsync(cluster).CfAwait();
            await added.WaitAsync(TimeSpan.FromSeconds(120)).CfAwait();

            // trigger the partition table creation
            var map = await client.GetMapAsync<object, object>("default").CfAwait();
            _ = map.GetAsync(new object());

            await partitions.WaitAsync(TimeSpan.FromSeconds(120)).CfAwait();
            await clientInternal.UnsubscribeAsync(subscriptionId).CfAwait();

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
        public static Task<bool> StopMemberAsync(this IRemoteControllerClient rc, Cluster cluster, Member member)
            => rc.ShutdownMemberAsync(cluster.Id, member.Uuid);

        /// <summary>
        /// Shuts a member down and wait until it is removed.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="client">The Hazelcast client.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        public static async Task StopMemberWaitRemovedAsync(this IRemoteControllerClient rc, IHazelcastClient client, Cluster cluster, Member member)
        {
            var clientInternal = (HazelcastClient) client;
            var removed = new SemaphoreSlim(0);

            var subscriptionId = await clientInternal.SubscribeAsync(on => on
                    .MembersUpdated((sender, args) =>
                    {
                        if (args.RemovedMembers.Count > 0) removed.Release();
                    }))
                .CfAwait();

            await rc.StopMemberAsync(cluster, member).CfAwait();
            await removed.WaitAsync(TimeSpan.FromSeconds(120)).CfAwait();
            await clientInternal.UnsubscribeAsync(subscriptionId).CfAwait();
        }

        /// <summary>
        /// Suspends a member.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        /// <returns>Whether the member was properly suspended.</returns>
        public static Task<bool> SuspendMemberAsync(IRemoteControllerClient rc, Cluster cluster, Member member)
            => rc.ShutdownMemberAsync(cluster.Id, member.Uuid);

        /// <summary>
        /// Resumes a member.
        /// </summary>
        /// <param name="rc">The remote controller.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="member">The member.</param>
        /// <returns>Whether the member was properly resumed.</returns>
        public static Task<bool> ResumeMemberAsync(this IRemoteControllerClient rc, Cluster cluster, Member member)
            => rc.ResumeMemberAsync(cluster.Id, member.Uuid);
    }
}
