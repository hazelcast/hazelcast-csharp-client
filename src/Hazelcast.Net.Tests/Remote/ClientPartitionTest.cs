// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Partitioning;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientPartitionTest : SingleMemberClientRemoteTestBase
    {
        [SetUp]
        public async Task Setup()
        {
            await Client.TriggerPartitionTableAsync().CfAwait();
        }

        private static int GetPartitionOwnerCount(Partitioner partitioner)
        {
            var owners = new HashSet<Guid>();
            var count = partitioner.Count;

            for (var i = 0; i < count; i++)
            {
                var owner = partitioner.GetPartitionOwner(i);
                if (owner != default) owners.Add(owner);
            }

            return owners.Count;
        }

        [Test]
        public async Task TestPartitionsUpdatedAfterNewNode()
        {
            var cluster = ((HazelcastClient) Client).Cluster;

            var partitionCount = cluster.Partitioner.Count;
            Assert.AreEqual(271, partitionCount);

            // 1 member = 1 partition owner
            // partition table must update eventually
            await AssertEx.SucceedsEventually(() =>
                {
                    Assert.That(GetPartitionOwnerCount(cluster.Partitioner), Is.EqualTo(1));
                },
                4000, 500);

            // add a second member
            // make sure we wait for it to be added - otherwise we get our 2 partitions
            // soon enough and then we stop the member before all migrations have run,
            // and that confuses the server which then does weird things and never fully
            // remove the second member, thus breaking the test.
            var member2 = await RcClient.StartMemberWaitAddedAsync(Client, RcCluster, 2);

            try
            {
                // partition table must update eventually
                await AssertEx.SucceedsEventually(() =>
                    {
                        Assert.That(GetPartitionOwnerCount(cluster.Partitioner), Is.EqualTo(2));
                    },
                    4000, 500);
            }
            finally
            {
                // whatever happens, make sure the member is stopped
                await RcClient.StopMemberWaitRemovedAsync(Client, RcCluster, member2);
            }

            // partition table must update eventually
            await AssertEx.SucceedsEventually(() =>
                {
                    Assert.That(GetPartitionOwnerCount(cluster.Partitioner), Is.EqualTo(1));
                },
                20000, 500);
        }
    }
}
